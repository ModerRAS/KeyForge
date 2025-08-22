using System;
using System.Threading.Tasks;
using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Core;

namespace KeyForge.CrossPlatform
{
    /// <summary>
    /// KeyForge跨平台启动器
    /// 提供简化的跨平台HAL初始化和使用接口
    /// </summary>
    public static class KeyForgeCrossPlatform
    {
        private static IHardwareAbstractionLayer _currentHAL;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// 获取当前HAL实例
        /// </summary>
        public static IHardwareAbstractionLayer CurrentHAL
        {
            get
            {
                lock (_lock)
                {
                    return _currentHAL;
                }
            }
        }
        
        /// <summary>
        /// 初始化KeyForge跨平台HAL
        /// </summary>
        /// <param name="platform">指定平台（可选，默认自动检测）</param>
        /// <param name="configuration">HAL配置（可选）</param>
        /// <returns>初始化完成的HAL实例</returns>
        public static async Task<IHardwareAbstractionLayer> InitializeAsync(Platform? platform = null, HALConfiguration? configuration = null)
        {
            lock (_lock)
            {
                if (_currentHAL != null)
                {
                    return _currentHAL;
                }
            }
            
            try
            {
                IHardwareAbstractionLayer hal;
                
                if (platform.HasValue)
                {
                    if (configuration != null)
                    {
                        hal = HALFactory.CreateHAL(configuration);
                    }
                    else
                    {
                        hal = HALFactory.CreateHAL(platform.Value);
                    }
                }
                else
                {
                    if (configuration != null)
                    {
                        hal = HALFactory.CreateHAL(configuration);
                    }
                    else
                    {
                        hal = HALFactory.CreateHAL();
                    }
                }
                
                await hal.InitializeAsync();
                
                lock (_lock)
                {
                    _currentHAL = hal;
                }
                
                return hal;
            }
            catch (Exception ex)
            {
                throw new HAL.Exceptions.HALException("Failed to initialize KeyForge cross-platform HAL", ex, HardwareOperation.Unknown, platform ?? PlatformDetector.DetectPlatform());
            }
        }
        
        /// <summary>
        /// 使用默认配置初始化KeyForge跨平台HAL
        /// </summary>
        /// <returns>初始化完成的HAL实例</returns>
        public static async Task<IHardwareAbstractionLayer> InitializeWithDefaultsAsync()
        {
            var platform = PlatformDetector.DetectPlatform();
            var configuration = HALFactory.GetDefaultConfiguration(platform);
            return await InitializeAsync(platform, configuration);
        }
        
        /// <summary>
        /// 关闭KeyForge跨平台HAL
        /// </summary>
        public static async Task ShutdownAsync()
        {
            lock (_lock)
            {
                if (_currentHAL == null)
                {
                    return;
                }
            }
            
            try
            {
                await _currentHAL.ShutdownAsync();
                
                lock (_lock)
                {
                    (_currentHAL as IDisposable)?.Dispose();
                    _currentHAL = null;
                }
            }
            catch (Exception ex)
            {
                throw new HAL.Exceptions.HALException("Failed to shutdown KeyForge cross-platform HAL", ex, HardwareOperation.Unknown, PlatformDetector.DetectPlatform());
            }
        }
        
        /// <summary>
        /// 获取平台信息
        /// </summary>
        /// <returns>平台信息</returns>
        public static PlatformInfo GetPlatformInfo()
        {
            return PlatformDetector.GetCurrentPlatformInfo();
        }
        
        /// <summary>
        /// 检查平台兼容性
        /// </summary>
        /// <param name="requiredFeatures">需要的功能特性</param>
        /// <returns>兼容性结果</returns>
        public static PlatformCompatibilityResult CheckPlatformCompatibility(PlatformFeatures requiredFeatures = PlatformFeatures.None)
        {
            var platform = PlatformDetector.DetectPlatform();
            return HALFactory.ValidatePlatformCompatibility(platform, requiredFeatures);
        }
        
        /// <summary>
        /// 创建演示实例
        /// </summary>
        /// <returns>演示实例</returns>
        public static Examples.CrossPlatformExample CreateDemo()
        {
            return new Examples.CrossPlatformExample();
        }
        
        /// <summary>
        /// 运行快速演示
        /// </summary>
        /// <returns>演示任务</returns>
        public static async Task RunQuickDemoAsync()
        {
            try
            {
                Console.WriteLine("=== KeyForge 跨平台快速演示 ===");
                
                // 初始化HAL
                var hal = await InitializeWithDefaultsAsync();
                Console.WriteLine($"已初始化HAL: {hal.PlatformInfo.Name} {hal.PlatformInfo.Version}");
                
                // 简单的键盘操作
                Console.WriteLine("演示键盘操作...");
                await hal.Keyboard.TypeTextAsync("Hello from KeyForge!");
                await Task.Delay(1000);
                
                // 简单的鼠标操作
                Console.WriteLine("演示鼠标操作...");
                var currentPos = await hal.Mouse.GetPositionAsync();
                await hal.Mouse.MoveToAsync(currentPos.X + 100, currentPos.Y + 100);
                await Task.Delay(500);
                await hal.Mouse.ClickAsync(MouseButton.Left);
                await Task.Delay(500);
                await hal.Mouse.MoveToAsync(currentPos.X, currentPos.Y);
                
                // 获取屏幕信息
                Console.WriteLine("获取屏幕信息...");
                var screens = await hal.Screen.GetScreensAsync();
                foreach (var screen in screens)
                {
                    Console.WriteLine($"  屏幕: {screen.Name} - {screen.Bounds}");
                }
                
                Console.WriteLine("演示完成！");
                
                // 关闭HAL
                await ShutdownAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"演示过程中发生错误: {ex.Message}");
                await ShutdownAsync();
            }
        }
    }
}