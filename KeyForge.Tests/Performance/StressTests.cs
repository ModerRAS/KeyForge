using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.Performance
{
    /// <summary>
    /// 压力测试类
    /// 测试系统在极限条件下的表现
    /// </summary>
    public class StressTests
    {
        [Fact]
        public void HighFrequencyScriptCreation_ShouldHandleLoad()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var createdScripts = new List<Script>();
            var maxDuration = TimeSpan.FromSeconds(30);
            var creationCount = 0;
            var errorCount = 0;

            // Act
            stopwatch.Start();
            var endTime = DateTime.Now.Add(maxDuration);

            while (DateTime.Now < endTime)
            {
                try
                {
                    var script = new Script(
                        new ScriptName($"Stress Test Script {creationCount}"),
                        new ScriptDescription($"Created during stress test")
                    );
                    
                    // 添加多个动作
                    for (int i = 0; i < 10; i++)
                    {
                        var action = TestDataFactory.CreateGameAction();
                        script.AddAction(action);
                    }
                    
                    createdScripts.Add(script);
                    creationCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"脚本创建失败: {ex.Message}");
                }
            }

            stopwatch.Stop();

            // Assert
            var creationRate = creationCount / maxDuration.TotalSeconds;
            var errorRate = (double)errorCount / creationCount * 100;

            Console.WriteLine($"高频脚本创建测试结果:");
            Console.WriteLine($"  创建脚本数: {creationCount}");
            Console.WriteLine($"  错误数: {errorCount}");
            Console.WriteLine($"  创建速率: {creationRate:F2} 脚本/秒");
            Console.WriteLine($"  错误率: {errorRate:F2}%");
            Console.WriteLine($"  平均每个脚本创建时间: {stopwatch.Elapsed.TotalMilliseconds / creationCount:F2} ms");

            Assert.True(creationRate > 50, "脚本创建速率过低"); // 至少50个脚本/秒
            Assert.True(errorRate < 1, "错误率过高"); // 错误率小于1%
            Assert.True(stopwatch.Elapsed.TotalMilliseconds / creationCount < 20, "平均创建时间过长"); // 平均创建时间小于20ms
        }

        [Fact]
        public void MassiveScriptExecution_ShouldHandleLoad()
        {
            // Arrange
            var testScript = new Script(
                new ScriptName("Massive Execution Test Script"),
                new ScriptDescription("Script for massive execution testing")
            );
            
            // 添加大量动作
            for (int i = 0; i < 10000; i++)
            {
                var action = TestDataFactory.CreateGameAction();
                testScript.AddAction(action);
            }

            var stopwatch = new Stopwatch();
            var executionCount = 0;
            var maxExecutions = 100;
            var errorCount = 0;

            // Act
            stopwatch.Start();

            for (int i = 0; i < maxExecutions; i++)
            {
                try
                {
                    // 模拟脚本执行
                    foreach (var action in testScript.Actions)
                    {
                        // 模拟动作执行
                        Thread.Sleep(1); // 模拟1ms的执行时间
                    }
                    
                    executionCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"脚本执行失败: {ex.Message}");
                }
            }

            stopwatch.Stop();

            // Assert
            var averageExecutionTime = stopwatch.Elapsed.TotalMilliseconds / executionCount;
            var actionsPerSecond = (testScript.Actions.Count * executionCount) / stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"大规模脚本执行测试结果:");
            Console.WriteLine($"  执行次数: {executionCount}");
            Console.WriteLine($"  错误数: {errorCount}");
            Console.WriteLine($"  总执行时间: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            Console.WriteLine($"  平均执行时间: {averageExecutionTime:F2} ms");
            Console.WriteLine($"  动作执行速率: {actionsPerSecond:F2} 动作/秒");

            Assert.True(executionCount == maxExecutions, "执行次数不足");
            Assert.True(errorCount == 0, "存在执行错误");
            Assert.True(actionsPerSecond > 1000, "动作执行速率过低"); // 至少1000个动作/秒
        }

        [Fact]
        public void ConcurrentStateTransitions_ShouldHandleLoad()
        {
            // Arrange
            var stateMachine = new StateMachine(
                new StateMachineName("Concurrent State Machine"),
                new StateMachineDescription("State machine for concurrent testing")
            );
            
            // 添加多个状态
            for (int i = 0; i < 50; i++)
            {
                var state = new State($"State_{i}", $"State {i}");
                stateMachine.AddState(state);
            }
            
            // 添加转换
            for (int i = 0; i < 49; i++)
            {
                var fromState = new State($"State_{i}", $"State {i}");
                var toState = new State($"State_{i + 1}", $"State {i + 1}");
                var condition = new StateCondition($"condition_{i}", "true");
                stateMachine.AddTransition(fromState, toState, condition);
            }

            var concurrentUsers = 20;
            var transitionsPerUser = 100;
            var tasks = new List<Task>();
            var successfulTransitions = 0;
            var failedTransitions = 0;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var currentState = stateMachine.States.First();
                    var userTransitions = 0;
                    
                    while (userTransitions < transitionsPerUser)
                    {
                        try
                        {
                            var availableTransitions = stateMachine.Transitions
                                .Where(t => t.FromState == currentState)
                                .ToList();
                            
                            if (availableTransitions.Any())
                            {
                                var nextTransition = availableTransitions[new Random().Next(availableTransitions.Count)];
                                currentState = nextTransition.ToState;
                                
                                Interlocked.Increment(ref successfulTransitions);
                            }
                            else
                            {
                                // 如果没有可用转换，重置到初始状态
                                currentState = stateMachine.States.First();
                            }
                            
                            userTransitions++;
                            
                            // 模拟处理时间
                            Thread.Sleep(new Random().Next(1, 10));
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref failedTransitions);
                            Console.WriteLine($"状态转换失败: {ex.Message}");
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            // Assert
            var totalTransitions = successfulTransitions + failedTransitions;
            var successRate = (double)successfulTransitions / totalTransitions * 100;
            var transitionsPerSecond = totalTransitions / stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"并发状态转换测试结果:");
            Console.WriteLine($"  总转换数: {totalTransitions}");
            Console.WriteLine($"  成功转换数: {successfulTransitions}");
            Console.WriteLine($"  失败转换数: {failedTransitions}");
            Console.WriteLine($"  成功率: {successRate:F2}%");
            Console.WriteLine($"  转换速率: {transitionsPerSecond:F2} 转换/秒");
            Console.WriteLine($"  总执行时间: {stopwatch.Elapsed.TotalSeconds:F2} 秒");

            Assert.True(successRate > 99, "状态转换成功率过低"); // 成功率大于99%
            Assert.True(transitionsPerSecond > 100, "状态转换速率过低"); // 至少100个转换/秒
        }

        [Fact]
        public void MemoryStressTest_ShouldNotLeak()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var maxIterations = 10000;
            var memorySamples = new List<long>();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            for (int i = 0; i < maxIterations; i++)
            {
                // 创建大量对象
                var script = new Script(
                    new ScriptName($"Memory Stress Script {i}"),
                    new ScriptDescription($"Memory stress test iteration {i}")
                );
                
                for (int j = 0; j < 100; j++)
                {
                    var action = TestDataFactory.CreateGameAction();
                    script.AddAction(action);
                }
                
                var stateMachine = new StateMachine(
                    new StateMachineName($"Memory Stress StateMachine {i}"),
                    new StateMachineDescription($"Memory stress test iteration {i}")
                );
                
                for (int j = 0; j < 20; j++)
                {
                    var state = new State($"State_{i}_{j}", $"State {j}");
                    stateMachine.AddState(state);
                }
                
                // 定期记录内存使用情况
                if (i % 100 == 0)
                {
                    var currentMemory = GC.GetTotalMemory(false);
                    memorySamples.Add(currentMemory);
                    
                    // 定期强制垃圾回收
                    if (i % 500 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
                
                // 对象在这里应该被垃圾回收
            }

            stopwatch.Stop();
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            var finalMemory = GC.GetTotalMemory(true);
            var memoryGrowth = finalMemory - initialMemory;

            // Assert
            var averageMemory = memorySamples.Average();
            var maxMemory = memorySamples.Max();
            var memoryVariance = memorySamples.Select(m => Math.Pow(m - averageMemory, 2)).Average();
            var standardDeviation = Math.Sqrt(memoryVariance);

            Console.WriteLine($"内存压力测试结果:");
            Console.WriteLine($"  迭代次数: {maxIterations}");
            Console.WriteLine($"  执行时间: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            Console.WriteLine($"  初始内存: {initialMemory / 1024 / 1024:F2} MB");
            Console.WriteLine($"  最终内存: {finalMemory / 1024 / 1024:F2} MB");
            Console.WriteLine($"  内存增长: {memoryGrowth / 1024 / 1024:F2} MB");
            Console.WriteLine($"  平均内存使用: {averageMemory / 1024 / 1024:F2} MB");
            Console.WriteLine($"  最大内存使用: {maxMemory / 1024 / 1024:F2} MB");
            Console.WriteLine($"  内存使用标准差: {standardDeviation / 1024 / 1024:F2} MB");

            Assert.True(memoryGrowth < 10 * 1024 * 1024, "内存泄漏检测到"); // 内存增长小于10MB
            Assert.True(standardDeviation < 2 * 1024 * 1024, "内存使用波动过大"); // 标准差小于2MB
        }

        [Fact]
        public void HighFrequencyDatabaseOperations_ShouldHandleLoad()
        {
            // Arrange
            var operationCount = 0;
            var errorCount = 0;
            var maxDuration = TimeSpan.FromSeconds(30);
            var stopwatch = new Stopwatch();
            var operations = new List<Func<int>>();

            // 创建大量数据库操作
            for (int i = 0; i < 1000; i++)
            {
                var scriptId = i;
                operations.Add(() =>
                {
                    try
                    {
                        // 模拟数据库操作
                        var script = new Script(
                            new ScriptName($"DB Test Script {scriptId}"),
                            new ScriptDescription($"Database test script {scriptId}")
                        );
                        
                        // 模拟保存到数据库
                        Thread.Sleep(new Random().Next(1, 5)); // 模拟数据库延迟
                        
                        return 1; // 成功
                    }
                    catch
                    {
                        return 0; // 失败
                    }
                });
            }

            // Act
            stopwatch.Start();
            var endTime = DateTime.Now.Add(maxDuration);

            while (DateTime.Now < endTime && operationCount < operations.Count)
            {
                var result = operations[operationCount]();
                operationCount++;
                
                if (result == 0)
                {
                    errorCount++;
                }
            }

            stopwatch.Stop();

            // Assert
            var operationRate = operationCount / maxDuration.TotalSeconds;
            var errorRate = (double)errorCount / operationCount * 100;

            Console.WriteLine($"高频数据库操作测试结果:");
            Console.WriteLine($"  执行操作数: {operationCount}");
            Console.WriteLine($"  错误数: {errorCount}");
            Console.WriteLine($"  操作速率: {operationRate:F2} 操作/秒");
            Console.WriteLine($"  错误率: {errorRate:F2}%");
            Console.WriteLine($"  平均操作时间: {stopwatch.Elapsed.TotalMilliseconds / operationCount:F2} ms");

            Assert.True(operationRate > 100, "数据库操作速率过低"); // 至少100个操作/秒
            Assert.True(errorRate < 5, "错误率过高"); // 错误率小于5%
        }

        [Fact]
        public void ExtremeConcurrencyTest_ShouldHandleLoad()
        {
            // Arrange
            var maxConcurrentTasks = 100;
            var operationsPerTask = 50;
            var tasks = new List<Task>();
            var successfulOperations = 0;
            var failedOperations = 0;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            for (int i = 0; i < maxConcurrentTasks; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerTask; j++)
                    {
                        try
                        {
                            // 模拟复杂操作
                            var script = new Script(
                                new ScriptName($"Extreme Concurrency Script {i}_{j}"),
                                new ScriptDescription($"Extreme concurrency test")
                            );
                            
                            for (int k = 0; k < 20; k++)
                            {
                                var action = TestDataFactory.CreateGameAction();
                                script.AddAction(action);
                            }
                            
                            var stateMachine = new StateMachine(
                                new StateMachineName($"Extreme Concurrency StateMachine {i}_{j}"),
                                new StateMachineDescription($"Extreme concurrency test")
                            );
                            
                            for (int k = 0; k < 10; k++)
                            {
                                var state = new State($"State_{i}_{j}_{k}", $"State {k}");
                                stateMachine.AddState(state);
                            }
                            
                            // 模拟处理时间
                            Thread.Sleep(new Random().Next(1, 10));
                            
                            Interlocked.Increment(ref successfulOperations);
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref failedOperations);
                            Console.WriteLine($"极端并发操作失败: {ex.Message}");
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            // Assert
            var totalOperations = successfulOperations + failedOperations;
            var successRate = (double)successfulOperations / totalOperations * 100;
            var operationsPerSecond = totalOperations / stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"极端并发测试结果:");
            Console.WriteLine($"  总操作数: {totalOperations}");
            Console.WriteLine($"  成功操作数: {successfulOperations}");
            Console.WriteLine($"  失败操作数: {failedOperations}");
            Console.WriteLine($"  成功率: {successRate:F2}%");
            Console.WriteLine($"  操作速率: {operationsPerSecond:F2} 操作/秒");
            Console.WriteLine($"  总执行时间: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            Console.WriteLine($"  并发任务数: {maxConcurrentTasks}");

            Assert.True(successRate > 95, "极端并发成功率过低"); // 成功率大于95%
            Assert.True(operationsPerSecond > 500, "极端并发操作速率过低"); // 至少500个操作/秒
        }
    }

    /// <summary>
    /// 压力测试运行器
    /// </summary>
    public class StressTestRunner
    {
        public static void RunAllStressTests()
        {
            Console.WriteLine("开始压力测试...");
            Console.WriteLine("=====================================");
            
            var tests = new List<Action>
            {
                () => new StressTests().HighFrequencyScriptCreation_ShouldHandleLoad(),
                () => new StressTests().MassiveScriptExecution_ShouldHandleLoad(),
                () => new StressTests().ConcurrentStateTransitions_ShouldHandleLoad(),
                () => new StressTests().MemoryStressTest_ShouldNotLeak(),
                () => new StressTests().HighFrequencyDatabaseOperations_ShouldHandleLoad(),
                () => new StressTests().ExtremeConcurrencyTest_ShouldHandleLoad()
            };

            var totalStopwatch = Stopwatch.StartNew();

            foreach (var test in tests)
            {
                try
                {
                    Console.WriteLine($"运行测试: {test.Method.Name}");
                    var testStopwatch = Stopwatch.StartNew();
                    
                    test();
                    
                    testStopwatch.Stop();
                    Console.WriteLine($"测试完成，耗时: {testStopwatch.Elapsed.TotalSeconds:F2} 秒");
                    Console.WriteLine(new string('-', 50));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"测试 {test.Method.Name} 失败: {ex.Message}");
                    Console.WriteLine(new string('-', 50));
                }
            }

            totalStopwatch.Stop();
            
            Console.WriteLine("=====================================");
            Console.WriteLine($"所有压力测试完成，总耗时: {totalStopwatch.Elapsed.TotalSeconds:F2} 秒");
        }
    }
}