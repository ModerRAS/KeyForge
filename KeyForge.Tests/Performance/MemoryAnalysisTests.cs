using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// 内存使用分析测试
    /// 检测内存泄漏和内存使用效率
    /// </summary>
    public class MemoryAnalysisTests
    {
        [Fact]
        public void ScriptCreation_MemoryUsage_ShouldBeEfficient()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var scripts = new List<Script>();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                var script = new Script(
                    new ScriptName($"Script {i}"),
                    new ScriptDescription($"Description {i}")
                );
                
                // 添加一些动作
                for (int j = 0; j < 10; j++)
                {
                    var action = TestDataFactory.CreateGameAction();
                    script.AddAction(action);
                }
                
                scripts.Add(script);
            }

            var finalMemory = GC.GetTotalMemory(true);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert
            Console.WriteLine($"创建1000个脚本，内存增长: {memoryIncrease / 1024:F2} KB");
            Console.WriteLine($"平均每个脚本内存使用: {memoryIncrease / 1000:F2} bytes");
            
            // 清理
            scripts.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            var afterCleanupMemory = GC.GetTotalMemory(true);
            var cleanupEfficiency = ((double)(memoryIncrease - (afterCleanupMemory - initialMemory)) / memoryIncrease) * 100;
            
            Console.WriteLine($"内存清理效率: {cleanupEfficiency:F2}%");
            
            // 内存使用应该在合理范围内
            Assert.True(memoryIncrease < 50 * 1024 * 1024, "内存使用过高"); // 小于50MB
        }

        [Fact]
        public void LargeScript_MemoryUsage_ShouldBeManaged()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act
            var largeScript = new Script(
                new ScriptName("Large Script"),
                new ScriptDescription("Large script with many actions")
            );
            
            // 添加大量动作
            for (int i = 0; i < 10000; i++)
            {
                var action = TestDataFactory.CreateGameAction();
                largeScript.AddAction(action);
            }

            var memoryAfterCreation = GC.GetTotalMemory(true);
            var creationMemoryIncrease = memoryAfterCreation - initialMemory;

            // 强制垃圾回收
            largeScript = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            var memoryAfterCleanup = GC.GetTotalMemory(true);
            var cleanupEfficiency = ((double)(creationMemoryIncrease - (memoryAfterCleanup - initialMemory)) / creationMemoryIncrease) * 100;

            // Assert
            Console.WriteLine($"创建大型脚本，内存增长: {creationMemoryIncrease / 1024 / 1024:F2} MB");
            Console.WriteLine($"内存清理效率: {cleanupEfficiency:F2}%");
            
            Assert.True(creationMemoryIncrease < 100 * 1024 * 1024, "大型脚本内存使用过高"); // 小于100MB
            Assert.True(cleanupEfficiency > 80, "内存清理效率过低"); // 清理效率大于80%
        }

        [Fact]
        public void StateMachineCreation_MemoryUsage_ShouldBeEfficient()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var stateMachines = new List<StateMachine>();

            // Act
            for (int i = 0; i < 100; i++)
            {
                var stateMachine = new StateMachine(
                    new StateMachineName($"StateMachine {i}"),
                    new StateMachineDescription($"Description {i}")
                );
                
                // 添加状态
                for (int j = 0; j < 20; j++)
                {
                    var state = new State($"State_{i}_{j}", $"State {j}");
                    stateMachine.AddState(state);
                }
                
                // 添加转换
                for (int j = 0; j < 19; j++)
                {
                    var fromState = new State($"State_{i}_{j}", $"State {j}");
                    var toState = new State($"State_{i}_{j + 1}", $"State {j + 1}");
                    var condition = new StateCondition($"condition_{i}_{j}", "true");
                    stateMachine.AddTransition(fromState, toState, condition);
                }
                
                stateMachines.Add(stateMachine);
            }

            var finalMemory = GC.GetTotalMemory(true);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert
            Console.WriteLine($"创建100个状态机，内存增长: {memoryIncrease / 1024:F2} KB");
            Console.WriteLine($"平均每个状态机内存使用: {memoryIncrease / 100:F2} bytes");
            
            // 清理
            stateMachines.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            var afterCleanupMemory = GC.GetTotalMemory(true);
            var cleanupEfficiency = ((double)(memoryIncrease - (afterCleanupMemory - initialMemory)) / memoryIncrease) * 100;
            
            Console.WriteLine($"内存清理效率: {cleanupEfficiency:F2}%");
            
            Assert.True(memoryIncrease < 20 * 1024 * 1024, "状态机内存使用过高"); // 小于20MB
        }

        [Fact]
        public void ConcurrentOperations_MemoryUsage_ShouldBeStable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var tasks = new List<Task>();
            
            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var scripts = new List<Script>();
                    
                    for (int j = 0; j < 100; j++)
                    {
                        var script = new Script(
                            new ScriptName($"Concurrent Script {i}_{j}"),
                            new ScriptDescription($"Concurrent script {i}_{j}")
                        );
                        
                        for (int k = 0; k < 20; k++)
                        {
                            var action = TestDataFactory.CreateGameAction();
                            script.AddAction(action);
                        }
                        
                        scripts.Add(script);
                    }
                    
                    return scripts;
                }));
            }
            
            Task.WaitAll(tasks.ToArray());
            
            var afterOperationsMemory = GC.GetTotalMemory(true);
            var memoryIncrease = afterOperationsMemory - initialMemory;
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            var afterCleanupMemory = GC.GetTotalMemory(true);
            var cleanupEfficiency = ((double)(memoryIncrease - (afterCleanupMemory - initialMemory)) / memoryIncrease) * 100;

            // Assert
            Console.WriteLine($"并发操作内存增长: {memoryIncrease / 1024 / 1024:F2} MB");
            Console.WriteLine($"内存清理效率: {cleanupEfficiency:F2}%");
            
            Assert.True(memoryIncrease < 100 * 1024 * 1024, "并发操作内存使用过高"); // 小于100MB
            Assert.True(cleanupEfficiency > 85, "并发操作内存清理效率过低"); // 清理效率大于85%
        }

        [Fact]
        public void ValueObjectCreation_MemoryUsage_ShouldBeMinimal()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act
            var valueObjects = new List<object>();
            
            for (int i = 0; i < 10000; i++)
            {
                valueObjects.Add(new ScriptName($"Script {i}"));
                valueObjects.Add(new ScriptDescription($"Description {i}"));
                valueObjects.Add(new ScriptId(Guid.NewGuid()));
                valueObjects.Add(new Timestamp(DateTime.UtcNow));
                valueObjects.Add(new Duration(i));
            }

            var afterCreationMemory = GC.GetTotalMemory(true);
            var memoryIncrease = afterCreationMemory - initialMemory;
            
            // 清理
            valueObjects.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            var afterCleanupMemory = GC.GetTotalMemory(true);
            var cleanupEfficiency = ((double)(memoryIncrease - (afterCleanupMemory - initialMemory)) / memoryIncrease) * 100;

            // Assert
            Console.WriteLine($"创建50000个值对象，内存增长: {memoryIncrease / 1024:F2} KB");
            Console.WriteLine($"平均每个值对象内存使用: {memoryIncrease / 50000:F2} bytes");
            Console.WriteLine($"内存清理效率: {cleanupEfficiency:F2}%");
            
            Assert.True(memoryIncrease < 10 * 1024 * 1024, "值对象内存使用过高"); // 小于10MB
            Assert.True(cleanupEfficiency > 90, "值对象内存清理效率过低"); // 清理效率大于90%
        }

        [Fact]
        public void LongRunningOperation_MemoryUsage_ShouldNotGrow()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var maxMemory = initialMemory;
            var memorySamples = new List<long>();
            
            // Act
            // 长时间运行的操作
            for (int iteration = 0; iteration < 100; iteration++)
            {
                // 创建一些对象
                var scripts = new List<Script>();
                for (int i = 0; i < 10; i++)
                {
                    var script = new Script(
                        new ScriptName($"Iteration {iteration} Script {i}"),
                        new ScriptDescription($"Script from iteration {iteration}")
                    );
                    
                    for (int j = 0; j < 5; j++)
                    {
                        var action = TestDataFactory.CreateGameAction();
                        script.AddAction(action);
                    }
                    
                    scripts.Add(script);
                }
                
                // 记录内存使用
                var currentMemory = GC.GetTotalMemory(false);
                memorySamples.Add(currentMemory);
                
                if (currentMemory > maxMemory)
                {
                    maxMemory = currentMemory;
                }
                
                // 清理当前迭代的对象
                scripts.Clear();
                
                // 每隔10次迭代强制垃圾回收
                if (iteration % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            var finalMemory = GC.GetTotalMemory(true);
            var memoryGrowth = finalMemory - initialMemory;
            var maxMemoryGrowth = maxMemory - initialMemory;

            // Assert
            Console.WriteLine($"长时间运行操作内存增长: {memoryGrowth / 1024:F2} KB");
            Console.WriteLine($"最大内存增长: {maxMemoryGrowth / 1024:F2} KB");
            
            // 计算内存稳定性
            var averageMemory = memorySamples.Average();
            var memoryVariance = memorySamples.Select(m => Math.Pow(m - averageMemory, 2)).Average();
            var standardDeviation = Math.Sqrt(memoryVariance);
            
            Console.WriteLine($"内存使用标准差: {standardDeviation / 1024:F2} KB");
            
            Assert.True(memoryGrowth < 5 * 1024 * 1024, "长时间运行内存增长过多"); // 小于5MB
            Assert.True(standardDeviation < 1024 * 1024, "内存使用波动过大"); // 标准差小于1MB
        }

        [Fact]
        public void ImageTemplateOperations_MemoryUsage_ShouldBeEfficient()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act
            var templates = new List<ImageTemplate>();
            
            for (int i = 0; i < 100; i++)
            {
                var template = new ImageTemplate(
                    new TemplateName($"Template {i}"),
                    new TemplateDescription($"Template {i} description")
                );
                
                // 添加匹配区域
                for (int j = 0; j < 50; j++)
                {
                    var matchArea = new MatchArea(
                        new ScreenLocation(j * 10, j * 10),
                        new ScreenLocation(j * 10 + 50, j * 10 + 50),
                        0.8,
                        $"Area_{j}"
                    );
                    template.AddMatchArea(matchArea);
                }
                
                templates.Add(template);
            }

            var afterCreationMemory = GC.GetTotalMemory(true);
            var memoryIncrease = afterCreationMemory - initialMemory;
            
            // 清理
            templates.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            var afterCleanupMemory = GC.GetTotalMemory(true);
            var cleanupEfficiency = ((double)(memoryIncrease - (afterCleanupMemory - initialMemory)) / memoryIncrease) * 100;

            // Assert
            Console.WriteLine($"创建100个图像模板，内存增长: {memoryIncrease / 1024:F2} KB");
            Console.WriteLine($"平均每个模板内存使用: {memoryIncrease / 100:F2} bytes");
            Console.WriteLine($"内存清理效率: {cleanupEfficiency:F2}%");
            
            Assert.True(memoryIncrease < 30 * 1024 * 1024, "图像模板内存使用过高"); // 小于30MB
            Assert.True(cleanupEfficiency > 85, "图像模板内存清理效率过低"); // 清理效率大于85%
        }
    }

    /// <summary>
    /// 内存分析工具类
    /// </summary>
    public static class MemoryAnalysisHelper
    {
        public static void PrintMemoryUsage(string operationName)
        {
            var memoryBefore = GC.GetTotalMemory(true);
            Console.WriteLine($"{operationName} - 内存使用前: {memoryBefore / 1024:F2} KB");
        }

        public static void PrintMemoryDifference(string operationName, long memoryBefore)
        {
            var memoryAfter = GC.GetTotalMemory(true);
            var difference = memoryAfter - memoryBefore;
            Console.WriteLine($"{operationName} - 内存使用后: {memoryAfter / 1024:F2} KB");
            Console.WriteLine($"{operationName} - 内存差异: {difference / 1024:F2} KB");
        }

        public static void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public static long GetProcessMemoryUsage()
        {
            using var process = Process.GetCurrentProcess();
            return process.PrivateMemorySize64;
        }

        public static void PrintProcessMemoryInfo()
        {
            using var process = Process.GetCurrentProcess();
            Console.WriteLine($"进程内存使用:");
            Console.WriteLine($"  工作集: {process.WorkingSet64 / 1024 / 1024:F2} MB");
            Console.WriteLine($"  专用内存: {process.PrivateMemorySize64 / 1024 / 1024:F2} MB");
            Console.WriteLine($"  虚拟内存: {process.VirtualMemorySize64 / 1024 / 1024:F2} MB");
            Console.WriteLine($"  分页内存: {process.PagedMemorySize64 / 1024 / 1024:F2} MB");
        }
    }

    /// <summary>
    /// 内存分析测试运行器
    /// </summary>
    public class MemoryAnalysisTestRunner
    {
        public static void RunAllMemoryTests()
        {
            Console.WriteLine("开始内存分析测试...");
            Console.WriteLine("=====================================");
            
            MemoryAnalysisHelper.PrintProcessMemoryInfo();
            Console.WriteLine();

            var tests = new List<Action>
            {
                () => new MemoryAnalysisTests().ScriptCreation_MemoryUsage_ShouldBeEfficient(),
                () => new MemoryAnalysisTests().LargeScript_MemoryUsage_ShouldBeManaged(),
                () => new MemoryAnalysisTests().StateMachineCreation_MemoryUsage_ShouldBeEfficient(),
                () => new MemoryAnalysisTests().ConcurrentOperations_MemoryUsage_ShouldBeStable(),
                () => new MemoryAnalysisTests().ValueObjectCreation_MemoryUsage_ShouldBeMinimal(),
                () => new MemoryAnalysisTests().LongRunningOperation_MemoryUsage_ShouldNotGrow(),
                () => new MemoryAnalysisTests().ImageTemplateOperations_MemoryUsage_ShouldBeEfficient()
            };

            foreach (var test in tests)
            {
                try
                {
                    MemoryAnalysisHelper.PrintMemoryUsage(test.Method.Name);
                    test();
                    MemoryAnalysisHelper.PrintMemoryDifference(test.Method.Name, GC.GetTotalMemory(true));
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"测试 {test.Method.Name} 失败: {ex.Message}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("=====================================");
            MemoryAnalysisHelper.PrintProcessMemoryInfo();
            Console.WriteLine("内存分析测试完成。");
        }
    }
}