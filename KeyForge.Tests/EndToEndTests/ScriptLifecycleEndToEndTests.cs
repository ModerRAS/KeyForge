using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using KeyForge.Application.Services;
using KeyForge.Application.Commands;
using KeyForge.Application.Queries;
using KeyForge.Core.Interfaces;
using KeyForge.Core.Models;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.EndToEndTests
{
    /// <summary>
    /// 完整的脚本生命周期端到端测试
    /// </summary>
    public class ScriptLifecycleEndToEndTests : TestBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IScriptService _scriptService;
        private readonly IScriptPlayer _scriptPlayer;
        private readonly IScriptRecorder _scriptRecorder;

        public ScriptLifecycleEndToEndTests()
        {
            // 设置完整的依赖注入
            var services = new ServiceCollection();
            
            // 添加所有服务
            services.AddInfrastructure();
            services.AddScoped<IScriptService, ScriptService>();
            services.AddSingleton<IInputSimulator, TestInputSimulator>();
            services.AddSingleton<ILoggerService, TestLoggerService>();
            services.AddSingleton<IScriptPlayer, ScriptPlayer>();
            services.AddSingleton<IScriptRecorder, ScriptRecorder>();
            
            _serviceProvider = services.BuildServiceProvider();
            
            // 获取服务实例
            _scriptService = _serviceProvider.GetRequiredService<IScriptService>();
            _scriptPlayer = _serviceProvider.GetRequiredService<IScriptPlayer>();
            _scriptRecorder = _serviceProvider.GetRequiredService<IScriptRecorder>();
        }

        [Fact]
        public async Task CompleteScriptLifecycle_ShouldWorkEndToEnd()
        {
            // 1. 录制脚本
            _scriptRecorder.StartRecording();
            
            // 模拟用户操作
            var inputSimulator = _serviceProvider.GetRequiredService<IInputSimulator>() as TestInputSimulator;
            inputSimulator?.SimulateKeyPress(KeyCode.A);
            inputSimulator?.SimulateKeyPress(KeyCode.B);
            inputSimulator?.SimulateKeyPress(KeyCode.C);
            inputSimulator?.SimulateMouseClick(MouseButton.Left, 100, 200);
            
            await Task.Delay(200); // 等待录制完成
            
            _scriptRecorder.StopRecording();
            
            // 获取录制的脚本
            var recordedScript = _scriptRecorder.GetRecordedScript();
            recordedScript.Should().NotBeNull();
            recordedScript.Actions.Should().HaveCountGreaterThanOrEqualTo(4);

            // 2. 保存脚本到数据库
            var createCommand = new CreateScriptCommand
            {
                Name = "Recorded Script",
                Description = "Script recorded from user actions",
                Actions = ConvertDomainActionsToDtos(recordedScript.Actions)
            };

            var savedScript = await _scriptService.Handle(createCommand, default);
            savedScript.Should().NotBeNull();
            savedScript.Name.Should().Be(createCommand.Name);
            savedScript.Actions.Should().HaveCount(recordedScript.Actions.Count);

            // 3. 从数据库加载脚本
            var loadQuery = new GetScriptQuery { Id = savedScript.Id };
            var loadedScript = await _scriptService.Handle(loadQuery, default);
            loadedScript.Should().NotBeNull();
            loadedScript.Id.Should().Be(savedScript.Id);

            // 4. 更新脚本
            var updateCommand = new UpdateScriptCommand
            {
                Id = savedScript.Id,
                Name = "Updated Recorded Script",
                Description = "Updated description"
            };

            var updatedScript = await _scriptService.Handle(updateCommand, default);
            updatedScript.Name.Should().Be(updateCommand.Name);

            // 5. 激活脚本
            var activateCommand = new ActivateScriptCommand { Id = savedScript.Id };
            var activateResult = await _scriptService.Handle(activateCommand, default);
            activateResult.Should().BeTrue();

            // 6. 播放脚本
            var domainScript = ConvertDtoToDomainScript(updatedScript);
            _scriptPlayer.LoadScript(domainScript);
            _scriptPlayer.PlayScript();

            // 验证播放状态
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.CurrentScript.Should().NotBeNull();

            // 7. 暂停和恢复播放
            _scriptPlayer.PauseScript();
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeTrue();

            _scriptPlayer.PlayScript();
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.IsPaused.Should().BeFalse();

            // 8. 停止播放
            _scriptPlayer.StopScript();
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeFalse();

            // 9. 停用脚本
            var deactivateCommand = new DeactivateScriptCommand { Id = savedScript.Id };
            var deactivateResult = await _scriptService.Handle(deactivateCommand, default);
            deactivateResult.Should().BeTrue();

            // 10. 删除脚本
            var deleteCommand = new DeleteScriptCommand { Id = savedScript.Id };
            var deleteResult = await _scriptService.Handle(deleteCommand, default);
            deleteResult.Should().BeTrue();

            // 11. 验证删除状态
            var deletedScript = await _scriptService.Handle(new GetScriptQuery { Id = savedScript.Id }, default);
            deletedScript.Status.Should().Be(ScriptStatus.Deleted);
        }

        [Fact]
        public async Task ComplexScriptWorkflow_ShouldHandleMultipleScenarios()
        {
            // 场景1: 创建复杂的自动化脚本
            var complexScript = await CreateComplexAutomationScript();
            
            // 场景2: 执行脚本并验证结果
            await ExecuteAndVerifyScript(complexScript);
            
            // 场景3: 修改脚本并重新执行
            await ModifyAndReexecuteScript(complexScript);
            
            // 场景4: 错误处理和恢复
            await TestErrorHandlingAndRecovery(complexScript);
        }

        [Fact]
        public async Task ConcurrentUsers_ShouldHandleMultipleSessions()
        {
            // 模拟多个用户同时操作
            var user1Task = SimulateUserSession("User1");
            var user2Task = SimulateUserSession("User2");
            var user3Task = SimulateUserSession("User3");

            await Task.WhenAll(user1Task, user2Task, user3Task);

            // 验证所有用户都成功完成操作
            var user1Result = await user1Task;
            var user2Result = await user2Task;
            var user3Result = await user3Task;

            user1Result.Should().NotBeNull();
            user2Result.Should().NotBeNull();
            user3Result.Should().NotBeNull();

            // 验证脚本ID各不相同
            user1Result.Id.Should().NotBe(user2Result.Id);
            user1Result.Id.Should().NotBe(user3Result.Id);
            user2Result.Id.Should().NotBe(user3Result.Id);
        }

        [Fact]
        public async Task PerformanceStressTest_ShouldHandleHighLoad()
        {
            const int scriptCount = 50;
            const int actionsPerScript = 100;

            var tasks = new Task<Application.DTOs.ScriptDto>[scriptCount];

            // 并发创建大量脚本
            for (int i = 0; i < scriptCount; i++)
            {
                tasks[i] = CreateStressTestScript(i, actionsPerScript);
            }

            var startTime = DateTime.UtcNow;
            var results = await Task.WhenAll(tasks);
            var totalTime = DateTime.UtcNow - startTime;

            // 验证性能
            totalTime.Should().BeLessThan(TimeSpan.FromSeconds(30)); // 30秒内完成
            results.Should().HaveCount(scriptCount);

            // 验证所有脚本都创建成功
            foreach (var result in results)
            {
                result.Should().NotBeNull();
                result.Actions.Should().HaveCount(actionsPerScript);
            }

            // 验证查询性能
            var queryStart = DateTime.UtcNow;
            var allScripts = await _scriptService.Handle(new GetAllScriptsQuery(), default);
            var queryTime = DateTime.UtcNow - queryStart;

            queryTime.Should().BeLessThan(TimeSpan.FromSeconds(5)); // 5秒内完成查询
            allScripts.Should().HaveCountGreaterThanOrEqualTo(scriptCount);
        }

        private async Task<Application.DTOs.ScriptDto> CreateComplexAutomationScript()
        {
            // 创建包含多种操作类型的复杂脚本
            var createCommand = new CreateScriptCommand
            {
                Name = "Complex Automation Script",
                Description = "Script with various action types",
                Actions = new System.Collections.Generic.List<Application.DTOs.GameActionDto>()
            };

            // 添加键盘操作
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.KeyDown,
                Key = KeyCode.Ctrl,
                Delay = 100
            });
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.KeyDown,
                Key = KeyCode.C,
                Delay = 50
            });
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.KeyUp,
                Key = KeyCode.C,
                Delay = 50
            });
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.KeyUp,
                Key = KeyCode.Ctrl,
                Delay = 50
            });

            // 添加鼠标操作
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.MouseMove,
                Button = MouseButton.Left,
                X = 100,
                Y = 200,
                Delay = 200
            });
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.MouseDown,
                Button = MouseButton.Left,
                X = 100,
                Y = 200,
                Delay = 100
            });
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.MouseUp,
                Button = MouseButton.Left,
                X = 100,
                Y = 200,
                Delay = 100
            });

            // 添加延迟
            createCommand.Actions.Add(new Application.DTOs.GameActionDto
            {
                Type = ActionType.Delay,
                Delay = 1000
            });

            return await _scriptService.Handle(createCommand, default);
        }

        private async Task ExecuteAndVerifyScript(Application.DTOs.ScriptDto scriptDto)
        {
            // 激活脚本
            var activateCommand = new ActivateScriptCommand { Id = scriptDto.Id };
            await _scriptService.Handle(activateCommand, default);

            // 转换为领域模型并执行
            var domainScript = ConvertDtoToDomainScript(scriptDto);
            _scriptPlayer.LoadScript(domainScript);
            _scriptPlayer.PlayScript();

            // 验证执行状态
            _scriptPlayer.IsPlaying.Should().BeTrue();

            // 等待执行完成
            await Task.Delay((int)scriptDto.EstimatedDuration.TotalMilliseconds + 1000);

            // 停止播放
            _scriptPlayer.StopScript();
            _scriptPlayer.IsPlaying.Should().BeFalse();
        }

        private async Task ModifyAndReexecuteScript(Application.DTOs.ScriptDto originalScript)
        {
            // 修改脚本
            var updateCommand = new UpdateScriptCommand
            {
                Id = originalScript.Id,
                Name = "Modified Script",
                Description = "Modified description"
            };

            var modifiedScript = await _scriptService.Handle(updateCommand, default);
            modifiedScript.Name.Should().Be(updateCommand.Name);

            // 重新执行
            await ExecuteAndVerifyScript(modifiedScript);
        }

        private async Task TestErrorHandlingAndRecovery(Application.DTOs.ScriptDto script)
        {
            // 测试各种错误情况
            var invalidUpdateCommand = new UpdateScriptCommand
            {
                Id = Guid.NewGuid(), // 不存在的ID
                Name = "Invalid Update",
                Description = "This should fail"
            };

            // 验证错误处理
            var action = async () => await _scriptService.Handle(invalidUpdateCommand, default);
            await action.Should().ThrowAsync<EntityNotFoundException>();

            // 验证原始脚本仍然可用
            var originalScript = await _scriptService.Handle(new GetScriptQuery { Id = script.Id }, default);
            originalScript.Should().NotBeNull();
        }

        private async Task<Application.DTOs.ScriptDto> SimulateUserSession(string userName)
        {
            // 模拟用户会话
            _scriptRecorder.StartRecording();

            // 模拟用户操作
            var inputSimulator = _serviceProvider.GetRequiredService<IInputSimulator>() as TestInputSimulator;
            inputSimulator?.SimulateKeyPress(KeyCode.A);
            inputSimulator?.SimulateMouseClick(MouseButton.Left, 100, 200);

            await Task.Delay(100);

            _scriptRecorder.StopRecording();

            var recordedScript = _scriptRecorder.GetRecordedScript();
            var createCommand = new CreateScriptCommand
            {
                Name = $"{userName}'s Script",
                Description = $"Script created by {userName}",
                Actions = ConvertDomainActionsToDtos(recordedScript.Actions)
            };

            return await _scriptService.Handle(createCommand, default);
        }

        private async Task<Application.DTOs.ScriptDto> CreateStressTestScript(int index, int actionCount)
        {
            var createCommand = new CreateScriptCommand
            {
                Name = $"Stress Test Script {index}",
                Description = $"Script {index} with {actionCount} actions",
                Actions = new System.Collections.Generic.List<Application.DTOs.GameActionDto>()
            };

            for (int i = 0; i < actionCount; i++)
            {
                createCommand.Actions.Add(new Application.DTOs.GameActionDto
                {
                    Type = i % 2 == 0 ? ActionType.KeyDown : ActionType.MouseDown,
                    Key = i % 2 == 0 ? KeyCode.A : null,
                    Button = i % 2 == 1 ? MouseButton.Left : null,
                    X = i % 2 == 1 ? 100 + i : 0,
                    Y = i % 2 == 1 ? 200 + i : 0,
                    Delay = 50
                });
            }

            return await _scriptService.Handle(createCommand, default);
        }

        private static System.Collections.Generic.List<Application.DTOs.GameActionDto> ConvertDomainActionsToDtos(
            System.Collections.Generic.IReadOnlyCollection<GameAction> actions)
        {
            var dtos = new System.Collections.Generic.List<Application.DTOs.GameActionDto>();
            foreach (var action in actions)
            {
                dtos.Add(new Application.DTOs.GameActionDto
                {
                    Id = action.Id,
                    Type = action.Type,
                    Key = action.Key,
                    Button = action.Button,
                    X = action.X,
                    Y = action.Y,
                    Delay = action.Delay,
                    Description = action.Description
                });
            }
            return dtos;
        }

        private static Script ConvertDtoToDomainScript(Application.DTOs.ScriptDto dto)
        {
            var script = new Script(dto.Id, dto.Name, dto.Description);
            
            foreach (var actionDto in dto.Actions)
            {
                var action = actionDto.Type switch
                {
                    ActionType.KeyDown or ActionType.KeyUp => new GameAction(
                        actionDto.Id,
                        actionDto.Type,
                        actionDto.Key.Value,
                        actionDto.Delay,
                        actionDto.Description),
                    
                    ActionType.MouseDown or ActionType.MouseUp or ActionType.MouseMove => new GameAction(
                        actionDto.Id,
                        actionDto.Type,
                        actionDto.Button.Value,
                        actionDto.X,
                        actionDto.Y,
                        actionDto.Delay,
                        actionDto.Description),
                    
                    ActionType.Delay => new GameAction(
                        actionDto.Id,
                        actionDto.Type,
                        actionDto.Delay,
                        actionDto.Description),
                    
                    _ => throw new ArgumentException($"Unsupported action type: {actionDto.Type}")
                };
                
                script.AddAction(action);
            }
            
            return script;
        }
    }
}