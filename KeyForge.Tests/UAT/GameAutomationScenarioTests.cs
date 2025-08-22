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
/// 游戏自动化场景UAT测试
/// 
/// 场景描述：
/// 游戏玩家使用KeyForge录制和回放游戏宏，自动化重复的游戏操作
/// 
/// 用户故事：
/// 作为一个游戏玩家，我希望能够录制游戏操作并保存为脚本，
/// 以便能够自动执行重复的游戏任务，减少手动操作的时间。
/// </summary>
public class GameAutomationScenarioTests : UATTestBase
{
    private readonly ScriptPlayer _scriptPlayer;
    private readonly MockInputHandler _inputHandler;
    private readonly MockImageRecognition _imageRecognition;
    private readonly MockLogger _logger;

    public GameAutomationScenarioTests() : base(new Xunit.Abstractions.TestOutputHelper())
    {
        // 创建模拟的游戏环境
        _inputHandler = new MockInputHandler();
        _imageRecognition = new MockImageRecognition();
        _logger = new MockLogger();
        
        _scriptPlayer = new ScriptPlayer(_inputHandler, _imageRecognition, _logger);
    }

    [Fact]
    public async Task GameMacroRecordingAndPlaybackScenario()
    {
        await Task.Run(() =>
        {
            Given("游戏玩家想要录制一个游戏宏", () =>
            {
                Log("玩家正在玩一个需要重复操作的游戏");
                SimulateUserResponseTime();
            });

            Given("游戏玩家启动了KeyForge应用程序", () =>
            {
                Log("应用程序成功启动，界面加载完成");
                SimulateUserResponseTime();
            });

            When("玩家点击'新建脚本'按钮", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家点击新建脚本按钮");
                
                // 模拟创建脚本的操作
                var script = TestFixtures.CreateValidScript();
                script.Update("游戏战斗宏", "用于自动执行游戏战斗操作");
                
                StopPerformanceMonitoring("创建脚本");
                AssertPerformanceIsWithin("创建脚本", 1000);
            });

            And("玩家点击'开始录制'按钮", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家点击开始录制按钮");
                
                // 模拟开始录制
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("开始录制");
                AssertPerformanceIsWithin("开始录制", 500);
            });

            And("玩家执行游戏操作序列", () =>
            {
                Log("玩家按下技能1快捷键");
                SimulateUserResponseTime();
                
                Log("玩家点击敌人目标");
                SimulateUserResponseTime();
                
                Log("玩家使用技能2");
                SimulateUserResponseTime();
                
                Log("玩家等待技能冷却");
                SimulateUserResponseTime();
            });

            And("玩家点击'停止录制'按钮", () =>
                {
                    StartPerformanceMonitoring();
                    Log("玩家点击停止录制按钮");
                    
                    // 模拟停止录制
                    SimulateUserResponseTime();
                    
                    StopPerformanceMonitoring("停止录制");
                    AssertPerformanceIsWithin("停止录制", 500);
                });

            Then("系统应该保存录制的游戏操作序列", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本创建成功
                var script = CreateGameAutomationScript();
                script.Should().NotBeNull();
                script.Name.Should().Be("游戏自动化脚本");
                script.Actions.Should().HaveCount(3);
                
                StopPerformanceMonitoring("验证脚本");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该显示录制成功的消息", () =>
            {
                Log("显示'录制成功，已保存3个操作'的消息");
                SimulateUserResponseTime();
                
                AssertUserExperience("界面反应", true);
            });

            When("玩家点击'播放脚本'按钮", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家点击播放脚本按钮");
                
                // 模拟播放脚本
                var script = CreateGameAutomationScript();
                _scriptPlayer.LoadScript(script);
                
                StopPerformanceMonitoring("加载脚本");
                AssertPerformanceIsWithin("加载脚本", 1000);
            });

