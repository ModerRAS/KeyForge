using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace KeyForge.Tests.Support;

/// <summary>
/// 测试配置管理类
/// 原本实现：复杂的配置管理
/// 简化实现：基础的配置读取
/// </summary>
public static class TestConfiguration
{
    private static readonly IConfigurationRoot Configuration;

    static TestConfiguration()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("test-config.json")
            .AddEnvironmentVariables()
            .Build();
    }

    public static string GetConnectionString()
    {
        return Configuration["DatabaseSettings:ConnectionString"] ?? "Data Source=:memory:";
    }

    public static int GetTestTimeout()
    {
        return int.Parse(Configuration["TestSettings:TestTimeout"] ?? "30000");
    }

    public static bool UseInMemoryDatabase()
    {
        return bool.Parse(Configuration["DatabaseSettings:UseInMemoryDatabase"] ?? "true");
    }

    public static int GetCoverageThreshold()
    {
        return int.Parse(Configuration["TestSettings:CoverageThreshold"] ?? "60");
    }

    public static string GetTestDataPath()
    {
        return Configuration["TestSettings:TestDataPath"] ?? "TestData/";
    }

    public static bool EnableParallelExecution()
    {
        return bool.Parse(Configuration["TestSettings:ParallelTestExecution"] ?? "true");
    }

    public static int GetMaxParallelThreads()
    {
        return int.Parse(Configuration["TestSettings:MaxParallelThreads"] ?? "4");
    }
}