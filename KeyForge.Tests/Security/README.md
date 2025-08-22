# KeyForge 安全测试套件

## 概述

KeyForge安全测试套件提供了全面的安全测试解决方案，包括认证和授权测试、输入验证测试、HTTP安全测试、加密测试、漏洞扫描和渗透测试。这些测试帮助确保系统在各种安全威胁下的防护能力。

## 安全测试类型

### 1. 认证和授权测试 (Authentication & Authorization Tests)
- **目的**: 测试身份验证和授权机制的安全性
- **测试内容**:
  - 密码复杂度验证
  - 账户锁定机制
  - 会话管理
  - JWT令牌验证
  - OAuth安全性
  - 权限控制

### 2. 输入验证测试 (Input Validation Tests)
- **目的**: 测试输入验证和过滤机制
- **测试内容**:
  - XSS防护
  - SQL注入防护
  - CSRF防护
  - 文件上传安全
  - 输入长度限制
  - 恶意输入过滤

### 3. HTTP安全测试 (HTTP Security Tests)
- **目的**: 测试HTTP层面的安全防护
- **测试内容**:
  - 安全头检查
  - HTTPS强制
  - CORS配置
  - 点击劫持防护
  - Cookie安全设置

### 4. 加密安全测试 (Encryption Security Tests)
- **目的**: 测试加密和数据保护机制
- **测试内容**:
  - AES-256加密
  - SHA-256哈希
  - 密码哈希
  - 密钥管理
  - 数据加密

### 5. 漏洞扫描 (Vulnerability Scanning)
- **目的**: 扫描已知的漏洞和安全问题
- **测试内容**:
  - SQL注入漏洞
  - XSS漏洞
  - 路径遍历漏洞
  - 依赖项漏洞
  - 配置安全问题
  - 代码安全问题

### 6. 渗透测试 (Penetration Testing)
- **目的**: 模拟真实攻击测试系统安全性
- **测试内容**:
  - 认证绕过
  - 授权绕过
  - 信息泄露
  - 提权攻击
  - 横向移动

## 快速开始

### 前置条件

1. **.NET 8.0 SDK**
2. **足够的系统资源** (推荐4GB+ RAM)
3. **Visual Studio 2022** 或 **.NET CLI**
4. **可选**: jq工具 (用于解析JSON报告)

### 运行测试

#### 使用命令行

```bash
# 进入测试项目目录
cd KeyForge.Tests/Security

# 运行快速安全检查
./run-security-tests.sh quick
# 或
run-security-tests.bat quick

# 运行认证测试
./run-security-tests.sh auth

# 运行输入验证测试
./run-security-tests.sh input

# 运行HTTP安全测试
./run-security-tests.sh http

# 运行加密测试
./run-security-tests.sh encryption

# 运行漏洞扫描
./run-security-tests.sh scan

# 运行渗透测试
./run-security-tests.sh pentest

# 运行完整安全测试套件
./run-security-tests.sh full

# 生成安全测试报告
./run-security-tests.sh report
```

#### 使用Visual Studio

1. 打开解决方案
2. 设置`KeyForge.Tests.Security`为启动项目
3. 按F5运行程序
4. 按照提示选择测试类型

## 配置

### 安全测试配置

编辑`security-test-config.json`文件来自定义测试参数：

```json
{
  "SecurityTestSettings": {
    "AuthenticationTests": {
      "TestPasswordComplexity": true,
      "MaxLoginAttempts": 5,
      "LockoutDurationMinutes": 15,
      "PasswordMinLength": 8
    },
    "InputValidationTests": {
      "TestXssPrevention": true,
      "TestSqlInjectionPrevention": true,
      "MaxInputLength": 1000
    },
    "VulnerabilityScanning": {
      "TestKnownVulnerabilities": true,
      "ScanFrequency": "Weekly",
      "SeverityLevels": ["Critical", "High", "Medium", "Low"]
    }
  }
}
```