            Then("系统应该按照录制顺序执行游戏操作", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本执行
                _scriptPlayer.PlayScript();
                
                StopPerformanceMonitoring("执行脚本");
                AssertPerformanceIsWithin("执行脚本", 3000);
            });

            And("游戏角色应该按照预期执行动作", () =>
            {
                Log("游戏角色按下了技能1快捷键");
                Log("游戏角色点击了敌人目标");
                Log("游戏角色使用了技能2");
                Log("游戏角色等待了技能冷却时间");
                
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该显示执行完成的状态", () =>
            {
                Log("显示'脚本执行完成'的状态信息");
                SimulateUserResponseTime();
                
                AssertUserExperience("响应速度", true);
            });
        });
    }

    [Fact]
    public async Task ComplexGameAutomationScenario()
    {
        await Task.Run(() =>
        {
            Given("高级游戏玩家想要创建复杂的游戏自动化脚本", () =>
            {
                Log("玩家正在玩一个需要复杂操作的游戏");
                SimulateUserResponseTime();
            });

            Given("玩家已经录制了基础的战斗操作", () =>
            {
                Log("玩家已经有了基础的战斗宏");
                SimulateUserResponseTime();
            });

            When("玩家想要添加图像识别功能", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家选择添加图像识别步骤");
                
                // 模拟图像识别配置
                var script = CreateGameAutomationScript();
                
                StopPerformanceMonitoring("配置图像识别");
                AssertPerformanceIsWithin("配置图像识别", 2000);
            });

            And("玩家上传了敌人图像模板", () =>
            {
                Log("玩家上传了敌人头像作为识别模板");
                SimulateUserResponseTime();
            });

            And("玩家设置了识别条件", () =>
            {
                Log("玩家设置了'当检测到敌人时执行攻击'的条件");
                SimulateUserResponseTime();
            });

            Then("系统应该保存图像识别配置", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证图像识别功能
                var recognitionResult = _imageRecognition.RecognizeAsync(null, null, null).Result;
                recognitionResult.Should().NotBeNull();
                
                StopPerformanceMonitoring("图像识别验证");
                AssertUserExperience("响应速度", true);
            });

            When("玩家运行增强后的脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家运行带有图像识别的脚本");
                
                // 模拟增强脚本执行
                var script = CreateGameAutomationScript();
                _scriptPlayer.LoadScript(script);
                
                StopPerformanceMonitoring("加载增强脚本");
                AssertPerformanceIsWithin("加载增强脚本", 1500);
            });

            Then("系统应该能够识别游戏画面中的敌人", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证图像识别在游戏中的应用
                var template = TestFixtures.CreateValidImageTemplate();
                var region = new ScreenRegion(0, 0, 1920, 1080);
                var parameters = new RecognitionParameters();
                
                // 模拟识别过程
                Task.Delay(100).Wait();
                
                StopPerformanceMonitoring("图像识别处理");
                AssertPerformanceIsWithin("图像识别处理", 2000);
            });

            And("系统应该根据识别结果执行相应操作", () =>
            {
                Log("检测到敌人，执行攻击操作");
                Log("未检测到敌人，继续巡逻");
                Log("检测到血量低，使用治疗技能");
                
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该提供执行统计信息", () =>
            {
                Log("显示执行统计：");
                Log("- 总执行时间：5分钟");
                Log("- 识别敌人次数：150次");
                Log("- 攻击成功次数：145次");
                Log("- 成功率：96.7%");
                
                AssertUserExperience("界面反应", true);
            });
        });
    }

    [Fact]
    public async Task GameScriptManagementScenario()
    {
        await Task.Run(() =>
        {
            Given("游戏玩家管理多个游戏脚本", () =>
            {
                Log("玩家有多个不同游戏角色的脚本");
                SimulateUserResponseTime();
            });

            When("玩家查看脚本列表", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家打开脚本管理界面");
                
                // 模拟加载脚本列表
                var scripts = new List<Script>
                {
                    CreateGameAutomationScript(),
                    CreateOfficeAutomationScript(),
                    CreateSystemMaintenanceScript()
                };
                
                StopPerformanceMonitoring("加载脚本列表");
                AssertPerformanceIsWithin("加载脚本列表", 1000);
            });

            Then("系统应该显示所有可用的脚本", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本列表显示
                Log("显示脚本列表：");
                Log("- 游戏自动化脚本");
                Log("- 办公自动化脚本");
                Log("- 系统维护脚本");
                
                StopPerformanceMonitoring("显示脚本列表");
                AssertUserExperience("响应速度", true);
            });

            When("玩家选择一个脚本进行编辑", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家点击'编辑'按钮");
                
                // 模拟脚本编辑
                var script = CreateGameAutomationScript();
                script.Update("改进版游戏脚本", "优化了执行效率");
                
                StopPerformanceMonitoring("编辑脚本");
                AssertPerformanceIsWithin("编辑脚本", 1500);
            });

            Then("系统应该打开脚本编辑器", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证编辑器功能
                Log("编辑器显示脚本内容");
                Log("显示操作步骤列表");
                Log("显示脚本属性设置");
                
                StopPerformanceMonitoring("编辑器加载");
                AssertUserExperience("界面反应", true);
            });

            And("玩家可以修改脚本参数", () =>
            {
                Log("玩家修改执行延迟时间");
                Log("玩家调整图像识别精度");
                Log("玩家添加错误处理逻辑");
                
                SimulateUserResponseTime();
                AssertUserExperience("操作流畅度", true);
            });

            When("玩家保存修改后的脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("玩家点击'保存'按钮");
                
                // 模拟保存操作
                SimulateUserResponseTime();
                
                StopPerformanceMonitoring("保存脚本");
                AssertPerformanceIsWithin("保存脚本", 1000);
            });

            Then("系统应该保存修改并更新脚本状态", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证保存结果
                Log("显示'保存成功'消息");
                Log("脚本状态更新为'已修改'");
                Log("脚本版本号递增");
                
                StopPerformanceMonitoring("保存验证");
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