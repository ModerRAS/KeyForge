using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.Performance
{
    /// <summary>
    /// 性能基准测试
    /// 测试领域模型的核心操作性能
    /// </summary>
    [MemoryDiagnoser]
    public class DomainPerformanceBenchmarks
    {
        private Script _testScript;
        private List<GameAction> _testActions;
        private StateMachine _testStateMachine;

        [GlobalSetup]
        public void Setup()
        {
            _testScript = TestDataFactory.CreateValidScript();
            _testActions = new List<GameAction>();
            
            // 创建大量测试动作
            for (int i = 0; i < 1000; i++)
            {
                _testActions.Add(TestDataFactory.CreateGameAction());
            }
            
            _testStateMachine = TestDataFactory.CreateValidStateMachine();
        }

        [Benchmark]
        public void ScriptCreation()
        {
            var script = new Script(
                new ScriptName($"Test Script {Guid.NewGuid()}"),
                new ScriptDescription("Performance test script")
            );
        }

        [Benchmark]
        public void ScriptAddAction()
        {
            var script = new Script(
                new ScriptName($"Test Script {Guid.NewGuid()}"),
                new ScriptDescription("Performance test script")
            );
            
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);
        }

        [Benchmark]
        public void ScriptAddMultipleActions()
        {
            var script = new Script(
                new ScriptName($"Test Script {Guid.NewGuid()}"),
                new ScriptDescription("Performance test script")
            );
            
            for (int i = 0; i < 100; i++)
            {
                var action = TestDataFactory.CreateGameAction();
                script.AddAction(action);
            }
        }

        [Benchmark]
        public void StateMachineCreation()
        {
            var stateMachine = new StateMachine(
                new StateMachineName($"Test StateMachine {Guid.NewGuid()}"),
                new StateMachineDescription("Performance test state machine")
            );
        }

        [Benchmark]
        public void StateMachineAddState()
        {
            var state = new State($"State_{Guid.NewGuid()}", "Test state");
            _testStateMachine.AddState(state);
        }

        [Benchmark]
        public void StateMachineAddTransition()
        {
            var fromState = new State($"From_{Guid.NewGuid()}", "From state");
            var toState = new State($"To_{Guid.NewGuid()}", "To state");
            var condition = new StateCondition("test_condition", "true");
            
            _testStateMachine.AddState(fromState);
            _testStateMachine.AddState(toState);
            _testStateMachine.AddTransition(fromState, toState, condition);
        }

        [Benchmark]
        public void ValueObjectCreation()
        {
            var scriptName = new ScriptName($"Script {Guid.NewGuid()}");
            var scriptDescription = new ScriptDescription($"Description {Guid.NewGuid()}");
            var scriptId = new ScriptId(Guid.NewGuid());
            var timestamp = new Timestamp(DateTime.UtcNow);
        }

        [Benchmark]
        public void LargeScriptSerialization()
        {
            // 创建包含大量动作的脚本
            var largeScript = new Script(
                new ScriptName($"Large Script {Guid.NewGuid()}"),
                new ScriptDescription("Large script for performance testing")
            );
            
            for (int i = 0; i < 500; i++)
            {
                var action = TestDataFactory.CreateGameAction();
                largeScript.AddAction(action);
            }
            
            // 模拟序列化操作
            var scriptData = new
            {
                Id = largeScript.Id.Value,
                Name = largeScript.Name.Value,
                Description = largeScript.Description.Value,
                Status = largeScript.Status.ToString(),
                ActionCount = largeScript.Actions.Count
            };
        }

        [Benchmark]
        public void ScriptExecutionSimulation()
        {
            // 模拟脚本执行的性能测试
            var executionContext = new
            {
                StartTime = DateTime.UtcNow,
                ScriptId = _testScript.Id.Value,
                TotalActions = _testScript.Actions.Count,
                ExecutedActions = 0
            };
            
            // 模拟执行所有动作
            foreach (var action in _testScript.Actions)
            {
                executionContext.ExecutedActions++;
                
                // 模拟动作执行
                var actionResult = new
                {
                    ActionId = action.Id.Value,
                    ActionType = action.GetType().Name,
                    ExecutionTime = DateTime.UtcNow,
                    Success = true
                };
            }
        }

        [Benchmark]
        public void ConcurrentScriptOperations()
        {
            var tasks = new List<Task>();
            
            // 创建多个并发任务
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var script = new Script(
                        new ScriptName($"Concurrent Script {Guid.NewGuid()}"),
                        new ScriptDescription("Concurrent operation test")
                    );
                    
                    for (int j = 0; j < 50; j++)
                    {
                        var action = TestDataFactory.CreateGameAction();
                        script.AddAction(action);
                    }
                }));
            }
            
            Task.WaitAll(tasks.ToArray());
        }

        [Benchmark]
        public void MemoryAllocationTest()
        {
            var scripts = new List<Script>();
            
            // 测试大量对象创建的内存分配
            for (int i = 0; i < 100; i++)
            {
                var script = new Script(
                    new ScriptName($"Memory Test Script {i}"),
                    new ScriptDescription($"Memory test script {i}")
                );
                
                for (int j = 0; j < 20; j++)
                {
                    var action = TestDataFactory.CreateGameAction();
                    script.AddAction(action);
                }
                
                scripts.Add(script);
            }
        }
    }

    /// <summary>
    /// 仓储性能基准测试
    /// </summary>
    [MemoryDiagnoser]
    public class RepositoryPerformanceBenchmarks
    {
        private List<Script> _testScripts;
        private KeyForgeDbContext _testDbContext;

        [GlobalSetup]
        public void Setup()
        {
            _testScripts = new List<Script>();
            
            // 创建测试数据
            for (int i = 0; i < 100; i++)
            {
                var script = TestDataFactory.CreateValidScript();
                
                // 为每个脚本添加多个动作
                for (int j = 0; j < 10; j++)
                {
                    var action = TestDataFactory.CreateGameAction();
                    script.AddAction(action);
                }
                
                _testScripts.Add(script);
            }
        }

        [Benchmark]
        public async Task BulkScriptInsert()
        {
            // 模拟批量插入操作
            var insertTasks = new List<Task>();
            
            foreach (var script in _testScripts.Take(50))
            {
                insertTasks.Add(Task.Run(async () =>
                {
                    // 模拟数据库插入延迟
                    await Task.Delay(1);
                    
                    var scriptData = new
                    {
                        Id = script.Id.Value,
                        Name = script.Name.Value,
                        Description = script.Description.Value,
                        Status = script.Status.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                }));
            }
            
            await Task.WhenAll(insertTasks);
        }

        [Benchmark]
        public async Task ScriptQueryById()
        {
            var scriptIds = _testScripts.Take(100).Select(s => s.Id.Value).ToList();
            var queryTasks = new List<Task>();
            
            foreach (var scriptId in scriptIds)
            {
                queryTasks.Add(Task.Run(async () =>
                {
                    // 模拟数据库查询延迟
                    await Task.Delay(1);
                    
                    var result = new
                    {
                        Id = scriptId,
                        Name = $"Script {scriptId}",
                        Found = true
                    };
                }));
            }
            
            await Task.WhenAll(queryTasks);
        }

        [Benchmark]
        public async Task ScriptQueryByStatus()
        {
            var statuses = Enum.GetValues<ScriptStatus>();
            var queryTasks = new List<Task>();
            
            foreach (var status in statuses)
            {
                queryTasks.Add(Task.Run(async () =>
                {
                    // 模拟按状态查询
                    await Task.Delay(1);
                    
                    var results = _testScripts
                        .Where(s => s.Status == status)
                        .Take(20)
                        .ToList();
                }));
            }
            
            await Task.WhenAll(queryTasks);
        }

        [Benchmark]
        public async Task ConcurrentReadOperations()
        {
            var readTasks = new List<Task>();
            
            // 创建多个并发读取操作
            for (int i = 0; i < 20; i++)
            {
                readTasks.Add(Task.Run(async () =>
                {
                    // 模拟并发读取
                    await Task.Delay(1);
                    
                    var scripts = _testScripts
                        .Skip(i * 5)
                        .Take(10)
                        .ToList();
                }));
            }
            
            await Task.WhenAll(readTasks);
        }
    }

    /// <summary>
    /// 程序入口
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DomainPerformanceBenchmarks>();
            Console.WriteLine("Domain Performance Benchmarks completed.");
            
            var repositorySummary = BenchmarkRunner.Run<RepositoryPerformanceBenchmarks>();
            Console.WriteLine("Repository Performance Benchmarks completed.");
        }
    }
}