### 测试配置文件

- `security-test-config.json` - 安全测试配置
- `vulnerability-database.json` - 漏洞数据库
- `SecurityRules.ruleset` - 安全规则集
- `SecurityReports/` - 安全测试报告

## 安全测试结果

### 测试报告

安全测试完成后会生成以下报告：

- **JSON报告**: `SecurityReports/security-test-YYYYMMDD-HHMMSS.json`
- **HTML报告**: `SecurityReports/security-test-YYYYMMDD-HHMMSS.html`
- **日志文件**: `logs/` 目录下的各个测试日志

### 安全评分

系统会根据测试结果计算安全评分：

- **90-100分**: 安全状况良好
- **80-89分**: 安全状况中等，需要改进
- **70-79分**: 存在较多安全问题
- **低于70分**: 安全状况较差，需要立即处理

### 漏洞严重性

漏洞按严重性分为：

- **Critical**: 严重漏洞，需要立即修复
- **High**: 高危漏洞，建议尽快修复
- **Medium**: 中危漏洞，建议修复
- **Low**: 低危漏洞，可选修复

## 漏洞数据库

### OWASP Top 10

系统包含完整的OWASP Top 10漏洞数据库：

1. **A01:2021** - 访问控制失效
2. **A02:2021** - 加密机制失效
3. **A03:2021** - 注入攻击
4. **A04:2021** - 不安全的设计
5. **A05:2021** - 安全配置错误
6. **A06:2021** - 易受攻击和过时的组件
7. **A07:2021** - 识别和认证失效
8. **A08:2021** - 软件和数据完整性失效
9. **A09:2021** - 安全日志和监控失效
10. **A10:2021** - 服务器端请求伪造

### SAST规则

系统包含静态应用程序安全测试(SAST)规则：

- **硬编码密钥检测**
- **SQL注入检测**
- **XSS漏洞检测**
- **路径遍历检测**
- **不安全随机数检测**
- **弱加密算法检测**

## 测试用例示例

### 认证测试

```csharp
[Fact]
public async Task PasswordComplexity_Should_EnforceStrongPasswords()
{
    var weakPasswords = new[]
    {
        "password",
        "123456",
        "qwerty",
        "letmein"
    };

    foreach (var weakPassword in weakPasswords)
    {
        var response = await TestClient.PostAsJsonAsync("/api/auth/register", new
        {
            Email = $"test_{weakPassword}@example.com",
            Password = weakPassword,
            Name = "Test User"
        });

        // 应该拒绝弱密码
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest,
            $"Weak password '{weakPassword}' was accepted");
    }
}
```

### 漏洞扫描

```csharp
[Fact]
public async Task ScanForXssVulnerabilities()
{
    var xssPayloads = new[]
    {
        "<script>alert('XSS')</script>",
        "<img src=x onerror=alert('XSS')>",
        "javascript:alert('XSS')"
    };

    foreach (var payload in xssPayloads)
    {
        var response = await TestClient.PostAsJsonAsync("/api/comments", new
        {
            Comment = payload
        });

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // 检查XSS payload是否被反射
            if (content.Contains(payload))
            {
                findings.Add(new VulnerabilityFinding
                {
                    Title = "Potential XSS Vulnerability",
                    Severity = VulnerabilitySeverity.High,
                    Recommendation = "Implement proper input sanitization"
                });
            }
        }
    }
}
```

## 最佳实践

### 安全测试执行

1. **环境准备**
   - 使用测试环境进行安全测试
   - 确保测试数据不包含真实敏感信息
   - 配置适当的测试权限

2. **测试执行**
   - 定期运行安全测试（建议每周）
   - 在代码部署前运行安全测试
   - 监控安全测试结果

3. **结果分析**
   - 关注Critical和High级别的漏洞
   - 分析漏洞的根本原因
   - 制定修复计划

### 安全建议

1. **代码安全**
   - 使用安全的编程实践
   - 定期更新依赖项
   - 实施代码审查

