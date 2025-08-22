# KeyForge 测试报告生成脚本
# 原本实现：复杂的报告生成脚本
# 简化实现：基础的报告生成功能

param(
    [string]$ResultsPath = "TestResults",
    [string]$Configuration = "Debug",
    [string]$TestCategory = "All",
    [string]$OutputPath = "TestResults/Reports"
)

# 设置脚本路径
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition

# 创建输出目录
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# 日志函数
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
}

function Get-TestResults {
    param([string]$ResultsPath)
    
    $testResults = @{
        Total = 0
        Passed = 0
        Failed = 0
        Skipped = 0
        Duration = 0
        Tests = @()
    }
    
    # 尝试读取TRX文件
    $trxFile = Join-Path $ResultsPath "test-results.trx"
    if (Test-Path $trxFile) {
        try {
            [xml]$trxXml = Get-Content $trxFile
            
            $testResults.Total = [int]$trxXml.TestRun.ResultSummary.Total
            $testResults.Passed = [int]$trxXml.TestRun.ResultSummary.Passed
            $testResults.Failed = [int]$trxXml.TestRun.ResultSummary.Failed
            $testResults.Skipped = [int]$trxXml.TestRun.ResultSummary.NotExecuted
            $testResults.Duration = [double]$trxXml.TestRun.Times.Total
            
            # 解析单个测试结果
            foreach ($test in $trxXml.TestRun.TestDefinitions.UnitTest) {
                $testResult = @{
                    Name = $test.Name
                    ClassName = $test.TestMethod.ClassName
                    Outcome = ""
                    Duration = 0
                    ErrorMessage = ""
                    ErrorStackTrace = ""
                }
                
                # 查找测试结果
                $testExecution = $trxXml.TestRun.Results.UnitTestResult | 
                    Where-Object { $_.TestId -eq $test.Execution.Id }
                
                if ($testExecution) {
                    $testResult.Outcome = $testExecution.outcome
                    $testResult.Duration = [double]$testExecution.Duration
                    
                    if ($testExecution.Output -and $testExecution.Output.ErrorInfo) {
                        $testResult.ErrorMessage = $testExecution.Output.ErrorInfo.Message
                        $testResult.ErrorStackTrace = $testExecution.Output.ErrorInfo.StackTrace
                    }
                }
                
                $testResults.Tests += $testResult
            }
            
            Write-Log "从TRX文件读取测试结果成功"
        } catch {
            Write-Log "读取TRX文件失败: $($_.Exception.Message)" -Level "WARNING"
        }
    }
    
    return $testResults
}

function Get-CoverageData {
    param([string]$ResultsPath)
    
    $coverageData = @{
        LineCoverage = 0
        BranchCoverage = 0
        MethodCoverage = 0
        ClassCoverage = 0
        OverallCoverage = 0
        CoveredLines = 0
        TotalLines = 0
        Assemblies = @()
    }
    
    # 尝试读取覆盖率文件
    $coverageFile = Join-Path $ResultsPath "coverage.cobertura.xml"
    if (Test-Path $coverageFile) {
        try {
            [xml]$coverageXml = Get-Content $coverageFile
            
            $coverageData.LineCoverage = [double]$coverageXml.coverage.'line-rate'
            $coverageData.BranchCoverage = [double]$coverageXml.coverage.'branch-rate'
            $coverageData.MethodCoverage = [double]$coverageXml.coverage.'line-rate' # 简化
            $coverageData.ClassCoverage = [double]$coverageXml.coverage.'line-rate' # 简化
            $coverageData.OverallCoverage = [double]$coverageXml.coverage.'line-rate'
            
            $coverageData.CoveredLines = [int]$coverageXml.coverage.'lines-valid'
            $coverageData.TotalLines = [int]$coverageXml.coverage.'lines-covered'
            
            # 解析程序集覆盖率
            foreach ($package in $coverageXml.coverage.packages.package) {
                $assemblyInfo = @{
                    Name = $package.name
                    LineCoverage = [double]$package.'line-rate'
                    BranchCoverage = [double]$package.'branch-rate'
                    CoveredLines = [int]$package.'lines-valid'
                    TotalLines = [int]$package.'lines-covered'
                }
                $coverageData.Assemblies += $assemblyInfo
            }
            
            Write-Log "从覆盖率文件读取数据成功"
        } catch {
            Write-Log "读取覆盖率文件失败: $($_.Exception.Message)" -Level "WARNING"
        }
    }
    
    return $coverageData
}

