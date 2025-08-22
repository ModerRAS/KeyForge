using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Moq;

namespace KeyForge.Tests.Simplified
{
    /// <summary>
    /// 简化的测试基类
    /// 专门为跨平台环境设计，避免Windows特定的依赖
    /// </summary>
    public class SimplifiedTestBase
    {
        /// <summary>
        /// 创建模拟的日志服务
        /// </summary>
        protected static Mock<ISimplifiedLoggerService> CreateMockLogger()
        {
            var mockLogger = new Mock<ISimplifiedLoggerService>();
            mockLogger.Setup(x => x.LogInformation(It.IsAny<string>()));
            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>()));
            mockLogger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()));
            mockLogger.Setup(x => x.LogDebug(It.IsAny<string>()));
            return mockLogger;
        }

        /// <summary>
        /// 创建模拟的输入模拟器
        /// </summary>
        protected static Mock<ISimplifiedInputSimulator> CreateMockInputSimulator()
        {
            var mockSimulator = new Mock<ISimplifiedInputSimulator>();
            mockSimulator.Setup(x => x.KeyDown(It.IsAny<SimplifiedKeyCode>()));
            mockSimulator.Setup(x => x.KeyUp(It.IsAny<SimplifiedKeyCode>()));
            mockSimulator.Setup(x => x.MouseClick(It.IsAny<SimplifiedMouseButton>(), It.IsAny<int>(), It.IsAny<int>()));
            return mockSimulator;
        }

        /// <summary>
        /// 验证对象不为null
        /// </summary>
        protected static void ShouldNotBeNull(object obj, string message = "Object should not be null")
        {
            obj.Should().NotBeNull(message);
        }

        /// <summary>
        /// 验证字符串不为空
        /// </summary>
        protected static void ShouldNotBeEmpty(string str, string message = "String should not be empty")
        {
            str.Should().NotBeNullOrEmpty(message);
        }

        /// <summary>
        /// 验证集合不为空
        /// </summary>
        protected static void ShouldNotBeEmpty<T>(ICollection<T> collection, string message = "Collection should not be empty")
        {
            collection.Should().NotBeEmpty(message);
        }

        /// <summary>
        /// 验证条件为true
        /// </summary>
        protected static void ShouldBeTrue(bool condition, string message = "Condition should be true")
        {
            condition.Should().BeTrue(message);
        }

        /// <summary>
        /// 验证条件为false
        /// </summary>
        protected static void ShouldBeFalse(bool condition, string message = "Condition should be false")
        {
            condition.Should().BeFalse(message);
        }

        /// <summary>
        /// 验证抛出指定类型的异常
        /// </summary>
        protected static void ShouldThrow<T>(Action action, string message = "Expected exception was not thrown") where T : Exception
        {
            action.Should().Throw<T>(message);
        }

        /// <summary>
        /// 验证两个对象相等
        /// </summary>
        protected static void ShouldEqual<T>(T expected, T actual, string message = "Objects should be equal")
        {
            actual.Should().Be(expected, message);
        }

        /// <summary>
        /// 验证时间在指定范围内
        /// </summary>
        protected static void ShouldBeCloseTo(DateTime expected, DateTime actual, TimeSpan precision, string message = "DateTime should be close to expected")
        {
            actual.Should().BeCloseTo(expected, precision, message);
        }
    }

    /// <summary>
    /// 简化的日志服务接口
    /// </summary>
    public interface ISimplifiedLoggerService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogDebug(string message);
    }

    /// <summary>
    /// 简化的输入模拟器接口
    /// </summary>
    public interface ISimplifiedInputSimulator
    {
        void KeyDown(SimplifiedKeyCode key);
        void KeyUp(SimplifiedKeyCode key);
        void KeyPress(SimplifiedKeyCode key, int delay = 50);
        void MouseDown(SimplifiedMouseButton button, int x, int y);
        void MouseUp(SimplifiedMouseButton button, int x, int y);
        void MouseMove(int x, int y);
        void MouseClick(SimplifiedMouseButton button, int x, int y, int delay = 100);
        void MouseDoubleClick(SimplifiedMouseButton button, int x, int y);
        void MouseScroll(int delta);
    }

    /// <summary>
    /// 简化的按键码枚举
    /// </summary>
    public enum SimplifiedKeyCode
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        Space, Enter, Escape, Tab, Shift, Control, Alt
    }

    /// <summary>
    /// 简化的鼠标按钮枚举
    /// </summary>
    public enum SimplifiedMouseButton
    {
        Left, Right, Middle, X1, X2
    }
}