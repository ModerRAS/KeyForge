using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.Performance
{
    /// <summary>
    /// 性能测试程序入口
    /// </summary>
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("KeyForge 性能测试套件");
            Console.WriteLine("======================");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            try
            {
                switch (args[0].ToLower())
                {
                    case "help":
                    case "-h":
                    case "--help":
                        ShowHelp();
                        break;

                    case "quick":
                        RunQuickTest();
                        break;

                    case "benchmark":
                        RunBenchmarkTests();
                        break;

                    case "load":
                        await RunLoadTestsAsync();
                        break;

                    case "memory":
                        RunMemoryTests();
                        break;

                    case "stress":
                        RunStressTests();
                        break;

                    case "all":
                        await RunAllTestsAsync();
                        break;

                    case "report":
                        GenerateReport();
                        break;

                    default:
                        Console.WriteLine($"未知命令: {args[0]}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行过程中发生错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        static void ShowHelp()
        {
            Console.WriteLine("可用命令:");
            Console.WriteLine("  help        - 显示此帮助信息");
            Console.WriteLine("  quick       - 运行快速性能检查");
            Console.WriteLine("  benchmark   - 运行基准测试");
            Console.WriteLine("  load        - 运行负载测试");
            Console.WriteLine("  memory      - 运行内存分析测试");
            Console.WriteLine("  stress      - 运行压力测试");
            Console.WriteLine("  all         - 运行所有性能测试");
            Console.WriteLine("  report      - 生成性能测试报告");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  dotnet run -- quick");
            Console.WriteLine("  dotnet run -- benchmark");
            Console.WriteLine("  dotnet run -- all");
        }

        static void RunQuickTest()
        {
            Console.WriteLine("运行快速性能检查...");
            Console.WriteLine("========================");
            
            PerformanceTestRunner.QuickPerformanceCheck();
        }

        static void RunBenchmarkTests()
        {
            Console.WriteLine("运行基准测试...");
            Console.WriteLine("================");
            
            try
            {
                var summary = BenchmarkRunner.Run<DomainPerformanceBenchmarks>();
                Console.WriteLine("领域模型基准测试完成。");
                
                var repositorySummary = BenchmarkRunner.Run<RepositoryPerformanceBenchmarks>();
                Console.WriteLine("仓储基准测试完成。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"基准测试失败: {ex.Message}");
            }
        }

        static async Task RunLoadTestsAsync()
        {
            Console.WriteLine("运行负载测试...");
            Console.WriteLine("================");
            
            try
            {
                await ComprehensiveLoadTestRunner.RunAllLoadTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"负载测试失败: {ex.Message}");
            }
        }

        static void RunMemoryTests()
        {
            Console.WriteLine("运行内存分析测试...");
            Console.WriteLine("=====================");
            
            try
            {
                MemoryAnalysisTestRunner.RunAllMemoryTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"内存分析测试失败: {ex.Message}");
            }
        }

        static void RunStressTests()
        {
            Console.WriteLine("运行压力测试...");
            Console.WriteLine("================");
            
            try
            {
                StressTestRunner.RunAllStressTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"压力测试失败: {ex.Message}");
            }
        }

        static async Task RunAllTestsAsync()
        {
            Console.WriteLine("运行所有性能测试...");
            Console.WriteLine("=====================");
            
            try
            {
                PerformanceTestRunner.RunAllPerformanceTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"性能测试失败: {ex.Message}");
            }
        }

        static void GenerateReport()
        {
            Console.WriteLine("生成性能测试报告...");
            Console.WriteLine("=====================");
            
            try
            {
                PerformanceTestRunner.GeneratePerformanceReport();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"报告生成失败: {ex.Message}");
            }
        }

        static void RunInteractiveMode()
        {
            Console.WriteLine("交互式性能测试模式");
            Console.WriteLine("=====================");
            
            while (true)
            {
                Console.WriteLine("\n请选择测试类型:");
                Console.WriteLine("1. 快速性能检查");
                Console.WriteLine("2. 基准测试");
                Console.WriteLine("3. 负载测试");
                Console.WriteLine("4. 内存分析测试");
                Console.WriteLine("5. 压力测试");
                Console.WriteLine("6. 运行所有测试");
                Console.WriteLine("7. 生成报告");
                Console.WriteLine("8. 退出");
                Console.Write("请输入选项 (1-8): ");

                var input = Console.ReadLine();
                
                switch (input)
                {
                    case "1":
                        RunQuickTest();
                        break;
                    case "2":
                        RunBenchmarkTests();
                        break;
                    case "3":
                        RunLoadTestsAsync().Wait();
                        break;
                    case "4":
                        RunMemoryTests();
                        break;
                    case "5":
                        RunStressTests();
                        break;
                    case "6":
                        RunAllTestsAsync().Wait();
                        break;
                    case "7":
                        GenerateReport();
                        break;
                    case "8":
                        Console.WriteLine("退出程序...");
                        return;
                    default:
                        Console.WriteLine("无效选项，请重新输入。");
                        break;
                }
                
                Console.WriteLine("\n按任意键继续...");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// 性能测试配置类
    /// </summary>
    public class PerformanceTestConfig
    {
        public static PerformanceTestConfig LoadFromFile(string filePath)
        {
            // 这里应该从JSON文件加载配置
            // 简化实现，返回默认配置
            return new PerformanceTestConfig();
        }

        public int WarmupIterations { get; set; } = 3;
        public int TargetIterations { get; set; } = 10;
        public TimeSpan MinIterationTime { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxIterationTime { get; set; } = TimeSpan.FromMinutes(1);
        public int MaxConcurrentUsers { get; set; } = 50;
        public int TestDurationSeconds { get; set; } = 30;
        public double SuccessRateThreshold { get; set; } = 95.0;
        public int ResponseTimeThresholdMs { get; set; } = 1000;
        public bool EnableDetailedLogging { get; set; } = true;
        public string LogFilePath { get; set; } = "performance-test.log";
    }

    /// <summary>
    /// 性能测试结果收集器
    /// </summary>
    public class PerformanceTestResultCollector
    {
        private readonly List<PerformanceTestResult> _results = new List<PerformanceTestResult>();

        public void AddResult(PerformanceTestResult result)
        {
            _results.Add(result);
        }

        public void SaveToFile(string filePath)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_results, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);
        }

        public PerformanceTestSummary GetSummary()
        {
            if (!_results.Any())
            {
                return new PerformanceTestSummary();
            }

            return new PerformanceTestSummary
            {
                TotalTests = _results.Count,
                PassedTests = _results.Count(r => r.Success),
                FailedTests = _results.Count(r => !r.Success),
                AverageExecutionTime = _results.Average(r => r.ExecutionTime.TotalMilliseconds),
                MinExecutionTime = _results.Min(r => r.ExecutionTime.TotalMilliseconds),
                MaxExecutionTime = _results.Max(r => r.ExecutionTime.TotalMilliseconds),
                SuccessRate = (double)_results.Count(r => r.Success) / _results.Count * 100
            };
        }
    }

    /// <summary>
    /// 性能测试结果
    /// </summary>
    public class PerformanceTestResult
    {
        public string TestName { get; set; }
        public bool Success { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 性能测试摘要
    /// </summary>
    public class PerformanceTestSummary
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public double AverageExecutionTime { get; set; }
        public double MinExecutionTime { get; set; }
        public double MaxExecutionTime { get; set; }
        public double SuccessRate { get; set; }
    }
}