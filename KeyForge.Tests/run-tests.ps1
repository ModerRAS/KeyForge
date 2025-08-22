# KeyForge 测试运行脚本
# 原本实现：复杂的测试运行脚本
# 简化实现：基础的测试运行功能

param(
    [string]$Configuration = "Debug",
    [string]$TestCategory = "All",
    [string]$Filter = "",
    [switch]$NoCoverage = $false,
    [switch]$NoReport = $false,
    [switch]$Verbose = $false,
    [switch]$Clean = $false,
    [switch]$Help = $false
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
KeyForge 测试运行脚本

用法: run-tests.ps1 [选项]

选项:
    -Configuration <string>    构建配置 (Debug/Release) [默认: Debug]
    -TestCategory <string>    测试类别 (All/Unit/Integration/Performance/E2E) [默认: All]
    -Filter <string>         测试过滤器
    -NoCoverage              不收集代码覆盖率
    -NoReport                不生成测试报告
    -Verbose                 详细输出
    -Clean                   清理之前的测试结果
    -Help                    显示此帮助信息

示例:
    run-tests.ps1 -Category Unit -Verbose
    run-tests.ps1 -Configuration Release -NoCoverage
    run-tests.ps1 -Filter "Performance" -Clean
"@
    exit 0
}

# 设置脚本路径
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectRoot = Split-Path -Parent $ScriptPath
$TestProjectPath = Join-Path $ProjectRoot "KeyForge.Tests"
$ResultsPath = Join-Path $ScriptPath "TestResults"

# 创建结果目录
if (-not (Test-Path $ResultsPath)) {
    New-Item -ItemType Directory -Path $ResultsPath -Force | Out-Null
}

# 日志函数
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    Write-Host $logMessage
    
    # 写入日志文件
    $logFile = Join-Path $ResultsPath "test-run.log"
    Add-Content -Path $logFile -Value $logMessage
}

function Write-VerboseLog {
    param([string]$Message)
    
    if ($Verbose) {
        Write-Log -Message $Message -Level "VERBOSE"
    }
}

# 清理之前的测试结果
if ($Clean) {
    Write-Log "清理之前的测试结果..."
    if (Test-Path $ResultsPath) {
        Remove-Item -Path $ResultsPath -Recurse -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Path $ResultsPath -Force | Out-Null
    }
    Write-Log "清理完成"
}

# 测试类别配置
$TestCategoryConfig = @{
    "All" = @{
        "Include" = @("**/UnitTests/**/*.cs", "**/IntegrationTests/**/*.cs", "**/PerformanceTests/**/*.cs", "**/EndToEndTests/**/*.cs")
        "Description" = "所有测试"
    }
    "Unit" = @{
        "Include" = @("**/UnitTests/**/*.cs")
        "Description" = "单元测试"
    }
    "Integration" = @{
        "Include" = @("**/IntegrationTests/**/*.cs")
        "Description" = "集成测试"
    }
    "Performance" = @{
        "Include" = @("**/PerformanceTests/**/*.cs")
        "Description" = "性能测试"
    }
    "E2E" = @{
        "Include" = @("**/EndToEndTests/**/*.cs")
        "Description" = "端到端测试"
    }
}

# 验证测试类别
if (-not $TestCategoryConfig.ContainsKey($TestCategory)) {
    Write-Log "无效的测试类别: $TestCategory" -Level "ERROR"
    Write-Log "有效的测试类别: $($TestCategoryConfig.Keys -join ', ')" -Level "ERROR"
    exit 1
}

# 获取测试配置
$testConfig = $TestCategoryConfig[$TestCategory]
Write-Log "运行测试类别: $testConfig.Description"

# 设置dotnet测试参数
$testArgs = @(
    "test",
    $TestProjectPath,
    "--configuration", $Configuration,
    "--logger", "console;verbosity=normal",
    "--logger", $"trx;LogFileName={Join-Path $ResultsPath 'test-results.trx'}",
    "--results-directory", $ResultsPath
)

# 添加测试过滤器
if ($Filter) {
    $testArgs += "--filter", $Filter
    Write-Log "测试过滤器: $Filter"
}

# 添加包含模式
foreach ($includePattern in $testConfig.Include) {
    $testArgs += "--test", $includePattern
}

# 添加代码覆盖率
if (-not $NoCoverage) {
    $testArgs += "--collect", "XPlat Code Coverage"
    Write-Log "启用代码覆盖率收集"
}

# 添加详细输出
if ($Verbose) {
    $testArgs += "--verbosity", "detailed"
    Write-Log "启用详细输出"
}

# 运行测试
Write-Log "开始运行测试..."
Write-Log "测试命令: dotnet $($testArgs -join ' ')"

$startTime = Get-Date

try {
    & dotnet $testArgs
    $exitCode = $LASTEXITCODE
    
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    Write-Log "测试完成，耗时: $($duration.TotalSeconds.ToString('F2')) 秒"
    
    if ($exitCode -eq 0) {
        Write-Log "所有测试通过!" -Level "SUCCESS"
    } else {
        Write-Log "有测试失败，退出代码: $exitCode" -Level "ERROR"
    }
    
    # 生成测试报告
    if (-not $NoReport) {
        Write-Log "生成测试报告..."
        & "$ScriptPath/generate-report.ps1" -ResultsPath $ResultsPath -Configuration $Configuration -TestCategory $TestCategory
    }
    
    # 显示测试结果摘要
    Write-Log "测试结果摘要:"
    Write-Log "  - 测试类别: $testConfig.Description"
    Write-Log "  - 配置: $Configuration"
    Write-Log "  - 耗时: $($duration.TotalSeconds.ToString('F2')) 秒"
    Write-Log "  - 退出代码: $exitCode"
    Write-Log "  - 结果目录: $ResultsPath"
    
    if (-not $NoCoverage) {
        $coverageFile = Join-Path $ResultsPath "coverage.cobertura.xml"
        if (Test-Path $coverageFile) {
            Write-Log "  - 覆盖率报告: $coverageFile"
        }
    }
    
    exit $exitCode
    
} catch {
    Write-Log "测试运行失败: $($_.Exception.Message)" -Level "ERROR"
    Write-Log "堆栈跟踪: $($_.Exception.StackTrace)" -Level "ERROR"
    exit 1
}