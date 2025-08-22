using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace KeyForge.Tests.Security
{
    /// <summary>
    /// 安全测试程序入口
    /// </summary>
    public class SecurityTestProgram
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("KeyForge 安全测试套件");
            Console.WriteLine("====================");
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
                .WriteTo.File("security-tests.log", rollingInterval: RollingInterval.Day)
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

                    case "quick":
                        await RunQuickSecurityCheckAsync(output);
                        break;

                    case "full":
                        await RunFullSecurityTestSuiteAsync(output);
                        break;

                    case "auth":
                        await RunAuthenticationTestsAsync(output);
                        break;

                    case "input":
                        await RunInputValidationTestsAsync(output);
                        break;

                    case "http":
                        await RunHttpSecurityTestsAsync(output);
                        break;

                    case "encryption":
                        await RunEncryptionTestsAsync(output);
                        break;

                    case "scan":
                        await RunVulnerabilityScanAsync(output);
                        break;

                    case "pentest":
                        await RunPenetrationTestsAsync(output);
                        break;

                    case "report":
                        await GenerateReportAsync(output);
                        break;

                    case "config":
                        ShowConfiguration();
                        break;

                    default:
                        Console.WriteLine($"未知命令: {command}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "安全测试执行失败");
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
            Console.WriteLine("  help        - 显示此帮助信息");
            Console.WriteLine("  quick       - 运行快速安全检查");
            Console.WriteLine("  full        - 运行完整安全测试套件");
            Console.WriteLine("  auth        - 运行认证和授权测试");
            Console.WriteLine("  input       - 运行输入验证测试");
            Console.WriteLine("  http        - 运行HTTP安全测试");
            Console.WriteLine("  encryption  - 运行加密安全测试");
            Console.WriteLine("  scan        - 运行漏洞扫描");
            Console.WriteLine("  pentest     - 运行渗透测试");
            Console.WriteLine("  report      - 生成安全测试报告");
            Console.WriteLine("  config      - 显示当前配置");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  dotnet run -- quick");
            Console.WriteLine("  dotnet run -- full");
            Console.WriteLine("  dotnet run -- scan");
            Console.WriteLine();
            Console.WriteLine("配置文件:");
            Console.WriteLine("  security-test-config.json - 安全测试配置");
            Console.WriteLine("  SecurityRules.ruleset - 安全规则");
        }

        static async Task RunQuickSecurityCheckAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行快速安全检查...");
            Console.WriteLine("=================");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunQuickSecurityCheckAsync();
            
            Console.WriteLine($"快速安全检查完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
            
            if (summary.SuccessRate < 80)
            {
                Console.WriteLine("警告: 安全测试通过率较低，请检查安全问题");
            }
        }

        static async Task RunFullSecurityTestSuiteAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行完整安全测试套件...");
            Console.WriteLine("=====================");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunFullSecurityTestSuiteAsync();
            
            Console.WriteLine($"完整安全测试套件完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
            
            if (summary.SuccessRate < 90)
            {
                Console.WriteLine("警告: 安全测试通过率较低，请检查安全问题");
            }
        }

        static async Task RunAuthenticationTestsAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行认证和授权测试...");
            Console.WriteLine("====================");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunSecurityTestsAsync(SecurityTestType.Authentication);
            
            Console.WriteLine($"认证和授权测试完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
        }

        static async Task RunInputValidationTestsAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行输入验证测试...");
            Console.WriteLine("==================");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunSecurityTestsAsync(SecurityTestType.InputValidation);
            
            Console.WriteLine($"输入验证测试完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
        }

        static async Task RunHttpSecurityTestsAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行HTTP安全测试...");
            Console.WriteLine("==================");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunSecurityTestsAsync(SecurityTestType.HttpSecurity);
            
            Console.WriteLine($"HTTP安全测试完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
        }

        static async Task RunEncryptionTestsAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行加密安全测试...");
            Console.WriteLine("==================");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunSecurityTestsAsync(SecurityTestType.Encryption);
            
            Console.WriteLine($"加密安全测试完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
        }

        static async Task RunVulnerabilityScanAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行漏洞扫描...");
            Console.WriteLine("===============");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunSecurityTestsAsync(SecurityTestType.VulnerabilityScan);
            
            Console.WriteLine($"漏洞扫描完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
        }

        static async Task RunPenetrationTestsAsync(ITestOutputHelper output)
        {
            Console.WriteLine("运行渗透测试...");
            Console.WriteLine("===============");
            
            var runner = new SecurityTestRunner(output);
            var summary = await runner.RunSecurityTestsAsync(SecurityTestType.PenetrationTest);
            
            Console.WriteLine($"渗透测试完成");
            Console.WriteLine($"总测试数: {summary.TotalTests}");
            Console.WriteLine($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            Console.WriteLine($"成功率: {summary.SuccessRate:F1}%");
        }

        static async Task GenerateReportAsync(ITestOutputHelper output)
        {
            Console.WriteLine("生成安全测试报告...");
            Console.WriteLine("===================");
            
            var runner = new SecurityTestRunner(output);
            await runner.GenerateSecurityReportAsync();
            
            Console.WriteLine("安全测试报告生成完成");
            Console.WriteLine("报告位置: SecurityReports/");
        }

        static void ShowConfiguration()
        {
            Console.WriteLine("当前安全测试配置:");
            Console.WriteLine("===================");
            
            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "security-test-config.json");
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

    /// <summary>
    /// 安全测试扩展方法
    /// </summary>
    public static class SecurityTestExtensions
    {
        /// <summary>
        /// 运行安全测试并记录结果
        /// </summary>
        public static async Task RunSecurityTestWithLoggingAsync(
            this SecurityTestRunner runner,
            string testName,
            Func<Task> testAction)
        {
            Console.WriteLine($"开始测试: {testName}");
            
            try
            {
                await testAction();
                Console.WriteLine($"测试通过: {testName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {testName} - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 验证安全评分
        /// </summary>
        public static void ValidateSecurityScore(this SecurityTestSummary summary, double minimumScore = 80.0)
        {
            if (summary.SuccessRate < minimumScore)
            {
                throw new Exception($"安全评分过低: {summary.SuccessRate:F1}% (最低要求: {minimumScore}%)");
            }
        }

        /// <summary>
        /// 生成安全建议
        /// </summary>
        public static List<string> GenerateSecurityRecommendations(this SecurityTestSummary summary)
        {
            var recommendations = new List<string>();

            if (summary.SuccessRate < 90)
            {
                recommendations.Add("提高测试通过率，检查失败的测试用例");
            }

            if (summary.FailedTests > 0)
            {
                recommendations.Add("修复失败的测试用例中的安全问题");
            }

            if (summary.AverageExecutionTime > 1000)
            {
                recommendations.Add("优化测试性能，减少执行时间");
            }

            return recommendations;
        }
    }
}