function Get-PerformanceData {
    param([string]$ResultsPath)
    
    $performanceData = @{
        AverageExecutionTime = 0
        MaxExecutionTime = 0
        MinExecutionTime = 0
        TotalTests = 0
        MemoryUsage = 0
        CpuUsage = 0
        Benchmarks = @()
    }
    
    # 尝试读取性能数据文件
    $perfFile = Join-Path $ResultsPath "performance.json"
    if (Test-Path $perfFile) {
        try {
            $perfJson = Get-Content $perfFile | ConvertFrom-Json
            
            $performanceData.AverageExecutionTime = $perfJson.AverageExecutionTime
            $performanceData.MaxExecutionTime = $perfJson.MaxExecutionTime
            $performanceData.MinExecutionTime = $perfJson.MinExecutionTime
            $performanceData.TotalTests = $perfJson.TotalTests
            $performanceData.MemoryUsage = $perfJson.MemoryUsage
            $performanceData.CpuUsage = $perfJson.CpuUsage
            
            if ($perfJson.Benchmarks) {
                $performanceData.Benchmarks = $perfJson.Benchmarks
            }
            
            Write-Log "从性能数据文件读取数据成功"
        } catch {
            Write-Log "读取性能数据文件失败: $($_.Exception.Message)" -Level "WARNING"
        }
    }
    
    return $performanceData
}

