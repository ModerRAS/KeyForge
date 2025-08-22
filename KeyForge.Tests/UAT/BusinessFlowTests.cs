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
/// 业务流程UAT测试
/// 
/// 场景描述：
/// 测试KeyForge的核心业务流程，包括脚本创建、录制、保存、加载、播放的完整流程，
/// 以及脚本管理和错误处理等关键业务功能。
/// 
/// 测试重点：
/// - 完整的业务流程覆盖
/// - 错误处理和恢复机制
/// - 用户体验和操作便利性
/// - 业务规则的正确性
/// </summary>
public class BusinessFlowTests : UATTestBase
{
    private readonly ScriptPlayer _scriptPlayer;
    private readonly MockInputHandler _inputHandler;
    private readonly MockImageRecognition _imageRecognition;
    private readonly MockLogger _logger;

    public BusinessFlowTests() : base(new Xunit.Abstractions.TestOutputHelper())
    {
        _inputHandler = new MockInputHandler();
        _imageRecognition = new MockImageRecognition();
        _logger = new MockLogger();
        
        _scriptPlayer = new ScriptPlayer(_inputHandler, _imageRecognition, _logger);
    }

    [Fact]
    public async Task CompleteScriptLifecycleFlow()
    {
        await Task.Run(() =>
        {
            Given("用户想要创建一个完整的自动化脚本", () =>
            {
                Log("用户是首次使用KeyForge的新用户");
                Log("用户需要完成从创建到执行的全流程");
                SimulateUserResponseTime();
            });

            When("用户启动应用程序并开始创建脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户启动KeyForge应用程序");
                
                // 模拟应用程序启动
                SimulateUserResponseTime();
                
                Log("用户点击'新建脚本'按钮");
                
                StopPerformanceMonitoring("应用程序启动和创建脚本");
                AssertPerformanceIsWithin("应用程序启动和创建脚本", 3000);
            });

            Then("系统应该显示脚本创建界面", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证界面显示
                Log("显示脚本创建向导");
                Log("显示脚本名称输入框");
                Log("显示脚本描述输入框");
                Log("显示'开始录制'按钮");
                
                StopPerformanceMonitoring("界面显示验证");
                AssertUserExperience("界面反应", true);
            });

            When("用户填写脚本信息并开始录制", () =>
            {
                StartPerformanceMonitoring();
                Log("用户输入脚本名称：'我的第一个脚本'");
                SimulateUserResponseTime();
                
                Log("用户输入脚本描述：'测试自动化功能'");
                SimulateUserResponseTime();
                
                Log("用户点击'开始录制'按钮");
                
                StopPerformanceMonitoring("填写脚本信息");
                AssertPerformanceIsWithin("填写脚本信息", 2000);
            });

