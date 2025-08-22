using System;
using System.Collections.Generic;
using System.Drawing;
using KeyForge.HAL.Abstractions;

namespace KeyForge.Tests.TestSupport
{
    /// <summary>
    /// 测试数据工厂接口
    /// </summary>
    public interface ITestDataFactory
    {
        // 平台信息
        PlatformInfo CreatePlatformInfo(Platform platform = Platform.Windows);
        
        // 键盘数据
        KeyCombination CreateKeyCombination(KeyCode key, KeyModifiers modifiers = KeyModifiers.None);
        HotkeyCombination CreateHotkeyCombination(KeyCode key, KeyModifiers modifiers = KeyModifiers.None);
        KeySequence CreateKeySequence();
        
        // 鼠标数据
        Point CreatePoint(int x = 100, int y = 100);
        Rectangle CreateRectangle(int x = 0, int y = 0, int width = 100, int height = 100);
        MouseState CreateMouseState(Point? position = null);
        
        // 屏幕数据
        ScreenInfo CreateScreenInfo(int index = 0, bool isPrimary = true);
        Color CreateColor(byte r = 255, byte g = 255, byte b = 255, byte a = 255);
        
        // 窗口数据
        WindowInfo CreateWindowInfo(IntPtr? handle = null);
        
        // 热键数据
        HotkeyInfo CreateHotkeyInfo(string id = "test_hotkey");
        
        // 图像识别数据
        RecognitionResult CreateRecognitionResult(bool isFound = false);
        
        // 权限数据
        PermissionRequest CreatePermissionRequest(PermissionType type = PermissionType.KeyboardInput);
        
        // 性能数据
        PerformanceMetrics CreatePerformanceMetrics();
        HealthCheckResult CreateHealthCheckResult(bool isHealthy = true);
    }

    /// <summary>
    /// 测试数据工厂实现
    /// 简化实现：基本的测试数据生成
    /// 原本实现：包含复杂的数据生成和验证逻辑
    /// </summary>
    public class TestDataFactory : ITestDataFactory
    {
        private readonly Random _random;
        
        public TestDataFactory()
        {
            _random = new Random(42); // 固定种子确保测试可重复
        }
        
        /// <summary>
        /// 创建平台信息
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <returns>平台信息</returns>
        public PlatformInfo CreatePlatformInfo(Platform platform = Platform.Windows)
        {
            return new PlatformInfo
            {
                Platform = platform,
                Name = platform.ToString(),
                Version = "10.0",
                Architecture = "x64",
                Capabilities = new PlatformCapabilities
                {
                    SupportsGlobalHotkeys = true,
                    SupportsLowLevelKeyboardHook = true,
                    SupportsLowLevelMouseHook = true,
                    SupportsMultipleDisplays = true,
                    SupportsWindowTransparency = true,
                    SupportsWindowTopmost = true,
                    SupportsScreenRecording = true,
                    SupportsAccessibility = true
                }
            };
        }
        
        /// <summary>
        /// 创建键盘组合
        /// </summary>
        /// <param name="key">按键代码</param>
        /// <param name="modifiers">修饰键</param>
        /// <returns>键盘组合</returns>
        public KeyCombination CreateKeyCombination(KeyCode key = KeyCode.A, KeyModifiers modifiers = KeyModifiers.None)
        {
            return new KeyCombination(key, modifiers, _random.Next(0, 100));
        }
        
        /// <summary>
        /// 创建热键组合
        /// </summary>
        /// <param name="key">按键代码</param>
        /// <param name="modifiers">修饰键</param>
        /// <returns>热键组合</returns>
        public HotkeyCombination CreateHotkeyCombination(KeyCode key = KeyCode.F1, KeyModifiers modifiers = KeyModifiers.Control | KeyModifiers.Alt)
        {
            return new HotkeyCombination(key, modifiers, $"Hotkey_{key}_{modifiers}");
        }
        
        /// <summary>
        /// 创建键盘序列
        /// </summary>
        /// <returns>键盘序列</returns>
        public KeySequence CreateKeySequence()
        {
            var sequence = new KeySequence
            {
                Name = "Test Sequence",
                Description = "Test key sequence"
            };
            
            // 添加一些测试按键
            sequence.AddKey(KeyCode.H);
            sequence.AddKey(KeyCode.E);
            sequence.AddKey(KeyCode.L);
            sequence.AddKey(KeyCode.L);
            sequence.AddKey(KeyCode.O);
            
            return sequence;
        }
        
        /// <summary>
        /// 创建点
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>点</returns>
        public Point CreatePoint(int x = 100, int y = 100)
        {
            return new Point(x, y);
        }
        
        /// <summary>
        /// 创建矩形
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>矩形</returns>
        public Rectangle CreateRectangle(int x = 0, int y = 0, int width = 100, int height = 100)
        {
            return new Rectangle(x, y, width, height);
        }
        
        /// <summary>
        /// 创建鼠标状态
        /// </summary>
        /// <param name="position">鼠标位置</param>
        /// <returns>鼠标状态</returns>
        public MouseState CreateMouseState(Point? position = null)
        {
            return new MouseState
            {
                Position = position ?? CreatePoint(),
                LeftButton = MouseButtonState.Released,
                RightButton = MouseButtonState.Released,
                MiddleButton = MouseButtonState.Released,
                X1Button = MouseButtonState.Released,
                X2Button = MouseButtonState.Released,
                WheelPosition = 0
            };
        }
        
