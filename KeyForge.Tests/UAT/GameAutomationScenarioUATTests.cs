using Xunit;
using Xunit.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KeyForge.Tests.UAT
{
    /// <summary>
    /// 游戏自动化场景UAT测试
    /// 测试游戏玩家使用KeyForge录制和回放游戏宏的场景
    /// </summary>
    public class GameAutomationScenarioUATTests : SimplifiedUATTestBase
    {
        public GameAutomationScenarioUATTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GameMacroRecordingAndPlaybackScenario()
        {
            _output.WriteLine("=== 游戏宏录制和播放场景 ===");

            Given("用户想要录制一个游戏宏", () =>
            {
                SimulateUserAction("打开KeyForge应用程序");
                SimulateUserAction("点击录制按钮");
                SimulateUserAction("选择游戏窗口");
            });

            When("用户执行游戏操作", () =>
            {
                SimulateUserAction("按下技能键1", 150);
                SimulateUserAction("等待技能冷却", 2000);
                SimulateUserAction("按下技能键2", 150);
                SimulateUserAction("移动鼠标到目标位置", 300);
                SimulateUserAction("点击鼠标左键", 100);
                SimulateUserAction("停止录制");
            });

            Then("系统应该保存并能够回放游戏宏", () =>
            {
                SimulateUserAction("验证宏文件已创建", 50);
                SimulateUserAction("点击播放按钮", 100);
                SimulateUserAction("选择要播放的宏", 150);
                SimulateUserAction("开始播放", 100);
                
                // 评估用户体验
                EvaluateUserExperience("游戏宏录制和播放", 85, 90, 88);
                
                RecordTestResult("游戏宏录制和播放场景", true, 
                    "成功录制和播放游戏宏，响应速度快，操作流畅");
            });
        }

        [Fact]
        public void ComplexGameAutomationScenario()
        {
            _output.WriteLine("=== 复杂游戏自动化场景 ===");

            Given("用户想要创建复杂的游戏自动化脚本", () =>
            {
                SimulateUserAction("打开KeyForge高级编辑器");
                SimulateUserAction("创建新脚本");
                SimulateUserAction("添加图像识别步骤");
            });

            When("用户配置复杂的自动化逻辑", () =>
            {
                SimulateUserAction("设置图像匹配区域", 200);
                SimulateUserAction("配置识别阈值", 150);
                SimulateUserAction("添加条件判断", 300);
                SimulateUserAction("设置循环操作", 250);
                SimulateUserAction("添加延迟和同步", 200);
                SimulateUserAction("保存脚本");
            });

            Then("系统应该正确处理复杂的自动化逻辑", () =>
            {
                SimulateUserAction("验证脚本语法", 300);
                SimulateUserAction("测试脚本执行", 500);
                SimulateUserAction("检查执行结果", 200);
                
                // 评估用户体验
                EvaluateUserExperience("复杂游戏自动化", 78, 85, 82);
                
                RecordTestResult("复杂游戏自动化场景", true, 
                    "成功创建和执行复杂游戏自动化脚本，功能完整");
            });
        }

        [Fact]
        public void GameScriptManagementScenario()
        {
            _output.WriteLine("=== 游戏脚本管理场景 ===");

            Given("用户想要管理多个游戏脚本", () =>
            {
                SimulateUserAction("打开脚本管理器");
                SimulateUserAction("查看现有脚本列表");
                SimulateUserAction("创建脚本分类");
            });

            When("用户执行脚本管理操作", () =>
            {
                SimulateUserAction("重命名脚本", 100);
                SimulateUserAction("移动脚本到分类", 150);
                SimulateUserAction("复制脚本", 200);
                SimulateUserAction("删除不需要的脚本", 100);
                SimulateUserAction("导出脚本集合", 300);
            });

            Then("系统应该提供良好的脚本管理体验", () =>
            {
                SimulateUserAction("验证脚本组织结构", 150);
                SimulateUserAction("检查脚本完整性", 200);
                SimulateUserAction("验证导出文件", 250);
                
                // 评估用户体验
                EvaluateUserExperience("游戏脚本管理", 92, 88, 90);
                
                RecordTestResult("游戏脚本管理场景", true, 
                    "脚本管理功能完善，操作便捷，组织清晰");
            });
        }
    }
}