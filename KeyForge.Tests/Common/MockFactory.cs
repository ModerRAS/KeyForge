using System;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Interfaces;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
using KeyForge.Application.DTOs;
using KeyForge.Application.Commands;
using KeyForge.Application.Queries;
using MediatR;
using Moq;

namespace KeyForge.Tests.Common
{
    /// <summary>
    /// 模拟对象工厂 - 创建各种模拟对象用于测试
    /// </summary>
    public static class MockFactory
    {
        #region Repository Mocks

        public static Mock<IScriptRepository> CreateScriptRepositoryMock()
        {
            var mock = new Mock<IScriptRepository>();
            
            // 设置默认行为
            mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => null);
            
            mock.Setup(r => r.AddAsync(It.IsAny<Script>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(r => r.UpdateAsync(It.IsAny<Script>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(Array.Empty<Script>());
            
            mock.Setup(r => r.GetByStatusAsync(It.IsAny<ScriptStatus>()))
                .ReturnsAsync(Array.Empty<Script>());

            return mock;
        }

        public static Mock<IStateMachineRepository> CreateStateMachineRepositoryMock()
        {
            var mock = new Mock<IStateMachineRepository>();
            
            mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => null);
            
            mock.Setup(r => r.AddAsync(It.IsAny<StateMachine>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(r => r.UpdateAsync(It.IsAny<StateMachine>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(Array.Empty<StateMachine>());

            return mock;
        }

        public static Mock<IImageTemplateRepository> CreateImageTemplateRepositoryMock()
        {
            var mock = new Mock<IImageTemplateRepository>();
            
            mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => null);
            
            mock.Setup(r => r.AddAsync(It.IsAny<ImageTemplate>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(r => r.UpdateAsync(It.IsAny<ImageTemplate>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(Array.Empty<ImageTemplate>());

            return mock;
        }

        #endregion

        #region Unit of Work Mock

        public static Mock<IUnitOfWork> CreateUnitOfWorkMock()
        {
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(u => u.CommitAsync())
                .ReturnsAsync(1);
            return mock;
        }

        #endregion

        #region Service Mocks

        public static Mock<IInputSimulator> CreateInputSimulatorMock()
        {
            var mock = new Mock<IInputSimulator>();
            
            // 设置所有方法为空实现
            mock.Setup(s => s.KeyDown(It.IsAny<KeyCode>()));
            mock.Setup(s => s.KeyUp(It.IsAny<KeyCode>()));
            mock.Setup(s => s.KeyPress(It.IsAny<KeyCode>(), It.IsAny<int>()));
            mock.Setup(s => s.MouseDown(It.IsAny<MouseButton>(), It.IsAny<int>(), It.IsAny<int>()));
            mock.Setup(s => s.MouseUp(It.IsAny<MouseButton>(), It.IsAny<int>(), It.IsAny<int>()));
            mock.Setup(s => s.MouseMove(It.IsAny<int>(), It.IsAny<int>()));
            mock.Setup(s => s.MouseClick(It.IsAny<MouseButton>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));
            mock.Setup(s => s.MouseDoubleClick(It.IsAny<MouseButton>(), It.IsAny<int>(), It.IsAny<int>()));
            mock.Setup(s => s.MouseScroll(It.IsAny<int>()));
            
            return mock;
        }

        public static Mock<IScriptRecorder> CreateScriptRecorderMock()
        {
            var mock = new Mock<IScriptRecorder>();
            
            mock.Setup(s => s.IsRecording).Returns(false);
            mock.Setup(s => s.GetRecordedScript()).Returns((Script)null);
            
            return mock;
        }

        public static Mock<IScriptPlayer> CreateScriptPlayerMock()
        {
            var mock = new Mock<IScriptPlayer>();
            
            mock.Setup(s => s.IsPlaying).Returns(false);
            mock.Setup(s => s.IsPaused).Returns(false);
            mock.Setup(s => s.CurrentScript).Returns((Script)null);
            
            return mock;
        }

        public static Mock<IConfigManager> CreateConfigManagerMock()
        {
            var mock = new Mock<IConfigManager>();
            
            mock.Setup(c => c.LoadConfig(It.IsAny<string>()))
                .Returns(new Config());
            
            mock.Setup(c => c.LoadScript(It.IsAny<string>()))
                .Returns((Script)null);
            
            return mock;
        }

        public static Mock<ILoggerService> CreateLoggerServiceMock()
        {
            var mock = new Mock<ILoggerService>();
            
            mock.Setup(l => l.LogInformation(It.IsAny<string>()));
            mock.Setup(l => l.LogWarning(It.IsAny<string>()));
            mock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()));
            mock.Setup(l => l.LogDebug(It.IsAny<string>()));
            
            return mock;
        }

        #endregion

        #region Mediator Mock

        public static Mock<IMediator> CreateMediatorMock()
        {
            var mock = new Mock<IMediator>();
            
            // 设置默认行为 - 返回 null 或空集合
            mock.Setup(m => m.Send(It.IsAny<CreateScriptCommand>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((ScriptDto)null);
            
            mock.Setup(m => m.Send(It.IsAny<GetScriptQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((ScriptDto)null);
            
            mock.Setup(m => m.Send(It.IsAny<GetAllScriptsQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(Array.Empty<ScriptDto>());

            return mock;
        }

        #endregion

        #region Image Recognition Mock

        public static Mock<IImageRecognitionService> CreateImageRecognitionServiceMock()
        {
            var mock = new Mock<IImageRecognitionService>();
            
            mock.Setup(s => s.RecognizeImageAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<double>()))
                .ReturnsAsync(new RecognitionResult(false, 0.0, new System.Drawing.Rectangle()));
            
            mock.Setup(s => s.CaptureScreenAsync())
                .ReturnsAsync(new byte[0]);
            
            return mock;
        }

        #endregion

        #region 配置模拟对象的便捷方法

        public static void SetupScriptRepositoryWithScript(Mock<IScriptRepository> mock, Script script)
        {
            mock.Setup(r => r.GetByIdAsync(script.Id))
                .ReturnsAsync(script);
            
            mock.Setup(r => r.AddAsync(script))
                .Callback<Script>(s => { });
            
            mock.Setup(r => r.UpdateAsync(script))
                .Callback<Script>(s => { });
        }

        public static void SetupScriptRepositoryWithScripts(Mock<IScriptRepository> mock, params Script[] scripts)
        {
            mock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(scripts);
            
            foreach (var script in scripts)
            {
                SetupScriptRepositoryWithScript(mock, script);
            }
        }

        public static void SetupMediatorForCreateScript(Mock<IMediator> mock, ScriptDto expectedDto)
        {
            mock.Setup(m => m.Send(It.IsAny<CreateScriptCommand>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(expectedDto);
        }

        public static void SetupMediatorForGetScript(Mock<IMediator> mock, ScriptDto expectedDto)
        {
            mock.Setup(m => m.Send(It.IsAny<GetScriptQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(expectedDto);
        }

        public static void SetupMediatorForGetAllScripts(Mock<IMediator> mock, params ScriptDto[] dtos)
        {
            mock.Setup(m => m.Send(It.IsAny<GetAllScriptsQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(dtos);
        }

        #endregion
    }
}