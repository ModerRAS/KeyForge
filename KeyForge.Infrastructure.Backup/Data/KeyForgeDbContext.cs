using Microsoft.EntityFrameworkCore;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;

namespace KeyForge.Infrastructure.Data
{
    /// <summary>
    /// KeyForge数据库上下文
    /// 
    /// 原本实现：完整的数据库设计，包含所有实体关系和索引
    /// 简化实现：基本的SQLite数据库，核心表结构
    /// </summary>
    public class KeyForgeDbContext : DbContext
    {
        public KeyForgeDbContext(DbContextOptions<KeyForgeDbContext> options) : base(options)
        {
        }

        // 脚本相关表
        public DbSet<Script> Scripts { get; set; }
        public DbSet<GameAction> GameActions { get; set; }

        // 图像识别相关表
        public DbSet<ImageTemplate> ImageTemplates { get; set; }

        // 决策相关表
        public DbSet<DecisionRule> DecisionRules { get; set; }
        public DbSet<StateMachine> StateMachines { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 简化实现：基本配置，忽略复杂属性
            // 这里会使用EF Core的默认约定
        }
    }

    /// <summary>
    /// 数据库配置选项
    /// </summary>
    public class DatabaseOptions
    {
        public string ConnectionString { get; set; } = "Data Source=keyforge.db";
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public bool EnableDetailedErrors { get; set; } = true;
        public int CommandTimeout { get; set; } = 30;
    }

    /// <summary>
    /// 数据库初始化器
    /// </summary>
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(KeyForgeDbContext context)
        {
            try
            {
                // 创建数据库
                await context.Database.EnsureCreatedAsync();

                // 应用迁移（如果有）
                await context.Database.MigrateAsync();

                // 初始化基础数据
                await SeedInitialDataAsync(context);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"数据库初始化失败: {ex.Message}", ex);
            }
        }

        private static async Task SeedInitialDataAsync(KeyForgeDbContext context)
        {
            // 简化实现：暂时不添加示例数据
            // 这里会根据实际需要添加种子数据
        }
    }
}