function Generate-HtmlReport {
    param(
        [hashtable]$TestResults,
        [hashtable]$CoverageData,
        [hashtable]$PerformanceData,
        [string]$OutputPath,
        [string]$Configuration,
        [string]$TestCategory
    )
    
    $reportFile = Join-Path $OutputPath "test-report.html"
    
    # 计算成功率
    $successRate = if ($TestResults.Total -gt 0) { 
        ($TestResults.Passed / $TestResults.Total) * 100 
    } else { 
        0 
    }
    
    # 确定状态颜色
    $statusColor = if ($TestResults.Failed -eq 0) { "#28a745" } elseif ($TestResults.Failed -le 5) { "#ffc107" } else { "#dc3545" }
    $coverageColor = if ($CoverageData.OverallCoverage -ge 80) { "#28a745" } elseif ($CoverageData.OverallCoverage -ge 60) { "#ffc107" } else { "#dc3545" }
    
    # 生成HTML报告
    $html = @"
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>KeyForge 测试报告</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background-color: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 30px;
        }
        .header {
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #e9ecef;
        }
        .header h1 {
            color: #343a40;
            margin: 0;
            font-size: 2.5rem;
        }
        .header p {
            color: #6c757d;
            margin: 10px 0 0 0;
            font-size: 1.1rem;
        }
        .summary-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        .summary-card {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            border: 1px solid #e9ecef;
        }
        .summary-card h3 {
            margin: 0 0 10px 0;
            color: #495057;
            font-size: 0.9rem;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        .summary-card .value {
            font-size: 2rem;
            font-weight: bold;
            margin: 0;
        }
        .summary-card.success .value { color: #28a745; }
        .summary-card.warning .value { color: #ffc107; }
        .summary-card.danger .value { color: #dc3545; }
        .summary-card.info .value { color: #17a2b8; }
        .section {
            margin-bottom: 40px;
        }
        .section h2 {
            color: #343a40;
            border-bottom: 2px solid #e9ecef;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }
        .test-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        .test-table th,
        .test-table td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #e9ecef;
        }
        .test-table th {
            background-color: #f8f9fa;
            font-weight: 600;
            color: #495057;
        }
        .test-table tr:hover {
            background-color: #f8f9fa;
        }
        .status-badge {
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 0.8rem;
            font-weight: 500;
        }
        .status-badge.passed {
            background-color: #d4edda;
            color: #155724;
        }
        .status-badge.failed {
            background-color: #f8d7da;
            color: #721c24;
        }
        .status-badge.skipped {
            background-color: #fff3cd;
            color: #856404;
        }
        .progress-bar {
            width: 100%;
            height: 20px;
            background-color: #e9ecef;
            border-radius: 10px;
            overflow: hidden;
            margin-bottom: 10px;
        }
        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #28a745, #20c997);
            transition: width 0.3s ease;
        }
        .coverage-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
        }
        .coverage-card {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            border: 1px solid #e9ecef;
        }
        .coverage-card h4 {
            margin: 0 0 15px 0;
            color: #495057;
        }
        .coverage-bar {
            width: 100%;
            height: 8px;
            background-color: #e9ecef;
            border-radius: 4px;
            overflow: hidden;
            margin-bottom: 10px;
        }
        .coverage-fill {
            height: 100%;
            background: linear-gradient(90deg, #28a745, #20c997);
        }
        .error-details {
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            border-radius: 4px;
            padding: 15px;
            margin-top: 10px;
        }
        .error-details h4 {
            color: #721c24;
            margin: 0 0 10px 0;
        }
        .error-details pre {
            background-color: #fff;
            padding: 10px;
            border-radius: 4px;
            overflow-x: auto;
            font-size: 0.9rem;
            color: #495057;
        }
        .footer {
            text-align: center;
            margin-top: 40px;
            padding-top: 20px;
            border-top: 2px solid #e9ecef;
            color: #6c757d;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>KeyForge 测试报告</h1>
            <p>生成时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
            <p>配置: $Configuration | 测试类别: $TestCategory</p>
        </div>

        <div class="summary-grid">
            <div class="summary-card success">
                <h3>总测试数</h3>
                <div class="value">$($TestResults.Total)</div>
            </div>
            <div class="summary-card success">
                <h3>通过</h3>
                <div class="value">$($TestResults.Passed)</div>
            </div>
            <div class="summary-card $($TestResults.Failed -eq 0 ? 'success' : 'danger')">
                <h3>失败</h3>
                <div class="value">$($TestResults.Failed)</div>
            </div>
            <div class="summary-card warning">
                <h3>跳过</h3>
                <div class="value">$($TestResults.Skipped)</div>
            </div>
            <div class="summary-card info">
                <h3>耗时</h3>
                <div class="value">$($TestResults.Duration.ToString('F2'))s</div>
            </div>
            <div class="summary-card" style="border-color: $coverageColor;">
                <h3>覆盖率</h3>
                <div class="value" style="color: $coverageColor;">$($CoverageData.OverallCoverage.ToString('F1'))%</div>
            </div>
        </div>

        <div class="progress-bar">
            <div class="progress-fill" style="width: $successRate%;"></div>
        </div>

        <div class="section">
            <h2>测试结果概览</h2>
            <table class="test-table">
                <thead>
                    <tr>
                        <th>状态</th>
                        <th>数量</th>
                        <th>百分比</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><span class="status-badge passed">通过</span></td>
                        <td>$($TestResults.Passed)</td>
                        <td>$($TestResults.Passed / $TestResults.Total * 100).ToString('F1')%</td>
                    </tr>
                    <tr>
                        <td><span class="status-badge failed">失败</span></td>
                        <td>$($TestResults.Failed)</td>
                        <td>$($TestResults.Failed / $TestResults.Total * 100).ToString('F1')%</td>
                    </tr>
                    <tr>
                        <td><span class="status-badge skipped">跳过</span></td>
                        <td>$($TestResults.Skipped)</td>
                        <td>$($TestResults.Skipped / $TestResults.Total * 100).ToString('F1')%</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div class="section">
            <h2>代码覆盖率</h2>
            <div class="coverage-grid">
                <div class="coverage-card">
                    <h4>整体覆盖率</h4>
                    <div class="coverage-bar">
                        <div class="coverage-fill" style="width: $($CoverageData.OverallCoverage * 100)%;"></div>
                    </div>
                    <p>$($CoverageData.OverallCoverage.ToString('F1'))%</p>
                </div>
                <div class="coverage-card">
                    <h4>行覆盖率</h4>
                    <div class="coverage-bar">
                        <div class="coverage-fill" style="width: $($CoverageData.LineCoverage * 100)%;"></div>
                    </div>
                    <p>$($CoverageData.LineCoverage.ToString('F1'))%</p>
                </div>
                <div class="coverage-card">
                    <h4>分支覆盖率</h4>
                    <div class="coverage-bar">
                        <div class="coverage-fill" style="width: $($CoverageData.BranchCoverage * 100)%;"></div>
                    </div>
                    <p>$($CoverageData.BranchCoverage.ToString('F1'))%</p>
                </div>
            </div>
        </div>

        <div class="section">
            <h2>失败的测试</h2>
            $(if ($TestResults.Failed -gt 0) {
                $failedTests = $TestResults.Tests | Where-Object { $_.Outcome -eq 'Failed' }
                foreach ($test in $failedTests) {
                    @"
                    <div class="error-details">
                        <h4>$($test.Name)</h4>
                        <p><strong>类名:</strong> $($test.ClassName)</p>
                        <p><strong>耗时:</strong> $($test.Duration.ToString('F2'))s</p>
                        <pre>$($test.ErrorMessage)</pre>
                        $(if ($test.ErrorStackTrace) {
                            "<pre>$($test.ErrorStackTrace)</pre>"
                        })
                    </div>
                    "@
                }
            } else {
                "<p style='color: #28a745; font-weight: bold;'>没有失败的测试！</p>"
            })
        </div>

        <div class="section">
            <h2>性能指标</h2>
            <table class="test-table">
                <thead>
                    <tr>
                        <th>指标</th>
                        <th>值</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>平均执行时间</td>
                        <td>$($PerformanceData.AverageExecutionTime.ToString('F2'))ms</td>
                    </tr>
                    <tr>
                        <td>最大执行时间</td>
                        <td>$($PerformanceData.MaxExecutionTime.ToString('F2'))ms</td>
                    </tr>
                    <tr>
                        <td>最小执行时间</td>
                        <td>$($PerformanceData.MinExecutionTime.ToString('F2'))ms</td>
                    </tr>
                    <tr>
                        <td>内存使用</td>
                        <td>$($PerformanceData.MemoryUsage.ToString('F2'))MB</td>
                    </tr>
                    <tr>
                        <td>CPU使用率</td>
                        <td>$($PerformanceData.CpuUsage.ToString('F1'))%</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div class="footer">
            <p>报告由 KeyForge 测试系统生成 | $(Get-Date -Format 'yyyy')</p>
        </div>
    </div>
</body>
</html>
"@

    # 保存HTML报告
    $html | Out-File -FilePath $reportFile -Encoding UTF8
    
    Write-Log "HTML报告生成成功: $reportFile"
}

function Generate-JsonReport {
    param(
        [hashtable]$TestResults,
        [hashtable]$CoverageData,
        [hashtable]$PerformanceData,
        [string]$OutputPath,
        [string]$Configuration,
        [string]$TestCategory
    )
    
    $reportFile = Join-Path $OutputPath "test-report.json"
    
    $report = @{
        GeneratedAt = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
        Configuration = $Configuration
        TestCategory = $TestCategory
        TestResults = @{
            Total = $TestResults.Total
            Passed = $TestResults.Passed
            Failed = $TestResults.Failed
            Skipped = $TestResults.Skipped
            Duration = $TestResults.Duration
            SuccessRate = if ($TestResults.Total -gt 0) { ($TestResults.Passed / $TestResults.Total) * 100 } else { 0 }
        }
        Coverage = @{
            Overall = $CoverageData.OverallCoverage
            Line = $CoverageData.LineCoverage
            Branch = $CoverageData.BranchCoverage
            Method = $CoverageData.MethodCoverage
            Class = $CoverageData.ClassCoverage
            CoveredLines = $CoverageData.CoveredLines
            TotalLines = $CoverageData.TotalLines
        }
        Performance = @{
            AverageExecutionTime = $PerformanceData.AverageExecutionTime
            MaxExecutionTime = $PerformanceData.MaxExecutionTime
            MinExecutionTime = $PerformanceData.MinExecutionTime
            TotalTests = $PerformanceData.TotalTests
            MemoryUsage = $PerformanceData.MemoryUsage
            CpuUsage = $PerformanceData.CpuUsage
        }
        FailedTests = @($TestResults.Tests | Where-Object { $_.Outcome -eq 'Failed' })
    }
    
    # 保存JSON报告
    $report | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportFile -Encoding UTF8
    
    Write-Log "JSON报告生成成功: $reportFile"
}

# 主执行逻辑
Write-Log "开始生成测试报告..."

# 获取测试数据
Write-Log "读取测试结果..."
$testResults = Get-TestResults -ResultsPath $ResultsPath

Write-Log "读取覆盖率数据..."
$coverageData = Get-CoverageData -ResultsPath $ResultsPath

Write-Log "读取性能数据..."
$performanceData = Get-PerformanceData -ResultsPath $ResultsPath

# 生成报告
Write-Log "生成HTML报告..."
Generate-HtmlReport -TestResults $testResults -CoverageData $coverageData -PerformanceData $performanceData -OutputPath $OutputPath -Configuration $Configuration -TestCategory $TestCategory

Write-Log "生成JSON报告..."
Generate-JsonReport -TestResults $testResults -CoverageData $coverageData -PerformanceData $performanceData -OutputPath $OutputPath -Configuration $Configuration -TestCategory $TestCategory

# 显示报告摘要
Write-Log "报告生成完成:"
Write-Log "  - HTML报告: $(Join-Path $OutputPath 'test-report.html')"
Write-Log "  - JSON报告: $(Join-Path $OutputPath 'test-report.json')"
Write-Log "  - 测试结果: $($testResults.Total) 个测试, $($testResults.Passed) 个通过, $($testResults.Failed) 个失败"
Write-Log "  - 代码覆盖率: $($coverageData.OverallCoverage.ToString('F1'))%"
Write-Log "  - 平均执行时间: $($performanceData.AverageExecutionTime.ToString('F2'))ms"

# 返回成功
exit 0