        /// <summary>
        /// 创建屏幕信息
        /// </summary>
        /// <param name="index">屏幕索引</param>
        /// <param name="isPrimary">是否为主屏幕</param>
        /// <returns>屏幕信息</returns>
        public ScreenInfo CreateScreenInfo(int index = 0, bool isPrimary = true)
        {
            var bounds = CreateRectangle(index * 1920, 0, 1920, 1080);
            var workingArea = CreateRectangle(index * 1920, 0, 1920, 1040);
            
            return new ScreenInfo
            {
                Index = index,
                Bounds = bounds,
                WorkingArea = workingArea,
                IsPrimary = isPrimary,
                DeviceName = $"\\\\.\\DISPLAY{index + 1}",
                DisplayName = $"Display {index + 1}",
                Resolution = new Size(bounds.Width, bounds.Height),
                RefreshRate = 60,
                ColorDepth = 32
            };
        }
        
        /// <summary>
        /// 创建颜色
        /// </summary>
        /// <param name="r">红色分量</param>
        /// <param name="g">绿色分量</param>
        /// <param name="b">蓝色分量</param>
        /// <param name="a">透明度分量</param>
        /// <returns>颜色</returns>
        public Color CreateColor(byte r = 255, byte g = 255, byte b = 255, byte a = 255)
        {
            return new Color(r, g, b, a);
        }
        
        /// <summary>
        /// 创建窗口信息
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <returns>窗口信息</returns>
        public WindowInfo CreateWindowInfo(IntPtr? handle = null)
        {
            var windowHandle = handle ?? new IntPtr(_random.Next(1, 10000));
            var bounds = CreateRectangle(_random.Next(0, 1000), _random.Next(0, 1000), 800, 600);
            
            return new WindowInfo
            {
                Handle = windowHandle,
                Title = $"Test Window {windowHandle}",
                ClassName = "TestWindowClass",
                Bounds = bounds,
                State = WindowState.Normal,
                IsVisible = true,
                IsActive = false,
                IsTopmost = false,
                ProcessId = _random.Next(1000, 9999),
                ProcessName = "TestProcess.exe"
            };
        }
        
        /// <summary>
        /// 创建热键信息
        /// </summary>
        /// <param name="id">热键ID</param>
        /// <returns>热键信息</returns>
        public HotkeyInfo CreateHotkeyInfo(string id = "test_hotkey")
        {
            var hotkey = CreateHotkeyCombination();
            return new HotkeyInfo(id, hotkey, $"Tag_{id}");
        }
        
        /// <summary>
        /// 创建识别结果
        /// </summary>
        /// <param name="isFound">是否找到</param>
        /// <returns>识别结果</returns>
        public RecognitionResult CreateRecognitionResult(bool isFound = false)
        {
            if (isFound)
            {
                return RecognitionResult.CreateSuccess(
                    CreatePoint(_random.Next(0, 1000), _random.Next(0, 1000)),
                    _random.NextDouble() * 0.5 + 0.5, // 0.5-1.0
                    CreateRectangle(_random.Next(0, 1000), _random.Next(0, 1000), 50, 50),
                    _random.NextDouble(),
                    _random.NextDouble() * 100
                );
            }
            else
            {
                return RecognitionResult.CreateFailure("No match found");
            }
        }
        
        /// <summary>
        /// 创建权限请求
        /// </summary>
        /// <param name="type">权限类型</param>
        /// <returns>权限请求</returns>
        public PermissionRequest CreatePermissionRequest(PermissionType type = PermissionType.KeyboardInput)
        {
            return new PermissionRequest(type, $"Request for {type} permission", true)
            {
                UserPrompt = $"Please grant {type} permission",
                TimeoutMs = 30000,
                ResourceId = $"resource_{type.ToString().ToLower()}"
            };
        }
        
        /// <summary>
        /// 创建性能指标
        /// </summary>
        /// <returns>性能指标</returns>
        public PerformanceMetrics CreatePerformanceMetrics()
        {
            var metrics = new PerformanceMetrics
            {
                CpuUsage = _random.NextDouble() * 100,
                MemoryUsage = _random.NextDouble() * 1024,
                CustomMetrics = new Dictionary<string, double>
                {
                    { "Uptime", _random.NextDouble() * 86400 },
                    { "OperationsPerSecond", _random.NextDouble() * 1000 },
                    { "ErrorRate", _random.NextDouble() * 0.1 }
                },
                Tags = new Dictionary<string, string>
                {
                    { "Environment", "Test" },
                    { "Version", "1.0.0" },
                    { "Platform", "Windows" }
                }
            };
            
            return metrics;
        }
        
        /// <summary>
        /// 创建健康检查结果
        /// </summary>
        /// <param name="isHealthy">是否健康</param>
        /// <returns>健康检查结果</returns>
        public HealthCheckResult CreateHealthCheckResult(bool isHealthy = true)
        {
            var result = new HealthCheckResult(isHealthy, isHealthy ? "All systems operational" : "Some issues detected");
            
            // 添加组件状态
            result.ComponentStatus["Keyboard"] = isHealthy;
            result.ComponentStatus["Mouse"] = isHealthy;
            result.ComponentStatus["Screen"] = isHealthy;
            result.ComponentStatus["GlobalHotkeys"] = isHealthy;
            result.ComponentStatus["Window"] = isHealthy;
            result.ComponentStatus["ImageRecognition"] = isHealthy;
            result.ComponentStatus["Initialized"] = isHealthy;
            
            return result;
        }
        
        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>随机数</returns>
        public int CreateRandomInt(int min = 0, int max = 1000)
        {
            return _random.Next(min, max);
        }
        
        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机字符串</returns>
        public string CreateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[_random.Next(chars.Length)];
            }
            
            return new string(result);
        }
    }
}