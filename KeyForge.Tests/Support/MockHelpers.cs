using Moq;
using KeyForge.Domain;
using KeyForge.Core;

namespace KeyForge.Tests.Support;

/// <summary>
/// Mock对象辅助类
/// 原本实现：复杂的Mock配置
/// 简化实现：基础的Mock创建
/// </summary>
public static class MockHelpers
{
    public static Mock<IScriptRepository> CreateMockRepository()
    {
        var mock = new Mock<IScriptRepository>();
        
        // 默认行为：返回测试脚本
        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => TestFixtures.CreateValidScript());
            
        mock.Setup(x => x.SaveAsync(It.IsAny<Script>()))
            .Returns(Task.CompletedTask);
            
        mock.Setup(x => x.UpdateAsync(It.IsAny<Script>()))
            .Returns(Task.CompletedTask);
            
        mock.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
            
        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(TestFixtures.CreateMultipleScripts());
            
        return mock;
    }

    public static Mock<IScriptExecutor> CreateMockExecutor()
    {
        var mock = new Mock<IScriptExecutor>();
        
        // 默认行为：执行成功
        mock.Setup(x => x.ExecuteAsync(It.IsAny<Script>()))
            .ReturnsAsync(true);
            
        mock.Setup(x => x.StopAsync())
            .Returns(Task.CompletedTask);
            
        mock.Setup(x => x.PauseAsync())
            .Returns(Task.CompletedTask);
            
        mock.Setup(x => x.ResumeAsync())
            .Returns(Task.CompletedTask);
            
        return mock;
    }

    public static Mock<IImageRecognizer> CreateMockImageRecognizer()
    {
        var mock = new Mock<IImageRecognizer>();
        
        // 默认行为：识别成功
        mock.Setup(x => x.RecognizeAsync(It.IsAny<ImageTemplate>(), It.IsAny<Rectangle>()))
            .ReturnsAsync(new RecognitionResult(true, 0.9, new Point(100, 100), 50));
            
        return mock;
    }

    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    public static Mock<IScriptRepository> CreateMockRepositoryWithException()
    {
        var mock = new Mock<IScriptRepository>();
        
        // 模拟异常情况
        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));
            
        return mock;
    }

    public static Mock<IScriptExecutor> CreateMockExecutorWithFailure()
    {
        var mock = new Mock<IScriptExecutor>();
        
        // 模拟执行失败
        mock.Setup(x => x.ExecuteAsync(It.IsAny<Script>()))
            .ReturnsAsync(false);
            
        return mock;
    }
}