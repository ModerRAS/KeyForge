using System;
using System.Threading.Tasks;
using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;

namespace KeyForge.HAL.Core
{
    /// <summary>
    /// HAL工厂类
    /// </summary>
    public static class HALFactory
    {
        /// <summary>
        /// 创建当前平台的HAL实例
        /// </summary>
        /// <returns>HAL实例</returns>
        public static IHardwareAbstractionLayer CreateHAL()
        {
            var platform = PlatformDetector.DetectPlatform();
            return CreateHAL(platform);
        }
        
        /// <summary>
        /// 创建指定平台的HAL实例
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <returns>HAL实例</returns>
        public static IHardwareAbstractionLayer CreateHAL(Platform platform)
        {
            if (!PlatformDetector.IsPlatformSupported(platform))
            {
                throw new PlatformNotSupportedException(
                    platform, 
                    $"Platform {platform} is not supported");
            }
            
            return platform switch
            {
                Platform.Windows => new Implementation.Windows.WindowsHAL(),
                Platform.MacOS => new Implementation.MacOS.MacOSHAL(),
                Platform.Linux => new Implementation.Linux.LinuxHAL(),
                Platform.Unknown => throw new PlatformNotSupportedException(
                    Platform.Unknown, 
                    "Unknown platform"),
                _ => throw new PlatformNotSupportedException(
                    platform, 
                    $"Platform {platform} is not supported")
            };
        }
        
        /// <summary>
        /// 异步创建并初始化HAL实例
        /// </summary>
        /// <returns>初始化完成的HAL实例</returns>
        public static async Task<IHardwareAbstractionLayer> CreateAndInitializeHALAsync()
        {
            var hal = CreateHAL();
            await hal.InitializeAsync();
            return hal;
        }
        
        /// <summary>
        /// 异步创建并初始化指定平台的HAL实例
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <returns>初始化完成的HAL实例</returns>
        public static async Task<IHardwareAbstractionLayer> CreateAndInitializeHALAsync(Platform platform)
        {
            var hal = CreateHAL(platform);
            await hal.InitializeAsync();
            return hal;
        }
        
        /// <summary>
        /// 创建HAL实例（带配置）
        /// </summary>
        /// <param name="configuration">HAL配置</param>
        /// <returns>HAL实例</returns>
        public static IHardwareAbstractionLayer CreateHAL(HALConfiguration configuration)
        {
            var hal = CreateHAL(configuration.Platform);
            
            // 应用配置
            if (configuration != null)
            {
                ApplyConfiguration(hal, configuration);
            }
            
            return hal;
        }
        
        /// <summary>
        /// 异步创建并初始化HAL实例（带配置）
        /// </summary>
        /// <param name="configuration">HAL配置</param>
        /// <returns>初始化完成的HAL实例</returns>
        public static async Task<IHardwareAbstractionLayer> CreateAndInitializeHALAsync(HALConfiguration configuration)
        {
            var hal = CreateHAL(configuration);
            
            // 应用配置
            if (configuration != null)
            {
                ApplyConfiguration(hal, configuration);
            }
            
            await hal.InitializeAsync();
            return hal;
        }
        
        /// <summary>
        /// 应用配置到HAL实例
        /// </summary>
        /// <param name="hal">HAL实例</param>
        /// <param name="configuration">HAL配置</param>
        private static void ApplyConfiguration(IHardwareAbstractionLayer hal, HALConfiguration configuration)
        {
            // 应用键盘配置
            if (configuration.KeyboardOptions != null)
            {
                // 这里可以设置键盘相关的配置
                // hal.Keyboard.SetOptions(configuration.KeyboardOptions);
            }
            
            // 应用鼠标配置
            if (configuration.MouseOptions != null)
            {
                // 这里可以设置鼠标相关的配置
                // hal.Mouse.SetOptions(configuration.MouseOptions);
            }
            
            // 应用屏幕配置
            if (configuration.ScreenOptions != null)
            {
                // 这里可以设置屏幕相关的配置
                // hal.Screen.SetOptions(configuration.ScreenOptions);
            }
            
            // 应用热键配置
            if (configuration.HotkeyOptions != null)
            {
                hal.GlobalHotkeys.SetHotkeyOptions(configuration.HotkeyOptions).Wait();
            }
            
            // 应用窗口配置
            if (configuration.WindowOptions != null)
            {
                // 这里可以设置窗口相关的配置
                // hal.Window.SetOptions(configuration.WindowOptions);
            }
            
            // 应用图像识别配置
            if (configuration.ImageRecognitionOptions != null)
            {
                // 这里可以设置图像识别相关的配置
                // hal.ImageRecognition.SetOptions(configuration.ImageRecognitionOptions);
            }
        }
        
        /// <summary>
        /// 验证平台兼容性
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <param name="requiredFeatures">需要的功能特性</param>
        /// <returns>验证结果</returns>
        public static PlatformCompatibilityResult ValidatePlatformCompatibility(Platform platform, PlatformFeatures requiredFeatures)
        {
            var result = new PlatformCompatibilityResult
            {
                Platform = platform,
                IsCompatible = true,
                SupportedFeatures = PlatformFeatures.None,
                MissingFeatures = PlatformFeatures.None
            };
            
            if (!PlatformDetector.IsPlatformSupported(platform))
            {
                result.IsCompatible = false;
                result.ErrorMessage = $"Platform {platform} is not supported";
                return result;
            }
            
            var platformInfo = PlatformDetector.GetPlatformInfo(platform);
            var capabilities = platformInfo.Capabilities;
            
            // 检查全局热键支持
            if (requiredFeatures.HasFlag(PlatformFeatures.GlobalHotkeys) && capabilities.SupportsGlobalHotkeys)
            {
                result.SupportedFeatures |= PlatformFeatures.GlobalHotkeys;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.GlobalHotkeys))
            {
                result.MissingFeatures |= PlatformFeatures.GlobalHotkeys;
            }
            
