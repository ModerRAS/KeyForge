using KeyForge.Abstractions.Interfaces.Core;
using KeyForge.Abstractions.Interfaces.HAL;
using KeyForge.Core.Services;
using KeyForge.HAL.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace KeyForge.Core.Extensions
{
    /// <summary>
    /// KeyForge核心服务依赖注入扩展
    /// 【优化实现】实现了统一的服务注册，支持跨平台依赖注入
    /// 原实现：服务注册分散在各层，缺乏统一管理
    /// 优化：通过扩展方法，实现了统一的服务注册
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加KeyForge核心服务
        /// </summary>
        public static IServiceCollection AddKeyForgeCore(this IServiceCollection services)
        {
            // 注册HAL服务
            services.AddHALServices();
            
            // 注册核心服务
            services.AddCoreServices();
            
            return services;
        }

        /// <summary>
        /// 添加HAL服务
        /// </summary>
        private static IServiceCollection AddHALServices(this IServiceCollection services)
        {
            // 注册输入HAL
            services.AddSingleton<IKeyboardHAL, WindowsKeyboardHAL>();
            services.AddSingleton<IMouseHAL, WindowsMouseHAL>();
            
            // 可以根据平台动态注册不同的HAL实现
            // services.AddSingleton<IKeyboardHAL>(sp => 
            // {
            //     if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //         return new WindowsKeyboardHAL(sp.GetRequiredService<ILogger<WindowsKeyboardHAL>>());
            //     else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //         return new LinuxKeyboardHAL(sp.GetRequiredService<ILogger<LinuxKeyboardHAL>>());
            //     else
            //         throw new PlatformNotSupportedException("不支持的操作系统");
            // });
            
            return services;
        }

        /// <summary>
        /// 添加核心服务
        /// </summary>
        private static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // 注册输入服务
            services.AddSingleton<IKeyboardService, KeyboardService>();
            services.AddSingleton<IMouseService, MouseService>();
            
            // 可以注册其他核心服务
            // services.AddSingleton<IImageService, ImageService>();
            // services.AddSingleton<IScriptService, ScriptService>();
            // services.AddSingleton<IConfigService, ConfigService>();
            
            return services;
        }
    }
}