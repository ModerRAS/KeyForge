using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.Performance
{
    /// <summary>
    /// 负载测试工具类
    /// 模拟大量并发用户操作
    /// </summary>
    public class LoadTestRunner
    {
        private readonly int _concurrentUsers;
        private readonly int _testDurationSeconds;
        private readonly Action<LoadTestContext> _testAction;
        private readonly List<LoadTestResult> _results;

        public LoadTestRunner(int concurrentUsers, int testDurationSeconds, Action<LoadTestContext> testAction)
        {
            _concurrentUsers = concurrentUsers;
            _testDurationSeconds = testDurationSeconds;
            _testAction = testAction;
            _results = new List<LoadTestResult>();
        }

        public async Task<LoadTestSummary> RunLoadTestAsync()
        {
            Console.WriteLine($"开始负载测试: {_concurrentUsers} 并发用户, {_testDurationSeconds} 秒持续时间");
            
            var cts = new CancellationTokenSource();
            var tasks = new List<Task>();
            
            // 创建并发用户任务
            for (int i = 0; i < _concurrentUsers; i++)
            {
                var userId = i + 1;
                tasks.Add(Task.Run(() => SimulateUser(userId, cts.Token)));
            }

            // 运行测试指定时间
            cts.CancelAfter(TimeSpan.FromSeconds(_testDurationSeconds));
            
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // 测试正常结束
            }

            return GenerateSummary();
        }

        private async Task SimulateUser(int userId, CancellationToken cancellationToken)
        {
            var userResults = new List<LoadTestResult>();
            var startTime = DateTime.UtcNow;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var requestStartTime = DateTime.UtcNow;
                var context = new LoadTestContext
                {
                    UserId = userId,
                    StartTime = requestStartTime,
                    Iteration = userResults.Count + 1
                };

                try
                {
                    _testAction(context);
                    context.EndTime = DateTime.UtcNow;
                    context.Success = true;
                }
                catch (Exception ex)
                {
                    context.EndTime = DateTime.UtcNow;
                    context.Success = false;
                    context.ErrorMessage = ex.Message;
                }

                var result = new LoadTestResult
                {
                    UserId = userId,
                    StartTime = context.StartTime,
                    EndTime = context.EndTime,
                    Duration = context.EndTime - context.StartTime,
                    Success = context.Success,
                    ErrorMessage = context.ErrorMessage,
                    Iteration = context.Iteration
                };

                userResults.Add(result);
                
                // 模拟用户思考时间
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(100, 500)), cancellationToken);
            }

            lock (_results)
            {
                _results.AddRange(userResults);
            }
        }

        private LoadTestSummary GenerateSummary()
        {
            if (!_results.Any())
            {
                return new LoadTestSummary();
            }

            var successfulRequests = _results.Where(r => r.Success).ToList();
            var failedRequests = _results.Where(r => !r.Success).ToList();

            return new LoadTestSummary
            {
                TotalRequests = _results.Count,
                SuccessfulRequests = successfulRequests.Count,
                FailedRequests = failedRequests.Count,
                SuccessRate = (double)successfulRequests.Count / _results.Count * 100,
                AverageResponseTime = successfulRequests.Any() 
                    ? successfulRequests.Average(r => r.Duration.TotalMilliseconds) 
                    : 0,
                MinResponseTime = successfulRequests.Any() 
                    ? successfulRequests.Min(r => r.Duration.TotalMilliseconds) 
                    : 0,
                MaxResponseTime = successfulRequests.Any() 
                    ? successfulRequests.Max(r => r.Duration.TotalMilliseconds) 
                    : 0,
                RequestsPerSecond = _results.Count / _testDurationSeconds,
                ConcurrentUsers = _concurrentUsers,
                TestDuration = TimeSpan.FromSeconds(_testDurationSeconds),
                ErrorMessages = failedRequests.Select(f => f.ErrorMessage).Distinct().ToList()
            };
        }
    }

    /// <summary>
    /// 负载测试上下文
    /// </summary>
    public class LoadTestContext
    {
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Iteration { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 负载测试结果
    /// </summary>
    public class LoadTestResult
    {
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int Iteration { get; set; }
    }

    /// <summary>
    /// 负载测试摘要
    /// </summary>
    public class LoadTestSummary
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public double RequestsPerSecond { get; set; }
        public int ConcurrentUsers { get; set; }
        public TimeSpan TestDuration { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();

        public void PrintSummary()
        {
            Console.WriteLine("\n=== 负载测试结果 ===");
            Console.WriteLine($"总请求数: {TotalRequests}");
            Console.WriteLine($"成功请求数: {SuccessfulRequests}");
            Console.WriteLine($"失败请求数: {FailedRequests}");
            Console.WriteLine($"成功率: {SuccessRate:F2}%");
            Console.WriteLine($"平均响应时间: {AverageResponseTime:F2}ms");
            Console.WriteLine($"最小响应时间: {MinResponseTime:F2}ms");
            Console.WriteLine($"最大响应时间: {MaxResponseTime:F2}ms");
            Console.WriteLine($"每秒请求数: {RequestsPerSecond:F2}");
            Console.WriteLine($"并发用户数: {ConcurrentUsers}");
            Console.WriteLine($"测试持续时间: {TestDuration}");

            if (ErrorMessages.Any())
            {
                Console.WriteLine("\n错误信息:");
                foreach (var error in ErrorMessages)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
        }
    }

    /// <summary>
    /// 脚本操作负载测试
    /// </summary>
    public class ScriptLoadTests
    {
        public static async Task RunScriptCreationLoadTest()
        {
            Console.WriteLine("开始脚本创建负载测试...");

            var loadTest = new LoadTestRunner(
                concurrentUsers: 50,
                testDurationSeconds: 30,
                testAction: context =>
                {
                    // 模拟脚本创建操作
                    var script = new Script(
                        new ScriptName($"Load Test Script {context.UserId}-{context.Iteration}"),
                        new ScriptDescription($"Created by user {context.UserId} at iteration {context.Iteration}")
                    );
                    
                    // 添加一些动作
                    for (int i = 0; i < 5; i++)
                    {
                        var action = TestDataFactory.CreateGameAction();
                        script.AddAction(action);
                    }
                    
                    // 模拟保存操作
                    if (context.Iteration % 10 == 0)
                    {
                        // 模拟偶尔的保存失败
                        throw new Exception("模拟保存失败");
                    }
                }
            );

            var summary = await loadTest.RunLoadTestAsync();
            summary.PrintSummary();
        }

        public static async Task RunScriptExecutionLoadTest()
        {
            Console.WriteLine("开始脚本执行负载测试...");

            // 创建测试脚本
            var testScript = TestDataFactory.CreateValidScript();
            for (int i = 0; i < 100; i++)
            {
                var action = TestDataFactory.CreateGameAction();
                testScript.AddAction(action);
            }

            var loadTest = new LoadTestRunner(
                concurrentUsers: 30,
                testDurationSeconds: 20,
                testAction: context =>
                {
                    // 模拟脚本执行操作
                    var executionTime = new Random().Next(10, 100);
                    
                    // 模拟执行每个动作
                    foreach (var action in testScript.Actions.Take(20))
                    {
                        // 模拟动作执行
                        var actionExecutionTime = new Random().Next(1, 10);
                        Task.Delay(actionExecutionTime).Wait();
                    }
                    
                    // 模拟偶尔的执行失败
                    if (context.Iteration % 15 == 0)
                    {
                        throw new Exception("模拟执行失败");
                    }
                }
            );

            var summary = await loadTest.RunLoadTestAsync();
            summary.PrintSummary();
        }

        public static async Task RunScriptQueryLoadTest()
        {
            Console.WriteLine("开始脚本查询负载测试...");

            // 创建测试数据
            var testScripts = new List<Script>();
            for (int i = 0; i < 1000; i++)
            {
                var script = TestDataFactory.CreateValidScript();
                testScripts.Add(script);
            }

            var loadTest = new LoadTestRunner(
                concurrentUsers: 100,
                testDurationSeconds: 15,
                testAction: context =>
                {
                    // 模拟查询操作
                    var queryType = new Random().Next(1, 4);
                    
                    switch (queryType)
                    {
                        case 1:
                            // 按ID查询
                            var scriptId = testScripts[new Random().Next(testScripts.Count)].Id;
                            var foundScript = testScripts.FirstOrDefault(s => s.Id == scriptId);
                            break;
                            
                        case 2:
                            // 按状态查询
                            var status = (ScriptStatus)new Random().Next(0, 4);
                            var scriptsByStatus = testScripts.Where(s => s.Status == status).ToList();
                            break;
                            
                        case 3:
                            // 获取所有脚本
                            var allScripts = testScripts.ToList();
                            break;
                    }
                    
                    // 模拟查询延迟
                    Task.Delay(new Random().Next(1, 5)).Wait();
                }
            );

            var summary = await loadTest.RunLoadTestAsync();
            summary.PrintSummary();
        }
    }

    /// <summary>
    /// 状态机操作负载测试
    /// </summary>
    public class StateMachineLoadTests
    {
        public static async Task RunStateMachineCreationLoadTest()
        {
            Console.WriteLine("开始状态机创建负载测试...");

            var loadTest = new LoadTestRunner(
                concurrentUsers: 20,
                testDurationSeconds: 25,
                testAction: context =>
                {
                    // 模拟状态机创建操作
                    var stateMachine = new StateMachine(
                        new StateMachineName($"Load Test StateMachine {context.UserId}-{context.Iteration}"),
                        new StateMachineDescription($"Created by user {context.UserId}")
                    );
                    
                    // 添加状态
                    for (int i = 0; i < 5; i++)
                    {
                        var state = new State($"State_{i}", $"State {i} description");
                        stateMachine.AddState(state);
                    }
                    
                    // 添加转换
                    for (int i = 0; i < 4; i++)
                    {
                        var fromState = new State($"State_{i}", $"State {i}");
                        var toState = new State($"State_{i + 1}", $"State {i + 1}");
                        var condition = new StateCondition($"condition_{i}", "true");
                        stateMachine.AddTransition(fromState, toState, condition);
                    }
                }
            );

            var summary = await loadTest.RunLoadTestAsync();
            summary.PrintSummary();
        }

        public static async Task RunStateMachineExecutionLoadTest()
        {
            Console.WriteLine("开始状态机执行负载测试...");

            // 创建测试状态机
            var testStateMachine = TestDataFactory.CreateValidStateMachine();
            
            // 添加更多状态和转换
            for (int i = 0; i < 10; i++)
            {
                var state = new State($"State_{i}", $"State {i}");
                testStateMachine.AddState(state);
            }

            var loadTest = new LoadTestRunner(
                concurrentUsers: 15,
                testDurationSeconds: 20,
                testAction: context =>
                {
                    // 模拟状态机执行操作
                    var currentState = testStateMachine.States.First();
                    var steps = 0;
                    
                    while (steps < 20)
                    {
                        // 模拟状态转换
                        var availableTransitions = testStateMachine.Transitions
                            .Where(t => t.FromState == currentState)
                            .ToList();
                        
                        if (availableTransitions.Any())
                        {
                            var nextTransition = availableTransitions[new Random().Next(availableTransitions.Count)];
                            currentState = nextTransition.ToState;
                            steps++;
                        }
                        else
                        {
                            break;
                        }
                        
                        // 模拟处理时间
                        Task.Delay(new Random().Next(1, 10)).Wait();
                    }
                }
            );

            var summary = await loadTest.RunLoadTestAsync();
            summary.PrintSummary();
        }
    }

    /// <summary>
    /// 综合负载测试运行器
    /// </summary>
    public class ComprehensiveLoadTestRunner
    {
        public static async Task RunAllLoadTests()
        {
            Console.WriteLine("开始综合负载测试...");
            Console.WriteLine("=====================================");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 运行各种负载测试
            await ScriptLoadTests.RunScriptCreationLoadTest();
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            await ScriptLoadTests.RunScriptExecutionLoadTest();
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            await ScriptLoadTests.RunScriptQueryLoadTest();
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            await StateMachineLoadTests.RunStateMachineCreationLoadTest();
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            await StateMachineLoadTests.RunStateMachineExecutionLoadTest();

            stopwatch.Stop();
            
            Console.WriteLine("\n=====================================");
            Console.WriteLine($"所有负载测试完成，总耗时: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
        }
    }
}