            // 检查键盘钩子支持
            if (requiredFeatures.HasFlag(PlatformFeatures.KeyboardHooks) && capabilities.SupportsLowLevelKeyboardHook)
            {
                result.SupportedFeatures |= PlatformFeatures.KeyboardHooks;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.KeyboardHooks))
            {
                result.MissingFeatures |= PlatformFeatures.KeyboardHooks;
            }
            
            // 检查鼠标钩子支持
            if (requiredFeatures.HasFlag(PlatformFeatures.MouseHooks) && capabilities.SupportsLowLevelMouseHook)
            {
                result.SupportedFeatures |= PlatformFeatures.MouseHooks;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.MouseHooks))
            {
                result.MissingFeatures |= PlatformFeatures.MouseHooks;
            }
            
            // 检查多显示器支持
            if (requiredFeatures.HasFlag(PlatformFeatures.MultipleDisplays) && capabilities.SupportsMultipleDisplays)
            {
                result.SupportedFeatures |= PlatformFeatures.MultipleDisplays;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.MultipleDisplays))
            {
                result.MissingFeatures |= PlatformFeatures.MultipleDisplays;
            }
            
            // 检查窗口透明度支持
            if (requiredFeatures.HasFlag(PlatformFeatures.WindowTransparency) && capabilities.SupportsWindowTransparency)
            {
                result.SupportedFeatures |= PlatformFeatures.WindowTransparency;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.WindowTransparency))
            {
                result.MissingFeatures |= PlatformFeatures.WindowTransparency;
            }
            
            // 检查窗口置顶支持
            if (requiredFeatures.HasFlag(PlatformFeatures.WindowTopmost) && capabilities.SupportsWindowTopmost)
            {
                result.SupportedFeatures |= PlatformFeatures.WindowTopmost;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.WindowTopmost))
            {
                result.MissingFeatures |= PlatformFeatures.WindowTopmost;
            }
            
            // 检查屏幕录制支持
            if (requiredFeatures.HasFlag(PlatformFeatures.ScreenRecording) && capabilities.SupportsScreenRecording)
            {
                result.SupportedFeatures |= PlatformFeatures.ScreenRecording;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.ScreenRecording))
            {
                result.MissingFeatures |= PlatformFeatures.ScreenRecording;
            }
            
            // 检查辅助功能支持
            if (requiredFeatures.HasFlag(PlatformFeatures.Accessibility) && capabilities.SupportsAccessibility)
            {
                result.SupportedFeatures |= PlatformFeatures.Accessibility;
            }
            else if (requiredFeatures.HasFlag(PlatformFeatures.Accessibility))
            {
                result.MissingFeatures |= PlatformFeatures.Accessibility;
            }
            
            // 如果有缺失的功能，标记为不兼容
            if (result.MissingFeatures != PlatformFeatures.None)
            {
                result.IsCompatible = false;
                result.ErrorMessage = $"Platform {platform} is missing required features: {result.MissingFeatures}";
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取平台特定的默认配置
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <returns>默认配置</returns>
        public static HALConfiguration GetDefaultConfiguration(Platform platform)
        {
            return platform switch
            {
                Platform.Windows => new HALConfiguration
                {
                    Platform = platform,
                    KeyboardOptions = new KeyboardOptions
                    {
                        EnableHooks = true,
                        EnableSimulation = true,
                        KeyDelay = 10,
                        EnableUnicode = true
                    },
                    MouseOptions = new MouseOptions
                    {
                        EnableHooks = true,
                        EnableSmoothMovement = true,
                        EnableDragging = true,
                        MouseSpeed = 1.0
                    },
                    HotkeyOptions = new HotkeyOptions
                    {
                        EnableConflictDetection = true,
                        EnableRepeatDetection = true,
                        EnableSystemHotkeys = true
                    }
                },
                Platform.MacOS => new HALConfiguration
                {
                    Platform = platform,
                    KeyboardOptions = new KeyboardOptions
                    {
                        EnableHooks = true,
                        EnableSimulation = true,
                        KeyDelay = 15,
                        EnableUnicode = true
                    },
                    MouseOptions = new MouseOptions
                    {
                        EnableHooks = true,
                        EnableSmoothMovement = true,
                        EnableDragging = true,
                        MouseSpeed = 1.0
                    },
                    HotkeyOptions = new HotkeyOptions
                    {
                        EnableConflictDetection = true,
                        EnableRepeatDetection = true,
                        EnableSystemHotkeys = true
                    }
                },
                Platform.Linux => new HALConfiguration
                {
                    Platform = platform,
                    KeyboardOptions = new KeyboardOptions
                    {
                        EnableHooks = true,
                        EnableSimulation = true,
                        KeyDelay = 20,
                        EnableUnicode = true
                    },
                    MouseOptions = new MouseOptions
                    {
                        EnableHooks = true,
                        EnableSmoothMovement = true,
                        EnableDragging = true,
                        MouseSpeed = 1.0
                    },
                    HotkeyOptions = new HotkeyOptions
                    {
                        EnableConflictDetection = true,
                        EnableRepeatDetection = true,
                        EnableSystemHotkeys = true
                    }
                },
                _ => throw new PlatformNotSupportedException(
                    platform, 
                    $"Platform {platform} is not supported")
            };
        }
    }
}