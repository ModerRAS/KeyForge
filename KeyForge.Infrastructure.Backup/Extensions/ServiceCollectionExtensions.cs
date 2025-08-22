using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using KeyForge.Infrastructure.Data;
using KeyForge.Domain.Services;

namespace KeyForge.Infrastructure.Extensions
{
    /// <summary>
    /// 服务集合扩展方法
    /// 
    /// 原本实现：完整的依赖注入配置，包含所有服务
    /// 简化实现：基本的服务注册
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 初始化KeyForge数据库
        /// </summary>
        public static async Task InitializeKeyForgeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KeyForgeDbContext>();
            await DatabaseInitializer.InitializeAsync(context);
        }
        
        /// <summary>
        /// 添加KeyForge基础设施服务
        /// </summary>
        public static IServiceCollection AddKeyForgeInfrastructure(this IServiceCollection services)
        {
            // 注册数据库上下文
            services.AddKeyForgeDbContext();
            
            // 注册工作流服务
            services.AddSingleton<ISenseService, SenseService>();
            services.AddSingleton<IJudgeService, JudgeService>();
            services.AddSingleton<IActService, ActService>();
            
            return services;
        }
        
        /// <summary>
        /// 添加KeyForge数据库上下文
        /// </summary>
        public static IServiceCollection AddKeyForgeDbContext(this IServiceCollection services)
        {
            // 简化实现：使用SQLite数据库
            services.AddDbContext<KeyForgeDbContext>(options =>
                options.UseSqlite("Data Source=keyforge.db"));
            
            return services;
        }
    }
}