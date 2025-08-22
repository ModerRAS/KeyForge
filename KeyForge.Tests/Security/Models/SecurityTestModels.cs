using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace KeyForge.Tests.Security
{
    /// <summary>
    /// 安全测试配置
    /// </summary>
    public class SecurityTestConfig
    {
        public AuthenticationTestSettings AuthenticationTests { get; set; } = new();
        public AuthorizationTestSettings AuthorizationTests { get; set; } = new();
        public InputValidationTestSettings InputValidationTests { get; set; } = new();
        public DataProtectionTestSettings DataProtectionTests { get; set; } = new();
        public NetworkSecurityTestSettings NetworkSecurityTests { get; set; } = new();
        public WebApplicationSecurityTestSettings WebApplicationSecurityTests { get; set; } = new();
        public VulnerabilityScanningSettings VulnerabilityScanning { get; set; } = new();
        public PenetrationTestSettings PenetrationTesting { get; set; } = new();
        public ComplianceTestSettings ComplianceTests { get; set; } = new();
        public PerformanceSecurityTestSettings PerformanceSecurityTests { get; set; } = new();
    }

    public class AuthenticationTestSettings
    {
        public bool TestPasswordComplexity { get; set; } = true;
        public bool TestAccountLockout { get; set; } = true;
        public bool TestSessionManagement { get; set; } = true;
        public bool TestOAuthSecurity { get; set; } = true;
        public bool TestJWTSecurity { get; set; } = true;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public int PasswordMinLength { get; set; } = 8;
        public bool PasswordRequireUppercase { get; set; } = true;
        public bool PasswordRequireLowercase { get; set; } = true;
        public bool PasswordRequireNumbers { get; set; } = true;
        public bool PasswordRequireSpecialChars { get; set; } = true;
    }

    public class AuthorizationTestSettings
    {
        public bool TestRoleBasedAccess { get; set; } = true;
        public bool TestPermissionBasedAccess { get; set; } = true;
        public bool TestResourceOwnership { get; set; } = true;
        public bool TestCrossTenantAccess { get; set; } = true;
        public bool TestAdminPrivileges { get; set; } = true;
        public List<string> DefaultRoles { get; set; } = new() { "User", "Admin", "Manager" };
        public List<string> AdminPermissions { get; set; } = new() { "*" };
        public List<string> UserPermissions { get; set; } = new() { "Read", "Create", "UpdateOwn" };
    }

    public class InputValidationTestSettings
    {
        public bool TestXssPrevention { get; set; } = true;
        public bool TestSqlInjectionPrevention { get; set; } = true;
        public bool TestCsrfProtection { get; set; } = true;
        public bool TestFileUploadSecurity { get; set; } = true;
        public bool TestApiValidation { get; set; } = true;
        public int MaxInputLength { get; set; } = 1000;
        public List<string> AllowedFileTypes { get; set; } = new() { ".jpg", ".png", ".gif", ".pdf" };
        public int MaxFileSizeMB { get; set; } = 10;
    }

    public class DataProtectionTestSettings
    {
        public bool TestEncryption { get; set; } = true;
        public bool TestDataMasking { get; set; } = true;
        public bool TestSecureStorage { get; set; } = true;
        public bool TestBackupSecurity { get; set; } = true;
        public bool TestCompliance { get; set; } = true;
        public string EncryptionAlgorithm { get; set; } = "AES-256";
        public int KeyRotationDays { get; set; } = 90;
        public int BackupRetentionDays { get; set; } = 30;
        public bool GDPRCompliance { get; set; } = true;
        public bool HIPAACompliance { get; set; } = false;
    }

    public class NetworkSecurityTestSettings
    {
        public bool TestHttpsSecurity { get; set; } = true;
        public bool TestApiSecurity { get; set; } = true;
        public bool TestFirewallRules { get; set; } = true;
        public bool TestRateLimiting { get; set; } = true;
        public bool TestDDoSProtection { get; set; } = true;
        public bool SSLEnforced { get; set; } = true;
        public bool SecurityHeaders { get; set; } = true;
        public bool CSPEnabled { get; set; } = true;
        public bool HSTSEnabled { get; set; } = true;
        public int RateLimitRequests { get; set; } = 100;
        public string RateLimitWindow { get; set; } = "00:01:00";
    }

    public class WebApplicationSecurityTestSettings
    {
        public bool TestClickjacking { get; set; } = true;
        public bool TestMimeSniffing { get; set; } = true;
        public bool TestContentTypeSniffing { get; set; } = true;
        public bool TestReferrerPolicy { get; set; } = true;
        public bool TestFrameOptions { get; set; } = true;
        public bool TestXSSProtection { get; set; } = true;
        public bool TestCorsPolicy { get; set; } = true;
        public List<string> AllowedOrigins { get; set; } = new() { "https://localhost:5001" };
        public List<string> AllowedMethods { get; set; } = new() { "GET", "POST", "PUT", "DELETE" };
        public List<string> AllowedHeaders { get; set; } = new() { "Content-Type", "Authorization" };
    }

    public class VulnerabilityScanningSettings
    {
        public bool TestKnownVulnerabilities { get; set; } = true;
        public bool TestDependencyVulnerabilities { get; set; } = true;
        public bool TestCodeSecurity { get; set; } = true;
        public bool TestConfigurationSecurity { get; set; } = true;
        public bool TestContainerSecurity { get; set; } = true;
        public string ScanFrequency { get; set; } = "Weekly";
        public List<string> SeverityLevels { get; set; } = new() { "Critical", "High", "Medium", "Low" };
        public List<string> IgnoreFindings { get; set; } = new();
        public bool UpdateDatabase { get; set; } = true;
    }

    public class PenetrationTestSettings
    {
        public bool TestAuthenticationBypass { get; set; } = true;
        public bool TestAuthorizationBypass { get; set; } = true;
        public bool TestDataExposure { get; set; } = true;
        public bool TestInjectionAttacks { get; set; } = true;
        public bool TestCrossSiteScripting { get; set; } = true;
        public bool TestInsecureDeserialization { get; set; } = true;
        public bool TestSecurityMisconfiguration { get; set; } = true;
        public bool TestSensitiveDataExposure { get; set; } = true;
        public bool TestBrokenAccessControl { get; set; } = true;
        public bool TestServerSideRequestForgery { get; set; } = true;
    }

    public class ComplianceTestSettings
    {
        public bool TestGDPRCompliance { get; set; } = true;
        public bool TestCCPACompliance { get; set; } = true;
        public bool TestSOC2Compliance { get; set; } = false;
        public bool TestISO27001Compliance { get; set; } = false;
        public bool TestPCICompliance { get; set; } = false;
        public int DataRetentionDays { get; set; } = 365;
        public bool RightToBeForgotten { get; set; } = true;
        public bool DataPortability { get; set; } = true;
        public bool ConsentManagement { get; set; } = true;
    }

    public class PerformanceSecurityTestSettings
    {
        public bool TestDoSResistance { get; set; } = true;
        public bool TestBruteForceProtection { get; set; } = true;
        public bool TestResourceExhaustion { get; set; } = true;
        public bool TestMemorySafety { get; set; } = true;
        public bool TestThreadSafety { get; set; } = true;
        public int MaxRequestsPerSecond { get; set; } = 1000;
        public int MaxMemoryUsageMB { get; set; } = 512;
        public int MaxCpuUsagePercent { get; set; } = 80;
        public int ConnectionTimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// 安全测试类型
    /// </summary>
    public enum SecurityTestType
    {
        General,
        Authentication,
        Authorization,
        InputValidation,
        Encryption,
        HttpSecurity,
        VulnerabilityScan,
        PenetrationTest,
        Compliance,
        Performance
    }

    /// <summary>
    /// 漏洞严重性
    /// </summary>
    public enum VulnerabilitySeverity
    {
        Critical,
        High,
        Medium,
        Low,
        Info
    }

    /// <summary>
    /// 安全测试结果
    /// </summary>
    public class SecurityTestResult
    {
        public string TestName { get; set; } = string.Empty;
        public SecurityTestType TestType { get; set; } = SecurityTestType.General;
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Warning { get; set; }
        public Exception? Exception { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    /// <summary>
    /// 漏洞扫描结果
    /// </summary>
    public class VulnerabilityScanResult
    {
        public string ScanType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public List<VulnerabilityFinding> Findings { get; set; } = new();
        public int CriticalCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public int LowCount { get; set; }
        public double SecurityScore { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 漏洞发现
    /// </summary>
    public class VulnerabilityFinding
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VulnerabilitySeverity Severity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? CodeSnippet { get; set; }
        public string? Recommendation { get; set; }
        public string? Remediation { get; set; }
        public List<string> References { get; set; } = new();
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
        public bool IsFalsePositive { get; set; }
        public string? AssignedTo { get; set; }
        public VulnerabilityStatus Status { get; set; } = VulnerabilityStatus.Open;
    }

    /// <summary>
    /// 漏洞状态
    /// </summary>
    public enum VulnerabilityStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed,
        FalsePositive
    }

    /// <summary>
    /// 安全头检查结果
    /// </summary>
    public class SecurityHeadersResult
    {
        public List<string> PresentHeaders { get; set; } = new();
        public List<string> MissingHeaders { get; set; } = new();
        public bool AllSecure { get; set; }
    }

    /// <summary>
    /// 漏洞数据库
    /// </summary>
    public class VulnerabilityDatabase
    {
        public List<OWASPTop10Item> OWASPTop10 { get; set; } = new();
        public List<SASTRule> SASTRules { get; set; } = new();
    }

    /// <summary>
    /// OWASP Top 10项目
    /// </summary>
    public class OWASPTop10Item
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VulnerabilitySeverity Severity { get; set; }
        public List<string> TestCases { get; set; } = new();
    }

    /// <summary>
    /// SAST规则
    /// </summary>
    public class SASTRule
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VulnerabilitySeverity Severity { get; set; }
        public string Pattern { get; set; } = string.Empty;
    }

    /// <summary>
    /// 安全测试结果收集器
    /// </summary>
    public class SecurityTestResultCollector
    {
        private readonly List<SecurityTestResult> _results = new();

        public void AddResult(SecurityTestResult result)
        {
            _results.Add(result);
        }

        public SecurityTestSummary GetSummary()
        {
            if (!_results.Any())
            {
                return new SecurityTestSummary();
            }

            return new SecurityTestSummary
            {
                TotalTests = _results.Count,
                PassedTests = _results.Count(r => r.Success),
                FailedTests = _results.Count(r => !r.Success),
                AverageExecutionTime = _results.Average(r => r.Duration.TotalMilliseconds),
                MinExecutionTime = _results.Min(r => r.Duration.TotalMilliseconds),
                MaxExecutionTime = _results.Max(r => r.Duration.TotalMilliseconds),
                SuccessRate = (double)_results.Count(r => r.Success) / _results.Count * 100,
                TestTypeDistribution = _results.GroupBy(r => r.TestType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
    }

    /// <summary>
    /// 安全测试摘要
    /// </summary>
    public class SecurityTestSummary
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public double AverageExecutionTime { get; set; }
        public double MinExecutionTime { get; set; }
        public double MaxExecutionTime { get; set; }
        public double SuccessRate { get; set; }
        public Dictionary<SecurityTestType, int> TestTypeDistribution { get; set; } = new();
    }

    /// <summary>
    /// 安全测试报告
    /// </summary>
    public class SecurityTestReport
    {
        public List<SecurityTestResult> TestResults { get; set; } = new();
        public SecurityTestSummary Summary { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string TestEnvironment { get; set; } = string.Empty;
        public double OverallSecurityScore { get; set; }
        public List<VulnerabilityScanResult> ScanResults { get; set; } = new();
    }
}