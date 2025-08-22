using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace KeyForge.Tests.Security
{
    /// <summary>
    /// KeyForge 安全测试基类
    /// </summary>
    public class SecurityTestBase : IDisposable
    {
        protected readonly ITestOutputHelper Output;
        protected readonly IConfiguration Configuration;
        protected readonly ILogger<SecurityTestBase> Logger;
        protected readonly SecurityTestConfig SecurityConfig;
        protected readonly VulnerabilityDatabase VulnerabilityDb;
        
        protected WebApplicationFactory<Program> ApplicationFactory;
        protected HttpClient TestClient;
        
        // 测试结果收集器
        protected readonly SecurityTestResultCollector Results = new SecurityTestResultCollector();
        
        // 测试数据
        protected readonly List<SecurityTestResult> TestResults = new List<SecurityTestResult>();

        public SecurityTestBase(ITestOutputHelper output)
        {
            Output = output;
            
            // 加载配置
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("security-test-config.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            // 配置日志
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXunit(output);
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            Logger = loggerFactory.CreateLogger<SecurityTestBase>();
            
            // 加载安全测试配置
            SecurityConfig = Configuration.GetSection("SecurityTestSettings").Get<SecurityTestConfig>() 
                ?? new SecurityTestConfig();
            
            // 加载漏洞数据库
            VulnerabilityDb = LoadVulnerabilityDatabase();
            
            // 初始化测试应用
            InitializeTestApplication();
            
            Output.WriteLine("Security Test Base initialized");
        }

        private VulnerabilityDatabase LoadVulnerabilityDatabase()
        {
            try
            {
                var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "vulnerability-database.json");
                if (File.Exists(dbPath))
                {
                    var json = File.ReadAllText(dbPath);
                    return JsonSerializer.Deserialize<VulnerabilityDatabase>(json) 
                        ?? new VulnerabilityDatabase();
                }
                
                // 返回默认漏洞数据库
                return new VulnerabilityDatabase
                {
                    OWASPTop10 = GetDefaultOWASPTop10(),
                    SASTRules = GetDefaultSASTRules()
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load vulnerability database");
                return new VulnerabilityDatabase();
            }
        }

        private void InitializeTestApplication()
        {
            try
            {
                ApplicationFactory = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.ConfigureAppConfiguration((context, config) =>
                        {
                            config.AddJsonFile("appsettings.json");
                            config.AddEnvironmentVariables();
                        });
                        
                        builder.ConfigureLogging(logging =>
                        {
                            logging.AddXunit(Output);
                            logging.SetMinimumLevel(LogLevel.Information);
                        });
                    });
                
                TestClient = ApplicationFactory.CreateClient();
                Logger.LogInformation("Test application initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize test application");
                throw;
            }
        }

        /// <summary>
        /// 运行安全测试
        /// </summary>
        protected async Task RunSecurityTestAsync(string testName, Func<Task> testAction)
        {
            var result = new SecurityTestResult
            {
                TestName = testName,
                StartTime = DateTime.UtcNow,
                TestType = SecurityTestType.General
            };
            
            try
            {
                Output.WriteLine($"Running security test: {testName}");
                
                await testAction();
                
                result.Success = true;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                
                Output.WriteLine($"Security test passed: {testName}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                
                Logger.LogError(ex, "Security test failed: {TestName}", testName);
                Output.WriteLine($"Security test failed: {testName} - {ex.Message}");
                
                throw;
            }
            finally
            {
                TestResults.Add(result);
                Results.AddResult(result);
            }
        }

        /// <summary>
        /// 运行漏洞扫描
        /// </summary>
        protected async Task<VulnerabilityScanResult> RunVulnerabilityScanAsync(
            string scanType, 
            Func<Task<List<VulnerabilityFinding>>> scanAction)
        {
            var result = new VulnerabilityScanResult
            {
                ScanType = scanType,
                StartTime = DateTime.UtcNow
            };
            
            try
            {
                Output.WriteLine($"Running vulnerability scan: {scanType}");
                
                result.Findings = await scanAction();
                
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                
                // 计算严重性统计
                result.CriticalCount = result.Findings.Count(f => f.Severity == VulnerabilitySeverity.Critical);
                result.HighCount = result.Findings.Count(f => f.Severity == VulnerabilitySeverity.High);
                result.MediumCount = result.Findings.Count(f => f.Severity == VulnerabilitySeverity.Medium);
                result.LowCount = result.Findings.Count(f => f.Severity == VulnerabilitySeverity.Low);
                
                // 计算安全评分
                result.SecurityScore = CalculateSecurityScore(result.Findings);
                
                Output.WriteLine($"Vulnerability scan completed: {scanType} - Found {result.Findings.Count} vulnerabilities");
            }
            catch (Exception ex)
            {
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.ErrorMessage = ex.Message;
                
                Logger.LogError(ex, "Vulnerability scan failed: {ScanType}", scanType);
                Output.WriteLine($"Vulnerability scan failed: {scanType} - {ex.Message}");
            }
            
            return result;
        }

        /// <summary>
        /// 测试HTTP请求安全性
        /// </summary>
        protected async Task<SecurityTestResult> TestHttpRequestSecurityAsync(
            string url, 
            HttpMethod method, 
            HttpContent content = null,
            Dictionary<string, string> headers = null)
        {
            var result = new SecurityTestResult
            {
                TestName = $"HTTP Security Test: {method} {url}",
                TestType = SecurityTestType.HttpSecurity,
                StartTime = DateTime.UtcNow
            };
            
            try
            {
                var request = new HttpRequestMessage(method, url);
                
                if (content != null)
                {
                    request.Content = content;
                }
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
                
                var response = await TestClient.SendAsync(request);
                
                result.Metrics.Add("StatusCode", response.StatusCode);
                result.Metrics.Add("ResponseTime", response.Headers.Date?.DateTime - result.StartTime);
                
                // 检查安全头
                var securityHeaders = CheckSecurityHeaders(response);
                result.Metrics.Add("SecurityHeaders", securityHeaders);
                
                // 检查内容类型
                var contentType = response.Content.Headers.ContentType?.MediaType;
                result.Metrics.Add("ContentType", contentType);
                
                result.Success = response.IsSuccessStatusCode;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                
                // 记录结果
                Output.WriteLine($"HTTP Security Test: {method} {url} - {response.StatusCode}");
                
                if (!securityHeaders.AllSecure)
                {
                    result.Warning = "Missing security headers: " + string.Join(", ", securityHeaders.MissingHeaders);
                    Output.WriteLine($"Warning: {result.Warning}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.ErrorMessage = ex.Message;
                
                Logger.LogError(ex, "HTTP security test failed: {Method} {Url}", method, url);
            }
            
            TestResults.Add(result);
            return result;
        }

        /// <summary>
        /// 检查安全头
        /// </summary>
        protected SecurityHeadersResult CheckSecurityHeaders(HttpResponseMessage response)
        {
            var result = new SecurityHeadersResult();
            
            var requiredHeaders = new[]
            {
                "X-Content-Type-Options",
                "X-Frame-Options",
                "X-XSS-Protection",
                "Strict-Transport-Security",
                "Content-Security-Policy",
                "Referrer-Policy"
            };
            
            foreach (var header in requiredHeaders)
            {
                if (response.Headers.Contains(header))
                {
                    result.PresentHeaders.Add(header);
                }
                else
                {
                    result.MissingHeaders.Add(header);
                }
            }
            
            result.AllSecure = result.MissingHeaders.Count == 0;
            
            return result;
        }

        /// <summary>
        /// 测试输入验证
        /// </summary>
        protected async Task<SecurityTestResult> TestInputValidationAsync(
            string endpoint, 
            string maliciousInput, 
            string expectedBehavior)
        {
            var result = new SecurityTestResult
            {
                TestName = $"Input Validation Test: {endpoint}",
                TestType = SecurityTestType.InputValidation,
                StartTime = DateTime.UtcNow
            };
            
            try
            {
                var response = await TestClient.PostAsJsonAsync(endpoint, new { input = maliciousInput });
                
                result.Metrics.Add("StatusCode", response.StatusCode);
                result.Metrics.Add("MaliciousInput", maliciousInput);
                result.Metrics.Add("ExpectedBehavior", expectedBehavior);
                
                // 检查是否成功阻止了恶意输入
                var isBlocked = response.StatusCode == System.Net.HttpStatusCode.BadRequest 
                    || response.StatusCode == System.Net.HttpStatusCode.Forbidden;
                
                result.Success = isBlocked;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                
                Output.WriteLine($"Input Validation Test: {endpoint} - {(isBlocked ? "Blocked" : "Allowed")}");
                
                if (!isBlocked)
                {
                    result.Warning = $"Malicious input was not blocked: {maliciousInput}";
                    Output.WriteLine($"Warning: {result.Warning}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.ErrorMessage = ex.Message;
                
                Logger.LogError(ex, "Input validation test failed: {Endpoint}", endpoint);
            }
            
            TestResults.Add(result);
            return result;
        }

        /// <summary>
        /// 测试认证和授权
        /// </summary>
        protected async Task<SecurityTestResult> TestAuthenticationAsync(
            string endpoint, 
            bool requiresAuth = true)
        {
            var result = new SecurityTestResult
            {
                TestName = $"Authentication Test: {endpoint}",
                TestType = SecurityTestType.Authentication,
                StartTime = DateTime.UtcNow
            };
            
            try
            {
                var response = await TestClient.GetAsync(endpoint);
                
                result.Metrics.Add("StatusCode", response.StatusCode);
                result.Metrics.Add("RequiresAuth", requiresAuth);
                
                var expectedStatus = requiresAuth 
                    ? System.Net.HttpStatusCode.Unauthorized 
                    : System.Net.HttpStatusCode.OK;
                
                result.Success = response.StatusCode == expectedStatus;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                
                Output.WriteLine($"Authentication Test: {endpoint} - {response.StatusCode}");
                
                if (!result.Success)
                {
                    result.Warning = $"Authentication test failed: expected {expectedStatus}, got {response.StatusCode}";
                    Output.WriteLine($"Warning: {result.Warning}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.ErrorMessage = ex.Message;
                
                Logger.LogError(ex, "Authentication test failed: {Endpoint}", endpoint);
            }
            
            TestResults.Add(result);
            return result;
        }

        /// <summary>
        /// 测试加密和哈希
        /// </summary>
        protected SecurityTestResult TestEncryptionAlgorithm(
            string algorithmName, 
            Func<byte[], byte[]> encryptFunc, 
            Func<byte[], byte[]> decryptFunc)
        {
            var result = new SecurityTestResult
            {
                TestName = $"Encryption Test: {algorithmName}",
                TestType = SecurityTestType.Encryption,
                StartTime = DateTime.UtcNow
            };
            
            try
            {
                var testData = Encoding.UTF8.GetBytes("Test data for encryption");
                
                // 加密
                var encrypted = encryptFunc(testData);
                
                // 解密
                var decrypted = decryptFunc(encrypted);
                
                // 验证
                var isValid = testData.SequenceEqual(decrypted);
                
                result.Metrics.Add("DataLength", testData.Length);
                result.Metrics.Add("EncryptedLength", encrypted.Length);
                result.Metrics.Add("EncryptionValid", isValid);
                
                result.Success = isValid;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                
                Output.WriteLine($"Encryption Test: {algorithmName} - {(isValid ? "Valid" : "Invalid")}");
                
                if (!isValid)
                {
                    result.Warning = "Encryption/decryption failed - data does not match";
                    Output.WriteLine($"Warning: {result.Warning}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.ErrorMessage = ex.Message;
                
                Logger.LogError(ex, "Encryption test failed: {Algorithm}", algorithmName);
            }
            
            TestResults.Add(result);
            return result;
        }

        /// <summary>
        /// 计算安全评分
        /// </summary>
        protected double CalculateSecurityScore(List<VulnerabilityFinding> findings)
        {
            if (findings.Count == 0)
                return 100.0;
            
            // 权重计算
            var criticalWeight = 10.0;
            var highWeight = 5.0;
            var mediumWeight = 2.0;
            var lowWeight = 1.0;
            
            var weightedScore = findings.Sum(f => f.Severity switch
            {
                VulnerabilitySeverity.Critical => criticalWeight,
                VulnerabilitySeverity.High => highWeight,
                VulnerabilitySeverity.Medium => mediumWeight,
                VulnerabilitySeverity.Low => lowWeight,
                _ => 0.5
            });
            
            // 最大可能分数
            var maxScore = findings.Count * criticalWeight;
            
            // 计算安全评分（100 - 加权分数百分比）
            var securityScore = Math.Max(0, 100 - (weightedScore / maxScore * 100));
            
            return Math.Round(securityScore, 2);
        }

        /// <summary>
        /// 生成安全测试报告
        /// </summary>
        protected void GenerateSecurityReport()
        {
            var report = new SecurityTestReport
            {
                TestResults = TestResults,
                Summary = Results.GetSummary(),
                GeneratedAt = DateTime.UtcNow,
                TestEnvironment = Environment.MachineName
            };
            
            var reportPath = Path.Combine("SecurityReports", $"security-test-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
            Directory.CreateDirectory("SecurityReports");
            
            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            File.WriteAllText(reportPath, json);
            
            Output.WriteLine($"Security test report generated: {reportPath}");
        }

        // 默认OWASP Top 10数据
        private List<OWASPTop10Item> GetDefaultOWASPTop10()
        {
            return new List<OWASPTop10Item>
            {
                new OWASPTop10Item
                {
                    Id = "A01:2021",
                    Name = "Broken Access Control",
                    Description = "访问控制失效",
                    Severity = VulnerabilitySeverity.Critical,
                    TestCases = new List<string> { "BrokenAccessControlTests" }
                },
                new OWASPTop10Item
                {
                    Id = "A02:2021",
                    Name = "Cryptographic Failures",
                    Description = "加密机制失效",
                    Severity = VulnerabilitySeverity.Critical,
                    TestCases = new List<string> { "EncryptionTests", "DataProtectionTests" }
                },
                new OWASPTop10Item
                {
                    Id = "A03:2021",
                    Name = "Injection",
                    Description = "注入攻击",
                    Severity = VulnerabilitySeverity.High,
                    TestCases = new List<string> { "SqlInjectionTests", "XssTests" }
                }
            };
        }

        // 默认SAST规则
        private List<SASTRule> GetDefaultSASTRules()
        {
            return new List<SASTRule>
            {
                new SASTRule
                {
                    Id = "SAST001",
                    Name = "Hardcoded Secrets",
                    Description = "硬编码密钥",
                    Severity = VulnerabilitySeverity.Critical,
                    Pattern = "password|secret|key|token"
                },
                new SASTRule
                {
                    Id = "SAST002",
                    Name = "SQL Injection",
                    Description = "SQL注入漏洞",
                    Severity = VulnerabilitySeverity.High,
                    Pattern = "ExecuteReader|ExecuteNonQuery|ExecuteScalar"
                }
            };
        }

        public void Dispose()
        {
            TestClient?.Dispose();
            ApplicationFactory?.Dispose();
            
            // 生成测试报告
            GenerateSecurityReport();
        }
    }
}