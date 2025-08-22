using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL;

/// <summary>
/// HAL工厂 - 创建和配置硬件抽象层实例
/// </summary>
public static class HALFactory
{
    /// <summary>
    /// 创建硬件抽象层实例
    /// </summary>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="platform">目标平台（可选，默认自动检测）</param>
    /// <returns>硬件抽象层实例</returns>
    public static IHardwareAbstractionLayer CreateHAL(
        ILogger<HardwareAbstractionLayer> logger,
        IServiceProvider serviceProvider,
        Platform? platform = null)
    {
        var targetPlatform = platform ?? PlatformDetector.DetectPlatform().Platform;

        logger.LogInformation("Creating HAL for platform: {Platform}", targetPlatform);

        // 创建各个服务实例
        var keyboardService = CreateKeyboardService(targetPlatform, logger, serviceProvider);
        var mouseService = CreateMouseService(targetPlatform, logger, serviceProvider);
        var screenService = CreateScreenService(targetPlatform, logger, serviceProvider);
        var globalHotkeyService = CreateGlobalHotkeyService(targetPlatform, logger, serviceProvider);
        var windowService = CreateWindowService(targetPlatform, logger, serviceProvider);
        var imageRecognitionService = CreateImageRecognitionService(targetPlatform, logger, serviceProvider);
        var performanceMonitor = CreatePerformanceMonitor(targetPlatform, logger, serviceProvider);

        // 创建HAL实例
        var hal = new HardwareAbstractionLayer(
            logger,
            serviceProvider,
            keyboardService,
            mouseService,
            screenService,
            globalHotkeyService,
            windowService,
            imageRecognitionService,
            performanceMonitor);

        return hal;
    }

    /// <summary>
    /// 创建键盘服务
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>键盘服务实例</returns>
    private static IKeyboardService CreateKeyboardService(
        Platform platform,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        return platform switch
        {
            Platform.Windows => new Services.Windows.WindowsKeyboardService(logger),
            Platform.MacOS => new Services.MacOS.MacOSKeyboardService(logger),
            Platform.Linux => new Services.Linux.LinuxKeyboardService(logger),
            _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// 创建鼠标服务
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>鼠标服务实例</returns>
    private static IMouseService CreateMouseService(
        Platform platform,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        return platform switch
        {
            Platform.Windows => new Services.Windows.WindowsMouseService(logger),
            Platform.MacOS => new Services.MacOS.MacOSMouseService(logger),
            Platform.Linux => new Services.Linux.LinuxMouseService(logger),
            _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// 创建屏幕服务
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>屏幕服务实例</returns>
    private static IScreenService CreateScreenService(
        Platform platform,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        return platform switch
        {
            Platform.Windows => new Services.Windows.WindowsScreenService(logger),
            Platform.MacOS => new Services.MacOS.MacOSScreenService(logger),
            Platform.Linux => new Services.Linux.LinuxScreenService(logger),
            _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// 创建全局热键服务
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>全局热键服务实例</returns>
    private static IGlobalHotkeyService CreateGlobalHotkeyService(
        Platform platform,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        return platform switch
        {
            Platform.Windows => new Services.Windows.WindowsGlobalHotkeyService(logger),
            Platform.MacOS => new Services.MacOS.MacOSGlobalHotkeyService(logger),
            Platform.Linux => new Services.Linux.LinuxGlobalHotkeyService(logger),
            _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// 创建窗口服务
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>窗口服务实例</returns>
    private static IWindowService CreateWindowService(
        Platform platform,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        return platform switch
        {
            Platform.Windows => new Services.Windows.WindowsWindowService(logger),
            Platform.MacOS => new Services.MacOS.MacOSWindowService(logger),
            Platform.Linux => new Services.Linux.LinuxWindowService(logger),
            _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// 创建图像识别服务
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>图像识别服务实例</returns>
    private static IImageRecognitionService CreateImageRecognitionService(
        Platform platform,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        return platform switch
        {
            Platform.Windows => new Services.Windows.WindowsImageRecognitionService(logger),
            Platform.MacOS => new Services.MacOS.MacOSImageRecognitionService(logger),
            Platform.Linux => new Services.Linux.LinuxImageRecognitionService(logger),
            _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// 创建性能监控服务
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>性能监控服务实例</returns>
    private static IPerformanceMonitor CreatePerformanceMonitor(
        Platform platform,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        return platform switch
        {
            Platform.Windows => new Services.Windows.WindowsPerformanceMonitor(logger),
            Platform.MacOS => new Services.MacOS.MacOSPerformanceMonitor(logger),
            Platform.Linux => new Services.Linux.LinuxPerformanceMonitor(logger),
            _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
        };
    }

    /// <summary>
    /// 配置依赖注入服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>配置后的服务集合</returns>
    public static IServiceCollection ConfigureHALServices(this IServiceCollection services)
    {
        // 注册HAL核心服务
        services.AddSingleton<IHardwareAbstractionLayer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<HardwareAbstractionLayer>>();
            return CreateHAL(logger, sp);
        });

        // 注册各个服务接口
        services.AddSingleton<IKeyboardService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Services.Windows.WindowsKeyboardService>>();
            var platform = PlatformDetector.DetectPlatform().Platform;
            return CreateKeyboardService(platform, logger, sp);
        });

        services.AddSingleton<IMouseService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Services.Windows.WindowsMouseService>>();
            var platform = PlatformDetector.DetectPlatform().Platform;
            return CreateMouseService(platform, logger, sp);
        });

        services.AddSingleton<IScreenService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Services.Windows.WindowsScreenService>>();
            var platform = PlatformDetector.DetectPlatform().Platform;
            return CreateScreenService(platform, logger, sp);
        });

        services.AddSingleton<IGlobalHotkeyService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Services.Windows.WindowsGlobalHotkeyService>>();
            var platform = PlatformDetector.DetectPlatform().Platform;
            return CreateGlobalHotkeyService(platform, logger, sp);
        });

        services.AddSingleton<IWindowService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Services.Windows.WindowsWindowService>>();
            var platform = PlatformDetector.DetectPlatform().Platform;
            return CreateWindowService(platform, logger, sp);
        });

        services.AddSingleton<IImageRecognitionService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Services.Windows.WindowsImageRecognitionService>>();
            var platform = PlatformDetector.DetectPlatform().Platform;
            return CreateImageRecognitionService(platform, logger, sp);
        });

        services.AddSingleton<IPerformanceMonitor>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Services.Windows.WindowsPerformanceMonitor>>();
            var platform = PlatformDetector.DetectPlatform().Platform;
            return CreatePerformanceMonitor(platform, logger, sp);
        });

        return services;
    }
}