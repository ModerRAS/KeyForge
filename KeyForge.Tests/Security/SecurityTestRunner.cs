using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace KeyForge.Tests.Security
{
    /// <summary>
    /// 安全测试运行器
    /// </summary>
    public class SecurityTestRunner : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<SecurityTestRunner> _logger;
        private readonly SecurityTestConfig _config;
        private readonly SecurityTestResultCollector _results;
        
        private readonly List<SecurityTestResult> _testResults = new();
        private readonly List<VulnerabilityScanResult> _scanResults = new();

        public SecurityTestRunner(ITestOutputHelper output)
        {
            _output = output;
            _results = new SecurityTestResultCollector();
            
            // 配置日志
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXunit(output);
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            _logger = loggerFactory.CreateLogger<SecurityTestRunner>();
            
            // 加载配置
            _config = LoadConfiguration();
        }

        /// <summary>
        /// 运行快速安全检查
        /// </summary>
        public async Task<SecurityTestSummary> RunQuickSecurityCheckAsync()
        {
            _logger.LogInformation("开始快速安全检查...");
            
            var runner = new AuthenticationSecurityTests(_output);
            var authResults = await RunAuthenticationTestsAsync(runner);
            
            var httpRunner = new HttpSecurityTests(_output);
            var httpResults = await RunHttpSecurityTestsAsync(httpRunner);
            
            var summary = _results.GetSummary();
            
            _logger.LogInformation("快速安全检查完成");
            _logger.LogInformation($"总测试数: {summary.TotalTests}");
            _logger.LogInformation($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            _logger.LogInformation($"成功率: {summary.SuccessRate:F1}%");
            
            return summary;
        }

        /// <summary>
        /// 运行完整安全测试套件
        /// </summary>
        public async Task<SecurityTestSummary> RunFullSecurityTestSuiteAsync()
        {
            _logger.LogInformation("开始完整安全测试套件...");
            
            // 1. 认证和授权测试
            var authRunner = new AuthenticationSecurityTests(_output);
            await RunAuthenticationTestsAsync(authRunner);
            
            // 2. 输入验证测试
            var inputRunner = new InputValidationSecurityTests(_output);
            await RunInputValidationTestsAsync(inputRunner);
            
            // 3. HTTP安全测试
            var httpRunner = new HttpSecurityTests(_output);
            await RunHttpSecurityTestsAsync(httpRunner);
            
            // 4. 加密安全测试
            var encryptionRunner = new EncryptionSecurityTests(_output);
            await RunEncryptionTestsAsync(encryptionRunner);
            
            // 5. 渗透测试
            var penTestRunner = new PenetrationTests(_output);
            await RunPenetrationTestsAsync(penTestRunner);
            
            // 6. 漏洞扫描
            var scanner = new VulnerabilityScanner(_output);
            await RunVulnerabilityScansAsync(scanner);
            
            var summary = _results.GetSummary();
            
            _logger.LogInformation("完整安全测试套件完成");
            _logger.LogInformation($"总测试数: {summary.TotalTests}");
            _logger.LogInformation($"通过: {summary.PassedTests}, 失败: {summary.FailedTests}");
            _logger.LogInformation($"成功率: {summary.SuccessRate:F1}%");
            
            // 生成报告
            await GenerateSecurityReportAsync();
            
            return summary;
        }

        /// <summary>
        /// 运行认证测试
        /// </summary>
        private async Task RunAuthenticationTestsAsync(AuthenticationSecurityTests runner)
        {
            try
            {
                await runner.PasswordComplexity_Should_EnforceStrongPasswords();
                await runner.AccountLockout_Should_PreventBruteForce();
                await runner.SessionManagement_Should_TimeoutProperly();
                await runner.JWTSecurity_Should_ValidateTokens();
                await runner.OAuthSecurity_Should_ValidateScopes();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "认证测试失败");
            }
        }

        /// <summary>
        /// 运行输入验证测试
        /// </summary>
        private async Task RunInputValidationTestsAsync(InputValidationSecurityTests runner)
        {
            try
            {
                await runner.XSSPrevention_Should_BlockMaliciousScripts();
                await runner.SQLInjectionPrevention_Should_BlockMaliciousQueries();
                await runner.CSRFProtection_Should_ValidateTokens();
                await runner.FileUploadSecurity_Should_ValidateFileTypes();
                await runner.InputLength_Should_BeLimited();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "输入验证测试失败");
            }
        }

        /// <summary>
        /// 运行HTTP安全测试
        /// </summary>
        private async Task RunHttpSecurityTestsAsync(HttpSecurityTests runner)
        {
            try
            {
                await runner.SecurityHeaders_Should_BePresent();
                await runner.HTTPS_Should_BeEnforced();
                await runner.CORS_Should_BeConfiguredProperly();
                await runner.ClickjackingProtection_Should_BeEnabled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP安全测试失败");
            }
        }

        /// <summary>
        /// 运行加密测试
        /// </summary>
        private async Task RunEncryptionTestsAsync(EncryptionSecurityTests runner)
        {
            try
            {
                await Task.Run(() => runner.AES256Encryption_Should_WorkCorrectly());
                await Task.Run(() => runner.HashingAlgorithm_Should_BeSecure());
                await Task.Run(() => runner.PasswordHashing_Should_UseSecureAlgorithm());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加密测试失败");
            }
        }

        /// <summary>
        /// 运行渗透测试
        /// </summary>
        private async Task RunPenetrationTestsAsync(PenetrationTests runner)
        {
            try
            {
                await runner.AuthenticationBypass_Should_NotBePossible();
                await runner.AuthorizationBypass_Should_NotBePossible();
                await runner.InformationDisclosure_Should_BePrevented();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "渗透测试失败");
            }
        }

        /// <summary>
        /// 运行漏洞扫描
        /// </summary>
        private async Task RunVulnerabilityScansAsync(VulnerabilityScanner scanner)
        {
            try
            {
                await scanner.ScanForKnownVulnerabilities();
                await scanner.ScanForDependencyVulnerabilities();
                await scanner.ScanForCodeSecurityIssues();
                await scanner.ScanForConfigurationSecurity();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "漏洞扫描失败");
            }
        }

        /// <summary>
        /// 运行特定类型的安全测试
        /// </summary>
        public async Task<SecurityTestSummary> RunSecurityTestsAsync(SecurityTestType testType)
        {
            _logger.LogInformation($"运行 {testType} 安全测试...");
            
            switch (testType)
            {
                case SecurityTestType.Authentication:
                    var authRunner = new AuthenticationSecurityTests(_output);
                    await RunAuthenticationTestsAsync(authRunner);
                    break;
                    
                case SecurityTestType.InputValidation:
                    var inputRunner = new InputValidationSecurityTests(_output);
                    await RunInputValidationTestsAsync(inputRunner);
                    break;
                    
                case SecurityTestType.HttpSecurity:
                    var httpRunner = new HttpSecurityTests(_output);
                    await RunHttpSecurityTestsAsync(httpRunner);
                    break;
                    
                case SecurityTestType.Encryption:
                    var encryptionRunner = new EncryptionSecurityTests(_output);
                    await RunEncryptionTestsAsync(encryptionRunner);
                    break;
                    
                case SecurityTestType.VulnerabilityScan:
                    var scanner = new VulnerabilityScanner(_output);
                    await RunVulnerabilityScansAsync(scanner);
                    break;
                    
                case SecurityTestType.PenetrationTest:
                    var penTestRunner = new PenetrationTests(_output);
                    await RunPenetrationTestsAsync(penTestRunner);
                    break;
                    
                default:
                    await RunFullSecurityTestSuiteAsync();
                    break;
            }
            
            return _results.GetSummary();
        }

        /// <summary>
        /// 生成安全测试报告
        /// </summary>
        public async Task GenerateSecurityReportAsync()
        {
            try
            {
                var report = new SecurityTestReport
                {
                    TestResults = _testResults,
                    ScanResults = _scanResults,
                    Summary = _results.GetSummary(),
                    GeneratedAt = DateTime.UtcNow,
                    TestEnvironment = Environment.MachineName
                };
                
                // 计算总体安全评分
                report.OverallSecurityScore = CalculateOverallSecurityScore(report);
                
                // 确保报告目录存在
                Directory.CreateDirectory("SecurityReports");
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var reportPath = $"SecurityReports/security-test-{timestamp}.json";
                var htmlReportPath = $"SecurityReports/security-test-{timestamp}.html";
                
                // 生成JSON报告
                var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(reportPath, json);
                
                // 生成HTML报告
                var htmlReport = GenerateHtmlReport(report);
                await File.WriteAllTextAsync(htmlReportPath, htmlReport);
                
                _logger.LogInformation($"安全测试报告已生成:");
                _logger.LogInformation($"JSON报告: {reportPath}");
                _logger.LogInformation($"HTML报告: {htmlReportPath}");
                _logger.LogInformation($"总体安全评分: {report.OverallSecurityScore:F1}/100");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成安全测试报告失败");
            }
        }

        /// <summary>
        /// 计算总体安全评分
        /// </summary>
        private double CalculateOverallSecurityScore(SecurityTestReport report)
        {
            if (report.Summary.TotalTests == 0)
                return 0;
            
            // 基础分数：测试成功率
            var baseScore = report.Summary.SuccessRate;
            
            // 漏洞扣分
            var totalVulnerabilities = report.ScanResults.Sum(r => r.Findings.Count);
            var criticalVulnerabilities = report.ScanResults.Sum(r => r.CriticalCount);
            var highVulnerabilities = report.ScanResults.Sum(r => r.HighCount);
            
            var vulnerabilityDeduction = Math.Min(50, 
                (criticalVulnerabilities * 10) + (highVulnerabilities * 5) + (totalVulnerabilities * 1));
            
            // 漏洞扫描评分
            var scanScore = report.ScanResults.Count > 0 
                ? report.ScanResults.Average(r => r.SecurityScore) 
                : 100;
            
            // 综合评分
            var overallScore = (baseScore * 0.4) + (scanScore * 0.6) - vulnerabilityDeduction;
            
            return Math.Max(0, Math.Min(100, overallScore));
        }

        /// <summary>
        /// 生成HTML报告
        /// </summary>
        private string GenerateHtmlReport(SecurityTestReport report)
        {
            var html = $@"
<!DOCTYPE html>
<html lang='zh-CN'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>KeyForge 安全测试报告</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; border-radius: 5px; }}
        .summary {{ background-color: #e9ecef; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .test-result {{ margin: 10px 0; padding: 10px; border-radius: 5px; }}
        .success {{ background-color: #d4edda; border-left: 4px solid #28a745; }}
        .failure {{ background-color: #f8d7da; border-left: 4px solid #dc3545; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; }}
        .vulnerability {{ margin: 10px 0; padding: 10px; border-radius: 5px; }}
        .critical {{ background-color: #f8d7da; border-left: 4px solid #dc3545; }}
        .high {{ background-color: #fd7e14; border-left: 4px solid #fd7e14; }}
        .medium {{ background-color: #ffc107; border-left: 4px solid #ffc107; }}
        .low {{ background-color: #28a745; border-left: 4px solid #28a745; }}
        .security-score {{ font-size: 24px; font-weight: bold; color: #28a745; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>KeyForge 安全测试报告</h1>
        <p>生成时间: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}</p>
        <p>测试环境: {report.TestEnvironment}</p>
        <div class='security-score'>总体安全评分: {report.OverallSecurityScore:F1}/100</div>
    </div>

    <div class='summary'>
        <h2>测试摘要</h2>
        <p>总测试数: {report.Summary.TotalTests}</p>
        <p>通过: {report.Summary.PassedTests}</p>
        <p>失败: {report.Summary.FailedTests}</p>
        <p>成功率: {report.Summary.SuccessRate:F1}%</p>
        <p>平均执行时间: {report.Summary.AverageExecutionTime:F2}ms</p>
    </div>

    <h2>测试结果详情</h2>
    <table>
        <thead>
            <tr>
                <th>测试名称</th>
                <th>类型</th>
                <th>状态</th>
                <th>执行时间</th>
                <th>错误信息</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var result in report.TestResults)
            {
                var statusClass = result.Success ? "success" : "failure";
                var statusText = result.Success ? "通过" : "失败";
                
                html += $@"
            <tr class='{statusClass}'>
                <td>{result.TestName}</td>
                <td>{result.TestType}</td>
                <td>{statusText}</td>
                <td>{result.Duration.TotalMilliseconds:F2}ms</td>
                <td>{result.ErrorMessage ?? ""}</td>
            </tr>";
            }

            html += @"
        </tbody>
    </table>";

            if (report.ScanResults.Any())
            {
                html += @"
    <h2>漏洞扫描结果</h2>
    <table>
        <thead>
            <tr>
                <th>扫描类型</th>
                <th>发现漏洞</th>
                <th>严重</th>
                <th>高</th>
                <th>中</th>
                <th>低</th>
                <th>安全评分</th>
            </tr>
        </thead>
        <tbody>";

                foreach (var scan in report.ScanResults)
                {
                    html += $@"
            <tr>
                <td>{scan.ScanType}</td>
                <td>{scan.Findings.Count}</td>
                <td>{scan.CriticalCount}</td>
                <td>{scan.HighCount}</td>
                <td>{scan.MediumCount}</td>
                <td>{scan.LowCount}</td>
                <td>{scan.SecurityScore:F1}</td>
            </tr>";
                }

                html += @"
        </tbody>
    </table>";
            }

            html += @"
</body>
</html>";

            return html;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private SecurityTestConfig LoadConfiguration()
        {
            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "security-test-config.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    return JsonSerializer.Deserialize<SecurityTestConfig>(json) ?? new SecurityTestConfig();
                }
                
                return new SecurityTestConfig();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载安全测试配置失败");
                return new SecurityTestConfig();
            }
        }

        /// <summary>
        /// 添加测试结果
        /// </summary>
        public void AddTestResult(SecurityTestResult result)
        {
            _testResults.Add(result);
            _results.AddResult(result);
        }

        /// <summary>
        /// 添加扫描结果
        /// </summary>
        public void AddScanResult(VulnerabilityScanResult result)
        {
            _scanResults.Add(result);
        }

        /// <summary>
        /// 获取测试摘要
        /// </summary>
        public SecurityTestSummary GetSummary()
        {
            return _results.GetSummary();
        }

        public void Dispose()
        {
            // 生成最终报告
            GenerateSecurityReportAsync().Wait();
        }
    }
}