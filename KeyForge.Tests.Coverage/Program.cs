using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace KeyForge.Tests.Coverage
{
    /// <summary>
    /// 测试覆盖率程序入口
    /// </summary>
    public class CoverageTestProgram
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("KeyForge 测试覆盖率分析工具");
            Console.WriteLine("============================");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            // 配置Serilog日志
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("coverage-tests.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                var command = args[0].ToLower();
                var output = new TestOutputHelper();

                switch (command)
                {
                    case "help":
                    case "-h":
                    case "--help":
                        ShowHelp();
                        break;

                    case "run":
                        await RunCoverageAnalysisAsync(output);
                        break;

                    case "report":
                        await GenerateCoverageReportAsync(output);
                        break;

                    case "validate":
                        await ValidateCoverageAsync(output);
                        break;

                    case "config":
                        ShowConfiguration();
                        break;

                    case "thresholds":
                        ShowThresholds();
                        break;

                    case "recommendations":
                        await ShowRecommendationsAsync(output);
                        break;

                    case "history":
                        await ShowCoverageHistoryAsync(output);
                        break;

                    case "compare":
                        await CompareCoverageAsync(output);
                        break;

                    default:
                        Console.WriteLine($"未知命令: {command}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "覆盖率分析失败");
                Console.WriteLine($"执行过程中发生错误: {ex.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        static void ShowHelp()
        {
            Console.WriteLine("可用命令:");
            Console.WriteLine("  help           - 显示此帮助信息");
            Console.WriteLine("  run            - 运行覆盖率分析");
            Console.WriteLine("  report         - 生成覆盖率报告");
            Console.WriteLine("  validate       - 验证覆盖率阈值");
            Console.WriteLine("  config         - 显示当前配置");
            Console.WriteLine("  thresholds     - 显示覆盖率阈值");
            Console.WriteLine("  recommendations - 显示覆盖率建议");
            Console.WriteLine("  history        - 显示覆盖率历史");
            Console.WriteLine("  compare        - 比较覆盖率数据");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  dotnet run -- run");
            Console.WriteLine("  dotnet run -- report");
            Console.WriteLine("  dotnet run -- validate");
            Console.WriteLine();
            Console.WriteLine("配置文件:");
            Console.WriteLine("  coverage-settings.json - 覆盖率分析配置");
            Console.WriteLine("  ci-settings.json - CI/CD 配置");
        }

        static async Task RunCoverageAnalysisAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行覆盖率分析...");
            Console.WriteLine("=================");

            var runner = new CoverageTestRunner(output);
            var summary = await runner.RunCoverageAnalysisAsync();

            Console.WriteLine($"覆盖率分析完成");
            Console.WriteLine($"总覆盖率: {summary.OverallCoverage:F1}%");
            Console.WriteLine($"行覆盖率: {summary.LineCoverage:F1}%");
            Console.WriteLine($"分支覆盖率: {summary.BranchCoverage:F1}%");
            Console.WriteLine($"方法覆盖率: {summary.MethodCoverage:F1}%");
            Console.WriteLine($"类覆盖率: {summary.ClassCoverage:F1}%");
            Console.WriteLine($"质量等级: {GetQualityGradeText(summary.QualityGrade)}");
            Console.WriteLine($"阈值检查: {(summary.ThresholdMet ? "✅ 通过" : "❌ 失败")}");

            if (!summary.ThresholdMet)
            {
                Console.WriteLine($"警告: 覆盖率低于最低要求 {summary.MinimumThreshold}%");
            }

            // 显示建议
            var recommendations = runner.GenerateCoverageRecommendations(summary);
            if (recommendations.Any())
            {
                Console.WriteLine("\n建议:");
                foreach (var recommendation in recommendations)
                {
                    Console.WriteLine($"  - {recommendation}");
                }
            }
        }

        static async Task GenerateCoverageReportAsync(ITestOutputHelper output)
        {
            Console.WriteLine("生成覆盖率报告...");
            Console.WriteLine("=================");

            var runner = new CoverageTestRunner(output);
            var summary = await runner.RunCoverageAnalysisAsync();

            Console.WriteLine($"覆盖率报告已生成");
            Console.WriteLine($"报告位置: coverage/report/");
            Console.WriteLine($"HTML报告: coverage/report/index.html");
            Console.WriteLine($"XML报告: coverage/report/coverage.xml");
            Console.WriteLine($"Markdown报告: coverage/report/README.md");
            Console.WriteLine($"徽章: coverage/report/coverage.svg");
        }

        static async Task ValidateCoverageAsync(ITestOutputHelper output)
        {
            Console.WriteLine("验证覆盖率阈值...");
            Console.WriteLine("=================");

            var runner = new CoverageTestRunner(output);
            var summary = await runner.RunCoverageAnalysisAsync();

            try
            {
                runner.ValidateCoverage(summary);
                Console.WriteLine("✅ 覆盖率验证通过");
                Console.WriteLine($"当前覆盖率: {summary.OverallCoverage:F1}%");
                Console.WriteLine($"最低要求: {summary.MinimumThreshold}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ 覆盖率验证失败");
                Console.WriteLine($"错误: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static void ShowConfiguration()
        {
            Console.WriteLine("当前覆盖率配置:");
            Console.WriteLine("=================");

            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "coverage-settings.json");
                if (File.Exists(configPath))
                {
                    var config = File.ReadAllText(configPath);
                    Console.WriteLine(config);
                }
                else
                {
                    Console.WriteLine("未找到配置文件，使用默认配置");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取配置文件失败: {ex.Message}");
            }
        }

        static void ShowThresholds()
        {
            Console.WriteLine("覆盖率阈值设置:");
            Console.WriteLine("=================");

            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "coverage-settings.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<CoverageSettings>(json);
                    
                    if (config?.Thresholds != null)
                    {
                        Console.WriteLine($"严重 (Critical):");
                        Console.WriteLine($"  行覆盖率: {config.Thresholds.Critical.LineCoverage}%");
                        Console.WriteLine($"  分支覆盖率: {config.Thresholds.Critical.BranchCoverage}%");
                        Console.WriteLine($"  方法覆盖率: {config.Thresholds.Critical.MethodCoverage}%");
                        Console.WriteLine($"  类覆盖率: {config.Thresholds.Critical.ClassCoverage}%");
                        Console.WriteLine();

                        Console.WriteLine($"警告 (Warning):");
                        Console.WriteLine($"  行覆盖率: {config.Thresholds.Warning.LineCoverage}%");
                        Console.WriteLine($"  分支覆盖率: {config.Thresholds.Warning.BranchCoverage}%");
                        Console.WriteLine($"  方法覆盖率: {config.Thresholds.Warning.MethodCoverage}%");
                        Console.WriteLine($"  类覆盖率: {config.Thresholds.Warning.ClassCoverage}%");
                        Console.WriteLine();

                        Console.WriteLine($"良好 (Good):");
                        Console.WriteLine($"  行覆盖率: {config.Thresholds.Good.LineCoverage}%");
                        Console.WriteLine($"  分支覆盖率: {config.Thresholds.Good.BranchCoverage}%");
                        Console.WriteLine($"  方法覆盖率: {config.Thresholds.Good.MethodCoverage}%");
                        Console.WriteLine($"  类覆盖率: {config.Thresholds.Good.ClassCoverage}%");
                        Console.WriteLine();

                        Console.WriteLine($"优秀 (Excellent):");
                        Console.WriteLine($"  行覆盖率: {config.Thresholds.Excellent.LineCoverage}%");
                        Console.WriteLine($"  分支覆盖率: {config.Thresholds.Excellent.BranchCoverage}%");
                        Console.WriteLine($"  方法覆盖率: {config.Thresholds.Excellent.MethodCoverage}%");
                        Console.WriteLine($"  类覆盖率: {config.Thresholds.Excellent.ClassCoverage}%");
                    }
                }
                else
                {
                    Console.WriteLine("未找到配置文件");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取配置文件失败: {ex.Message}");
            }
        }

        static async Task ShowRecommendationsAsync(ITestOutputHelper output)
        {
            Console.WriteLine("覆盖率建议:");
            Console.WriteLine("=============");

            var runner = new CoverageTestRunner(output);
            var summary = await runner.RunCoverageAnalysisAsync();

            var recommendations = runner.GenerateCoverageRecommendations(summary);

            foreach (var recommendation in recommendations)
            {
                Console.WriteLine($"- {recommendation}");
            }

            if (!recommendations.Any())
            {
                Console.WriteLine("当前覆盖率指标良好，无特殊建议");
            }
        }

        static async Task ShowCoverageHistoryAsync(ITestOutputHelper output)
        {
            Console.WriteLine("覆盖率历史:");
            Console.WriteLine("=============");

            var historyDir = Path.Combine(Directory.GetCurrentDirectory(), "coverage", "history");
            if (Directory.Exists(historyDir))
            {
                var historyFiles = Directory.GetFiles(historyDir, "*.json")
                    .OrderBy(f => f)
                    .Take(10); // 显示最近10次

                foreach (var file in historyFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var summary = JsonSerializer.Deserialize<CoverageSummary>(json);
                        
                        Console.WriteLine($"{Path.GetFileNameWithoutExtension(file)}: {summary?.OverallCoverage:F1}% ({GetQualityGradeText(summary?.QualityGrade ?? QualityGrade.Warning)})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"读取历史文件失败: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("未找到覆盖率历史数据");
            }
        }

        static async Task CompareCoverageAsync(ITestOutputHelper output)
        {
            Console.WriteLine("比较覆盖率数据:");
            Console.WriteLine("================");

            var runner = new CoverageTestRunner(output);
            var currentSummary = await runner.RunCoverageAnalysisAsync();

            // 查找前一次的覆盖率数据
            var historyDir = Path.Combine(Directory.GetCurrentDirectory(), "coverage", "history");
            CoverageSummary? previousSummary = null;

            if (Directory.Exists(historyDir))
            {
                var historyFiles = Directory.GetFiles(historyDir, "*.json")
                    .OrderByDescending(f => f)
                    .Skip(1)
                    .FirstOrDefault();

                if (historyFiles != null)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(historyFiles);
                        previousSummary = JsonSerializer.Deserialize<CoverageSummary>(json);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"读取历史数据失败: {ex.Message}");
                    }
                }
            }

            if (previousSummary != null)
            {
                Console.WriteLine($"当前覆盖率: {currentSummary.OverallCoverage:F1}%");
                Console.WriteLine($"前次覆盖率: {previousSummary.OverallCoverage:F1}%");
                Console.WriteLine($"变化: {(currentSummary.OverallCoverage - previousSummary.OverallCoverage):+F1}%");

                Console.WriteLine("\n详细对比:");
                Console.WriteLine($"行覆盖率: {currentSummary.LineCoverage:F1}% (前次: {previousSummary.LineCoverage:F1}%)");
                Console.WriteLine($"分支覆盖率: {currentSummary.BranchCoverage:F1}% (前次: {previousSummary.BranchCoverage:F1}%)");
                Console.WriteLine($"方法覆盖率: {currentSummary.MethodCoverage:F1}% (前次: {previousSummary.MethodCoverage:F1}%)");
                Console.WriteLine($"类覆盖率: {currentSummary.ClassCoverage:F1}% (前次: {previousSummary.ClassCoverage:F1}%)");
            }
            else
            {
                Console.WriteLine("未找到历史数据进行比较");
                Console.WriteLine($"当前覆盖率: {currentSummary.OverallCoverage:F1}%");
            }
        }

        static string GetQualityGradeText(QualityGrade grade)
        {
            return grade switch
            {
                QualityGrade.Excellent => "优秀",
                QualityGrade.Good => "良好",
                QualityGrade.Warning => "警告",
                QualityGrade.Critical => "严重",
                _ => "未知"
            };
        }

        /// <summary>
        /// 测试输出帮助器
        /// </summary>
        private class TestOutputHelper : ITestOutputHelper
        {
            public void WriteLine(string message)
            {
                Console.WriteLine(message);
            }

            public void WriteLine(string format, params object[] args)
            {
                Console.WriteLine(format, args);
            }
        }
    }
}