2. **配置安全**
   - 禁用不必要的服务
   - 使用安全配置
   - 定期审查配置

3. **监控和响应**
   - 实施安全监控
   - 建立安全事件响应流程
   - 定期进行安全培训

## 故障排除

### 常见问题

1. **测试失败**
   - 检查测试环境配置
   - 查看详细日志文件
   - 验证测试数据

2. **性能问题**
   - 优化测试参数
   - 增加系统资源
   - 分批次运行测试

3. **误报问题**
   - 调整测试配置
   - 更新漏洞数据库
   - 优化检测规则

### 日志文件

- `logs/quick-security-test.log` - 快速安全测试日志
- `logs/auth-test.log` - 认证测试日志
- `logs/vulnerability-scan.log` - 漏洞扫描日志
- `logs/full-security-test.log` - 完整测试套件日志
- `logs/report-generation.log` - 报告生成日志

## 扩展测试

### 添加新的安全测试

```csharp
public class CustomSecurityTests : SecurityTestBase
{
    public CustomSecurityTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task CustomSecurityTest()
    {
        await RunSecurityTestAsync("Custom Security Test", async () =>
        {
            // 实现自定义安全测试逻辑
        });
    }
}
```

### 添加新的漏洞检测规则

在`vulnerability-database.json`中添加新的检测规则：

```json
{
  "SASTRules": [
    {
      "Id": "SAST999",
      "Name": "Custom Security Rule",
      "Description": "自定义安全规则",
      "Severity": "High",
      "Pattern": "custom_pattern"
    }
  ]
}
```

### 添加新的攻击模式

```json
{
  "AttackPatterns": {
    "CustomAttacks": [
      {
        "Pattern": "custom_attack_pattern",
        "Description": "自定义攻击模式"
      }
    ]
  }
}
```

## 集成到CI/CD

### GitHub Actions

```yaml
name: Security Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  security-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Run Security Tests
      run: |
        cd KeyForge.Tests/Security
        ./run-security-tests.sh quick
    
    - name: Upload Security Reports
      uses: actions/upload-artifact@v3
      with:
        name: security-reports
        path: SecurityReports/
```

### Azure DevOps

```yaml
trigger:
- main
- develop

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'
  
- script: |
    cd KeyForge.Tests/Security
    ./run-security-tests.sh quick
  displayName: 'Run Security Tests'
  
- task: PublishBuildArtifacts@1
  inputs:
    pathtoPublish: 'SecurityReports'
    artifactName: 'security-reports'
```

## 性能优化

### 测试性能优化

1. **并行测试**
   - 使用xunit的并行测试功能
   - 分布式测试执行
   - 优化测试数据准备

2. **缓存优化**
   - 缓存测试结果
   - 重复使用测试数据
   - 优化数据库连接

3. **资源优化**
   - 合理配置并发数
   - 优化内存使用
   - 监控系统资源

## 安全合规

### 合规性测试

系统支持多种合规性测试：

- **GDPR**: 通用数据保护条例
- **CCPA**: 加州消费者隐私法案
- **SOC2**: 服务组织控制报告
- **ISO 27001**: 信息安全管理体系

### 数据保护

- 数据加密存储
- 访问日志记录
- 数据删除功能
- 数据导出功能

## 许可证

本安全测试套件遵循与主项目相同的许可证。

## 贡献指南

### 报告安全问题

发现安全问题时，请通过以下方式报告：

1. 创建GitHub Issue (使用私有仓库)
2. 发送邮件到安全团队
3. 使用安全报告工具

### 贡献测试用例

1. Fork项目
2. 创建功能分支
3. 添加测试用例
4. 提交合并请求

## 联系信息

如有安全问题或建议，请通过以下方式联系：

- 安全团队邮箱: security@keyforge.com
- GitHub Issues: 创建私有issue
- 紧急安全问题: 使用紧急联系渠道

---

*最后更新: 2024-01-01*