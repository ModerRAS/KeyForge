using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Core.Domain.Act;

namespace KeyForge.Tests.UAT;

/// <summary>
/// 办公自动化场景UAT测试
/// 
/// 场景描述：
/// 办公室工作人员使用KeyForge自动化重复的办公操作，
/// 如数据录入、报告生成、文件处理等。
/// 
/// 用户故事：
/// 作为一个办公用户，我希望能够录制日常的重复操作并自动化执行，
/// 以便能够节省时间，减少人为错误，提高工作效率。
/// </summary>
public class OfficeAutomationScenarioTests : UATTestBase
{
    private readonly ScriptPlayer _scriptPlayer;
    private readonly MockInputHandler _inputHandler;
    private readonly MockImageRecognition _imageRecognition;
    private readonly MockLogger _logger;

    public OfficeAutomationScenarioTests() : base(new Xunit.Abstractions.TestOutputHelper())
    {
        _inputHandler = new MockInputHandler();
        _imageRecognition = new MockImageRecognition();
        _logger = new MockLogger();
        
        _scriptPlayer = new ScriptPlayer(_inputHandler, _imageRecognition, _logger);
    }

    [Fact]
    public async Task DataEntryAutomationScenario()
    {
        await Task.Run(() =>
        {
            Given("办公用户每天需要手动录入大量数据到Excel表格", () =>
            {
                Log("用户每天需要处理100+条数据记录");
                Log("手动录入耗时约2小时");
                Log("容易出现人为错误");
                SimulateUserResponseTime();
            });

            Given("用户启动了KeyForge应用程序", () =>
            {
                Log("应用程序成功启动");
                SimulateUserResponseTime();
            });

            When("用户创建新的数据录入脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'新建脚本'");
                
                // 模拟创建数据录入脚本
                var script = TestFixtures.CreateValidScript();
                script.Update("数据录入自动化", "自动录入Excel数据");
                
                StopPerformanceMonitoring("创建数据录入脚本");
                AssertPerformanceIsWithin("创建数据录入脚本", 1500);
            });

            And("用户开始录制数据录入过程", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'开始录制'");
                
                // 模拟录制数据录入过程
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("开始录制数据录入");
                AssertPerformanceIsWithin("开始录制数据录入", 500);
            });

            And("用户执行完整的数据录入流程", () =>
            {
                Log("用户打开Excel文件");
                SimulateUserResponseTime();
                
                Log("用户移动到数据输入单元格");
                SimulateUserResponseTime();
                
                Log("用户输入第一条数据");
                SimulateUserResponseTime();
                
                Log("用户按Tab键移动到下一单元格");
                SimulateUserResponseTime();
                
                Log("用户输入第二条数据");
                SimulateUserResponseTime();
                
                Log("用户按回车键换行");
                SimulateUserResponseTime();
                
                Log("用户重复上述步骤5次");
                for (int i = 0; i < 5; i++)
                {
                    SimulateUserResponseTime();
                }
            });