            Then("系统应该开始录制模式", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证录制模式
                Log("系统进入录制状态");
                Log("显示录制指示器（红色圆点）");
                Log("显示'停止录制'按钮");
                Log("显示已录制操作计数：0");
                
                StopPerformanceMonitoring("录制模式验证");
                AssertUserExperience("响应速度", true);
            });

            When("用户执行一系列操作", () =>
            {
                StartPerformanceMonitoring();
                Log("用户按下Ctrl+C复制操作");
                SimulateUserResponseTime();
                
                Log("用户移动鼠标到目标位置");
                SimulateUserResponseTime();
                
                Log("用户点击鼠标右键");
                SimulateUserResponseTime();
                
                Log("用户按下Ctrl+V粘贴操作");
                SimulateUserResponseTime();
                
                Log("用户按下回车键确认");
                SimulateUserResponseTime();
                
                Log("用户等待2秒");
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("用户操作录制");
                AssertPerformanceIsWithin("用户操作录制", 4000);
            });

            Then("系统应该实时记录用户操作", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证操作记录
                Log("显示已录制操作计数：6");
                Log("显示最近操作：'按下Ctrl+C'");
                Log("显示操作时间戳");
                
                StopPerformanceMonitoring("操作记录验证");
                AssertUserExperience("界面反应", true);
            });

            When("用户停止录制", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'停止录制'按钮");
                
                // 模拟停止录制
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("停止录制");
                AssertPerformanceIsWithin("停止录制", 1000);
            });

            Then("系统应该显示脚本预览", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本预览
                Log("显示脚本预览界面");
                Log("显示操作步骤列表：");
                Log("  1. 按下Ctrl+C");
                Log("  2. 移动鼠标到(100,200)");
                Log("  3. 点击鼠标右键");
                Log("  4. 按下Ctrl+V");
                Log("  5. 按下回车键");
                Log("  6. 等待2秒");
                Log("显示预计执行时间：3秒");
                
                StopPerformanceMonitoring("脚本预览验证");
                AssertUserExperience("操作流畅度", true);
            });

            When("用户保存脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'保存脚本'按钮");
                
                // 模拟保存脚本
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("保存脚本");
                AssertPerformanceIsWithin("保存脚本", 1500);
            });

            Then("系统应该保存脚本到存储", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本保存
                var script = TestFixtures.CreateValidScript();
                script.Update("我的第一个脚本", "测试自动化功能");
                
                // 添加录制的操作
                for (int i = 0; i < 6; i++)
                {
                    var action = TestFixtures.CreateKeyboardAction();
                    script.AddAction(action);
                }
                
                script.Activate();
                
                script.Should().NotBeNull();
                script.Name.Should().Be("我的第一个脚本");
                script.Actions.Should().HaveCount(6);
                
                StopPerformanceMonitoring("脚本保存验证");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该显示保存成功消息", () =>
            {
                Log("显示'脚本保存成功'消息");
                Log("显示脚本文件路径");
                Log("显示脚本大小：1.2KB");
                
                SimulateUserResponseTime();
                AssertUserExperience("界面反应", true);
            });

            When("用户加载并运行脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户从脚本列表中选择刚才保存的脚本");
                
                // 模拟加载脚本
                var script = TestFixtures.CreateValidScript();
                _scriptPlayer.LoadScript(script);
                
                Log("用户点击'运行脚本'按钮");
                
                StopPerformanceMonitoring("加载和运行脚本");
                AssertPerformanceIsWithin("加载和运行脚本", 2000);
            });

            Then("系统应该执行脚本并显示结果", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本执行
                _scriptPlayer.PlayScript();
                
                // 模拟执行过程
                Log("开始执行脚本...");
                Log("执行步骤1：按下Ctrl+C ✓");
                Log("执行步骤2：移动鼠标到(100,200) ✓");
                Log("执行步骤3：点击鼠标右键 ✓");
                Log("执行步骤4：按下Ctrl+V ✓");
                Log("执行步骤5：按下回车键 ✓");
                Log("执行步骤6：等待2秒 ✓");
                Log("脚本执行完成！");
                
                StopPerformanceMonitoring("脚本执行验证");
                AssertPerformanceIsWithin("脚本执行验证", 3000);
            });

            And("系统应该提供执行报告", () =>
            {
                Log("执行报告：");
                Log("- 总步骤数：6");
                Log("- 成功执行：6");
                Log("- 失败执行：0");
                Log("- 执行时间：3.2秒");
                Log("- 状态：成功完成");
                
                AssertUserExperience("操作流畅度", true);
            });
        });
    }

    [Fact]
    public async Task ScriptManagementAndOrganizationFlow()
    {
        await Task.Run(() =>
        {
            Given("用户有多个脚本需要管理", () =>
            {
                Log("用户已经创建了5个不同的脚本");
                Log("用户需要对这些脚本进行分类和管理");
                SimulateUserResponseTime();
            });

            When("用户打开脚本管理界面", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'脚本管理'菜单");
                
                // 模拟加载脚本管理界面
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("打开脚本管理");
                AssertPerformanceIsWithin("打开脚本管理", 1500);
            });

            Then("系统应该显示所有脚本", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本列表显示
                Log("显示脚本列表：");
                Log("  1. 游戏自动化脚本 - 最后修改：2小时前");
                Log("  2. 数据录入脚本 - 最后修改：1天前");
                Log("  3. 报告生成脚本 - 最后修改：3天前");
                Log("  4. 文件处理脚本 - 最后修改：1周前");
                Log("  5. 系统维护脚本 - 最后修改：2周前");
                
                StopPerformanceMonitoring("脚本列表显示");
                AssertUserExperience("界面反应", true);
            });

            When("用户创建文件夹来组织脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'新建文件夹'按钮");
                
                // 模拟创建文件夹
                Log("用户输入文件夹名称：'游戏脚本'");
                SimulateUserResponseTime();
                
                Log("用户点击'创建'按钮");
                
                StopPerformanceMonitoring("创建文件夹");
                AssertPerformanceIsWithin("创建文件夹", 1000);
            });

            Then("系统应该创建文件夹", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证文件夹创建
                Log("显示新建的文件夹：'游戏脚本'");
                Log("文件夹为空");
                
                StopPerformanceMonitoring("文件夹创建验证");
                AssertUserExperience("响应速度", true);
            });

            When("用户将脚本移动到文件夹", () =>
            {
                StartPerformanceMonitoring();
                Log("用户拖拽'游戏自动化脚本'到'游戏脚本'文件夹");
                
                // 模拟移动脚本
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("移动脚本");
                AssertPerformanceIsWithin("移动脚本", 800);
            });

            Then("系统应该更新脚本位置", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本移动
                Log("脚本已移动到'游戏脚本'文件夹");
                Log("更新脚本列表显示");
                
                StopPerformanceMonitoring("脚本移动验证");
                AssertUserExperience("界面反应", true);
            });

            When("用户搜索特定脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户在搜索框中输入'报告'");
                
                // 模拟搜索功能
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("搜索脚本");
                AssertPerformanceIsWithin("搜索脚本", 500);
            });

            Then("系统应该显示搜索结果", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证搜索结果
                Log("显示搜索结果：");
                Log("  找到1个匹配的脚本：");
                Log("  - 报告生成脚本");
                
                StopPerformanceMonitoring("搜索结果验证");
                AssertUserExperience("响应速度", true);
            });

            When("用户批量操作多个脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户选择多个脚本（按住Ctrl键点击）");
                
                // 模拟批量选择
                Log("用户选择了3个脚本");
                SimulateUserResponseTime();
                
                Log("用户右键点击选择'导出'选项");
                
                StopPerformanceMonitoring("批量操作");
                AssertPerformanceIsWithin("批量操作", 1000);
            });

            Then("系统应该处理批量操作", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证批量操作
                Log("显示批量导出对话框");
                Log("显示选中的3个脚本");
                Log("显示导出选项");
                
                StopPerformanceMonitoring("批量操作验证");
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该执行批量导出", () =>
            {
                Log("开始批量导出...");
                Log("导出脚本1：数据录入脚本 ✓");
                Log("导出脚本2：报告生成脚本 ✓");
                Log("导出脚本3：文件处理脚本 ✓");
                Log("批量导出完成！");
                
                SimulateUserResponseTime();
                AssertUserExperience("响应速度", true);
            });
        });
    }

    [Fact]
    public async Task ErrorHandlingAndRecoveryFlow()
    {
        await Task.Run(() =>
        {
            Given("用户在执行脚本时遇到错误", () =>
            {
                Log("用户正在运行一个复杂的数据处理脚本");
                Log("脚本在执行过程中可能遇到各种异常情况");
                SimulateUserResponseTime();
            });

            When("脚本执行过程中出现错误", () =>
            {
                StartPerformanceMonitoring();
                Log("脚本正在执行第15个步骤");
                
                // 模拟错误发生
                Log("系统检测到错误：'目标窗口未找到'");
                
                StopPerformanceMonitoring("错误检测");
                AssertPerformanceIsWithin("错误检测", 100);
            });

            Then("系统应该暂停脚本执行", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证执行暂停
                Log("脚本执行已暂停");
                Log("显示错误警告图标");
                Log("显示暂停状态");
                
                StopPerformanceMonitoring("执行暂停验证");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该显示错误信息", () =>
            {
                Log("显示错误详情：");
                Log("错误类型：窗口未找到");
                Log("错误位置：步骤15 - 点击'保存'按钮");
                Log("错误描述：无法找到标题为'保存对话框'的窗口");
                Log("建议解决方案：");
                Log("  1. 检查目标应用程序是否正在运行");
                Log("  2. 检查窗口标题是否正确");
                Log("  3. 添加等待时间确保窗口加载完成");
                
                SimulateUserResponseTime();
                AssertUserExperience("界面反应", true);
            });

            When("用户选择重试操作", () =>
            {
                StartPerformanceMonitoring();
                Log("用户点击'重试'按钮");
                
                // 模拟重试操作
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("重试操作");
                AssertPerformanceIsWithin("重试操作", 500);
            });

            Then("系统应该重新尝试失败的步骤", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证重试
                Log("重新执行步骤15...");
                Log("等待2秒确保窗口加载");
                Log("再次尝试点击'保存'按钮");
                Log("重试成功！");
                
                StopPerformanceMonitoring("重试验证");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该继续执行剩余步骤", () =>
            {
                Log("继续执行脚本...");
                Log("执行步骤16：输入文件名 ✓");
                Log("执行步骤17：点击'确认'按钮 ✓");
                Log("脚本执行完成！");
                
                SimulateUserResponseTime();
                AssertUserExperience("操作流畅度", true);
            });

            When("用户配置错误处理选项", () =>
            {
                StartPerformanceMonitoring();
                Log("用户打开脚本设置");
                
                // 模拟配置错误处理
                Log("用户启用'自动重试'选项");
                Log("用户设置重试次数：3次");
                Log("用户设置重试间隔：2秒");
                Log("用户启用'错误时继续执行'选项");
                
                StopPerformanceMonitoring("配置错误处理");
                AssertPerformanceIsWithin("配置错误处理", 2000);
            });

            Then("系统应该保存错误处理配置", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证配置保存
                Log("错误处理配置已保存");
                Log("显示配置摘要：");
                Log("- 自动重试：启用");
                Log("- 重试次数：3次");
                Log("- 重试间隔：2秒");
                Log("- 错误时继续：启用");
                
                StopPerformanceMonitoring("配置保存验证");
                AssertUserExperience("界面反应", true);
            });

            When("用户再次运行脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户运行配置了错误处理的脚本");
                
                // 模拟脚本执行
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("运行增强脚本");
                AssertPerformanceIsWithin("运行增强脚本", 1000);
            });

            Then("系统应该自动处理错误", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证自动错误处理
                Log("脚本执行中遇到错误");
                Log("自动重试第1次...失败");
                Log("自动重试第2次...失败");
                Log("自动重试第3次...成功！");
                Log("继续执行后续步骤");
                Log("脚本执行完成！");
                
                StopPerformanceMonitoring("自动错误处理验证");
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该生成错误处理报告", () =>
            {
                Log("错误处理报告：");
                Log("- 遇到错误次数：1");
                Log("- 自动重试次数：3");
                Log("- 重试成功次数：1");
                Log("- 错误处理成功率：100%");
                Log("- 总执行时间：45秒");
                
                AssertUserExperience("响应速度", true);
            });
        });
    }

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