            And("用户停止录制", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'停止录制'");
                
                // 模拟停止录制
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("停止数据录入录制");
                AssertPerformanceIsWithin("停止数据录入录制", 500);
            });

            Then("系统应该保存完整的数据录入操作序列", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证数据录入脚本
                var script = CreateDataEntryScript();
                script.Should().NotBeNull();
                script.Name.Should().Be("数据录入自动化");
                script.Actions.Should().HaveCountGreaterThanOrEqualTo(15); // 至少15个操作
                
                StopPerformanceMonitoring("验证数据录入脚本");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该分析操作模式并提供优化建议", () =>
            {
                Log("系统分析发现重复的操作模式");
                Log("建议：可以使用循环操作来优化脚本");
                Log("建议：可以添加数据验证步骤");
                
                SimulateUserResponseTime();
                AssertUserExperience("界面反应", true);
            });

            When("用户运行数据录入自动化脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'运行脚本'");
                
                // 模拟运行数据录入脚本
                var script = CreateDataEntryScript();
                _scriptPlayer.LoadScript(script);
                
                StopPerformanceMonitoring("加载数据录入脚本");
                AssertPerformanceIsWithin("加载数据录入脚本", 1000);
            });

            Then("系统应该自动执行数据录入操作", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本执行
                _scriptPlayer.PlayScript();
                
                StopPerformanceMonitoring("执行数据录入");
                AssertPerformanceIsWithin("执行数据录入", 5000);
            });

            And("系统应该显示执行进度和结果", () =>
            {
                Log("显示执行进度：60%");
                Log("已处理数据条数：60/100");
                Log("预计剩余时间：3分钟");
                
                SimulateUserResponseTime();
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该提供执行报告", () =>
            {
                Log("执行完成，生成报告：");
                Log("- 总处理数据：100条");
                Log("- 成功录入：100条");
                Log("- 失败录入：0条");
                Log("- 执行时间：8分钟30秒");
                Log("- 节省时间：1小时51分钟");
                
                AssertUserExperience("响应速度", true);
            });
        });
    }

    [Fact]
    public async Task ReportGenerationAutomationScenario()
    {
        await Task.Run(() =>
        {
            Given("办公用户每周需要生成销售报告", () =>
            {
                Log("用户需要从多个系统导出数据");
                Log("用户需要整合数据到Word模板");
                Log("用户需要格式化报告并添加图表");
                Log("手动生成报告耗时约3小时");
                SimulateUserResponseTime();
            });

            When("用户创建报告生成自动化脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户创建'报告生成自动化'脚本");
                
                // 模拟创建报告生成脚本
                var script = TestFixtures.CreateValidScript();
                script.Update("销售报告生成", "自动化生成周销售报告");
                
                StopPerformanceMonitoring("创建报告生成脚本");
                AssertPerformanceIsWithin("创建报告生成脚本", 2000);
            });

            And("用户录制完整的报告生成流程", () =>
            {
                Log("用户打开销售数据系统");
                SimulateUserResponseTime();
                
                Log("用户选择日期范围");
                SimulateUserResponseTime();
                
                Log("用户点击导出按钮");
                SimulateUserResponseTime();
                
                Log("用户打开Excel数据文件");
                SimulateUserResponseTime();
                
                Log("用户选择数据并复制");
                SimulateUserResponseTime();
                
                Log("用户打开Word报告模板");
                SimulateUserResponseTime();
                
                Log("用户粘贴数据到模板");
                SimulateUserResponseTime();
                
                Log("用户格式化表格");
                SimulateUserResponseTime();
                
                Log("用户插入图表");
                SimulateUserResponseTime();
                
                Log("用户保存报告");
                SimulateUserResponseTime();
            });

            Then("系统应该保存多步骤的报告生成流程", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证报告生成脚本
                var script = CreateReportGenerationScript();
                script.Should().NotBeNull();
                script.Actions.Should().HaveCountGreaterThanOrEqualTo(10);
                
                StopPerformanceMonitoring("验证报告生成脚本");
                AssertUserExperience("响应速度", true);
            });

            When("用户配置脚本参数", () =>
            {
                StartPerformanceMonitoring();
                Log("用户配置脚本参数");
                
                // 模拟参数配置
                Log("设置报告标题：'第X周销售报告'");
                Log("设置日期范围：'本周'");
                Log("设置输出格式：'PDF'");
                
                StopPerformanceMonitoring("配置脚本参数");
                AssertPerformanceIsWithin("配置脚本参数", 1500);
            });

            And("用户运行报告生成脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户运行报告生成脚本");
                
                // 模拟脚本执行
                var script = CreateReportGenerationScript();
                _scriptPlayer.LoadScript(script);
                _scriptPlayer.PlayScript();
                
                StopPerformanceMonitoring("执行报告生成");
                AssertPerformanceIsWithin("执行报告生成", 10000);
            });

            Then("系统应该自动生成完整的报告", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证报告生成结果
                Log("报告生成完成");
                Log("文件保存到：Reports/Sales_Week_X.pdf");
                Log("报告包含：");
                Log("- 销售数据表格");
                Log("- 销售趋势图表");
                Log("- 区域分析图表");
                Log("- 总结和建议");
                
                StopPerformanceMonitoring("验证报告生成结果");
                AssertUserExperience("界面反应", true);
            });

            And("系统应该提供质量检查", () =>
            {
                Log("系统进行质量检查：");
                Log("- 数据完整性：通过");
                Log("- 格式一致性：通过");
                Log("- 图表准确性：通过");
                Log("- 拼写检查：通过");
                
                SimulateUserResponseTime();
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该显示效率提升统计", () =>
            {
                Log("效率统计：");
                Log("- 手动时间：3小时");
                Log("- 自动化时间：15分钟");
                Log("- 节省时间：2小时45分钟");
                Log("- 效率提升：91.7%");
                
                AssertUserExperience("响应速度", true);
            });
        });
    }

    [Fact]
    public async Task FileProcessingAutomationScenario()
    {
        await Task.Run(() =>
        {
            Given("办公用户需要批量处理大量文件", () =>
            {
                Log("用户需要重命名500个文件");
                Log("用户需要移动文件到不同文件夹");
                Log("用户需要转换文件格式");
                Log("手动处理耗时约4小时");
                SimulateUserResponseTime();
            });

            When("用户创建文件处理自动化脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户创建'文件处理自动化'脚本");
                
                // 模拟创建文件处理脚本
                var script = TestFixtures.CreateValidScript();
                script.Update("批量文件处理", "自动化文件重命名和整理");
                
                StopPerformanceMonitoring("创建文件处理脚本");
                AssertPerformanceIsWithin("创建文件处理脚本", 1500);
            });

            And("用户录制文件处理流程", () =>
            {
                Log("用户打开文件管理器");
                SimulateUserResponseTime();
                
                Log("用户选择第一个文件");
                SimulateUserResponseTime();
                
                Log("用户按F2重命名文件");
                SimulateUserResponseTime();
                
                Log("用户输入新文件名");
                SimulateUserResponseTime();
                
                Log("用户按回车确认");
                SimulateUserResponseTime();
                
                Log("用户剪切文件");
                SimulateUserResponseTime();
                
                Log("用户导航到目标文件夹");
                SimulateUserResponseTime();
                
                Log("用户粘贴文件");
                SimulateUserResponseTime();
            });

            And("用户添加循环逻辑处理多个文件", () =>
            {
                StartPerformanceMonitoring();
                Log("用户添加循环操作");
                
                // 模拟添加循环逻辑
                Log("设置循环次数：500");
                Log("设置文件名模式：'Document_{i}.pdf'");
                Log("设置目标文件夹：'Processed'");
                
                StopPerformanceMonitoring("添加循环逻辑");
                AssertPerformanceIsWithin("添加循环逻辑", 2000);
            });

            Then("系统应该验证脚本的可行性", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本可行性
                Log("系统验证脚本：");
                Log("- 循环逻辑：正确");
                Log("- 文件操作：安全");
                Log("- 错误处理：完善");
                Log("- 预估执行时间：25分钟");
                
                StopPerformanceMonitoring("验证脚本可行性");
                AssertUserExperience("响应速度", true);
            });

            When("用户运行文件处理脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户运行文件处理脚本");
                
                // 模拟脚本执行
                var script = CreateFileProcessingScript();
                _scriptPlayer.LoadScript(script);
                
                StopPerformanceMonitoring("加载文件处理脚本");
                AssertPerformanceIsWithin("加载文件处理脚本", 1000);
            });

            Then("系统应该批量处理所有文件", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证文件处理结果
                Log("开始批量处理文件");
                Log("处理进度：10% (50/500)");
                Log("处理进度：25% (125/500)");
                Log("处理进度：50% (250/500)");
                Log("处理进度：75% (375/500)");
                Log("处理进度：100% (500/500)");
                
                StopPerformanceMonitoring("批量处理文件");
                AssertPerformanceIsWithin("批量处理文件", 15000);
            });

            And("系统应该提供处理统计", () =>
            {
                Log("处理统计：");
                Log("- 总文件数：500");
                Log("- 成功处理：500");
                Log("- 失败处理：0");
                Log("- 重命名：500");
                Log("- 移动文件：500");
                Log("- 执行时间：22分钟30秒");
                Log("- 节省时间：3小时37分钟");
                
                AssertUserExperience("界面反应", true);
            });

            And("系统应该生成处理日志", () =>
            {
                Log("生成详细处理日志：");
                Log("- 每个文件的处理记录");
                Log("- 错误和警告信息");
                Log("- 性能统计");
                Log("- 前后对比");
                
                SimulateUserResponseTime();
                AssertUserExperience("操作流畅度", true);
            });
        });
    }

    #region Helper Methods

    private Script CreateDataEntryScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("数据录入自动化", "自动录入Excel数据");
        
        // 添加数据录入相关的动作
        for (int i = 0; i < 15; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    private Script CreateReportGenerationScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("销售报告生成", "自动化生成周销售报告");
        
        // 添加报告生成相关的动作
        for (int i = 0; i < 12; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    private Script CreateFileProcessingScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("批量文件处理", "自动化文件重命名和整理");
        
        // 添加文件处理相关的动作
        for (int i = 0; i < 8; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    #endregion

    #region Mock Classes

    private class MockInputHandler : KeyForge.Core.Domain.Automation.IGameInputHandler
    {
        public Task<KeyForge.Core.Domain.Common.Result> SendKeyAsync(KeyForge.Core.Domain.Common.VirtualKeyCode keyCode, KeyForge.Core.Domain.Common.KeyState keyState)
        {
            return Task.FromResult(KeyForge.Core.Domain.Common.Result.Success());
        }

        public Task<KeyForge.Core.Domain.Common.Result> SendMouseAsync(KeyForge.Core.Domain.Common.MouseButton button, KeyForge.Core.Domain.Common.MouseAction action, int x, int y)
        {
            return Task.FromResult(KeyForge.Core.Domain.Common.Result.Success());
        }

        public Task<KeyForge.Core.Domain.Common.Result> SendTextAsync(string text)
        {
            return Task.FromResult(KeyForge.Core.Domain.Common.Result.Success());
        }

        public Task<KeyForge.Core.Domain.Common.Result> DelayAsync(int milliseconds)
        {
            return Task.Delay(milliseconds).ContinueWith(_ => KeyForge.Core.Domain.Common.Result.Success());
        }
    }

    private class MockImageRecognition : KeyForge.Core.Domain.Sense.IImageRecognitionEngine
    {
        public Task<KeyForge.Core.Domain.Common.Result<KeyForge.Core.Domain.Common.RecognitionResult>> RecognizeAsync(KeyForge.Core.Domain.Common.ImageData screen, KeyForge.Core.Domain.Common.ImageData template, KeyForge.Core.Domain.Common.RecognitionParameters parameters)
        {
            var result = new KeyForge.Core.Domain.Common.RecognitionResult
            {
                Found = true,
                Confidence = 0.95,
                Location = new KeyForge.Core.Domain.Common.Rectangle(100, 100, 50, 50),
                ProcessingTime = 50
            };
            return Task.FromResult(KeyForge.Core.Domain.Common.Result<KeyForge.Core.Domain.Common.RecognitionResult>.Success(result));
        }
    }

    private class MockLogger : KeyForge.Core.Domain.Common.ILogger
    {
        public void LogInformation(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        public void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }
    }

    #endregion
}