using Xunit;
using FluentAssertions;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.IntegrationTests.Infrastructure;

/// <summary>
/// 数据库集成测试
/// 原本实现：完整的数据库集成测试
/// 简化实现：核心数据库功能测试
/// </summary>
public class DatabaseIntegrationTests : TestBase, IDisposable
{
    private readonly DbConnection _connection;
    private readonly TestDbContext _dbContext;
    private readonly string _testDatabaseName;

    public DatabaseIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _testDatabaseName = $"KeyForge_DbTest_{Guid.NewGuid()}";
        _connection = new SqliteConnection($"Data Source={_testDatabaseName}.db");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new TestDbContext(options);
        _dbContext.Database.EnsureCreated();
        
        Log($"数据库集成测试初始化: {_testDatabaseName}");
    }

    public void Dispose()
    {
        try
        {
            _dbContext?.Dispose();
            _connection?.Dispose();
            
            // 清理数据库文件
            var dbFile = $"{_testDatabaseName}.db";
            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
                Log($"数据库文件清理: {dbFile}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理数据库资源失败: {ex.Message}");
        }
    }

    [Fact]
    public async Task DatabaseConnection_ShouldBeEstablished()
    {
        // Act
        var canConnect = await _dbContext.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
        Log("数据库连接建立成功");
    }

    [Fact]
    public async Task DatabaseCreation_ShouldCreateAllTables()
    {
        // Act
        var tables = await GetDatabaseTables();

        // Assert
        tables.Should().Contain("Scripts");
        tables.Should().Contain("Actions");
        tables.Should().Contain("States");
        tables.Should().Contain("DecisionRules");
        Log($"数据库表创建成功: {tables.Count}个表");
    }

    [Fact]
    public async Task InsertAndRetrieveScript_ShouldWorkCorrectly()
    {
        // Arrange
        var script = new TestScript
        {
            Name = "Test Script",
            Description = "Test Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Act
        _dbContext.Scripts.Add(script);
        await _dbContext.SaveChangesAsync();

        var retrievedScript = await _dbContext.Scripts.FindAsync(script.Id);

        // Assert
        retrievedScript.Should().NotBeNull();
        retrievedScript.Name.Should().Be(script.Name);
        retrievedScript.Description.Should().Be(script.Description);
        Log("脚本插入和检索成功");
    }

    [Fact]
    public async Task UpdateScript_ShouldPersistChanges()
    {
        // Arrange
        var script = new TestScript
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _dbContext.Scripts.Add(script);
        await _dbContext.SaveChangesAsync();

        // Act
        script.Name = "Updated Name";
        script.Description = "Updated Description";
        script.UpdatedAt = DateTime.Now;
        await _dbContext.SaveChangesAsync();

        var updatedScript = await _dbContext.Scripts.FindAsync(script.Id);

        // Assert
        updatedScript.Should().NotBeNull();
        updatedScript.Name.Should().Be("Updated Name");
        updatedScript.Description.Should().Be("Updated Description");
        Log("脚本更新成功");
    }

    [Fact]
    public async Task DeleteScript_ShouldRemoveFromDatabase()
    {
        // Arrange
        var script = new TestScript
        {
            Name = "Test Script",
            Description = "Test Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _dbContext.Scripts.Add(script);
        await _dbContext.SaveChangesAsync();

        // Act
        _dbContext.Scripts.Remove(script);
        await _dbContext.SaveChangesAsync();

        var deletedScript = await _dbContext.Scripts.FindAsync(script.Id);

        // Assert
        deletedScript.Should().BeNull();
        Log("脚本删除成功");
    }

    [Fact]
    public async Task QueryScripts_ShouldReturnCorrectResults()
    {
        // Arrange
        var scripts = new[]
        {
            new TestScript { Name = "Script 1", Description = "Description 1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new TestScript { Name = "Script 2", Description = "Description 2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new TestScript { Name = "Script 3", Description = "Description 3", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };

        _dbContext.Scripts.AddRange(scripts);
        await _dbContext.SaveChangesAsync();

        // Act
        var allScripts = await _dbContext.Scripts.ToListAsync();
        var filteredScripts = await _dbContext.Scripts
            .Where(s => s.Name.Contains("Script"))
            .ToListAsync();

        // Assert
        allScripts.Should().HaveCount(3);
        filteredScripts.Should().HaveCount(3);
        Log("脚本查询成功");
    }

    [Fact]
    public async Task InsertWithRelatedEntities_ShouldWorkCorrectly()
    {
        // Arrange
        var script = new TestScript
        {
            Name = "Test Script with Actions",
            Description = "Test Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Actions = new List<TestAction>
            {
                new TestAction { Name = "Action 1", Description = "Description 1", Delay = 100 },
                new TestAction { Name = "Action 2", Description = "Description 2", Delay = 200 }
            }
        };

        // Act
        _dbContext.Scripts.Add(script);
        await _dbContext.SaveChangesAsync();

        var retrievedScript = await _dbContext.Scripts
            .Include(s => s.Actions)
            .FirstOrDefaultAsync(s => s.Id == script.Id);

        // Assert
        retrievedScript.Should().NotBeNull();
        retrievedScript.Actions.Should().HaveCount(2);
        retrievedScript.Actions.Should().Contain(a => a.Name == "Action 1");
        retrievedScript.Actions.Should().Contain(a => a.Name == "Action 2");
        Log("关联实体插入成功");
    }

    [Fact]
    public async Task Transaction_ShouldRollbackOnError()
    {
        // Arrange
        var script1 = new TestScript
        {
            Name = "Script 1",
            Description = "Description 1",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var script2 = new TestScript
        {
            Name = "Script 2",
            Description = "Description 2",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Act
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                _dbContext.Scripts.Add(script1);
                await _dbContext.SaveChangesAsync();

                // 模拟错误
                throw new Exception("Simulated error");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
            }
        }

        var scriptCount = await _dbContext.Scripts.CountAsync();

        // Assert
        scriptCount.Should().Be(0);
        Log("事务回滚成功");
    }

    [Fact]
    public async Task Transaction_ShouldCommitOnSuccess()
    {
        // Arrange
        var script = new TestScript
        {
            Name = "Test Script",
            Description = "Test Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Act
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            _dbContext.Scripts.Add(script);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        var scriptCount = await _dbContext.Scripts.CountAsync();
        var retrievedScript = await _dbContext.Scripts.FindAsync(script.Id);

        // Assert
        scriptCount.Should().Be(1);
        retrievedScript.Should().NotBeNull();
        Log("事务提交成功");
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var taskCount = 10;
        var tasks = new Task[taskCount];

        // Act
        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                var script = new TestScript
                {
                    Name = $"Concurrent Script {i}",
                    Description = $"Description {i}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbContext.Scripts.Add(script);
                await _dbContext.SaveChangesAsync();
            });
        }

        await Task.WhenAll(tasks);

        // Assert
        var scriptCount = await _dbContext.Scripts.CountAsync();
        scriptCount.Should().Be(taskCount);
        Log($"并发操作成功: {taskCount}个脚本");
    }

    [Fact]
    public async Task LargeDataset_ShouldHandleEfficiently()
    {
        // Arrange
        var scriptCount = 1000;
        var scripts = new List<TestScript>();

        for (int i = 0; i < scriptCount; i++)
        {
            scripts.Add(new TestScript
            {
                Name = $"Script {i}",
                Description = $"Description {i}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _dbContext.Scripts.AddRange(scripts);
        await _dbContext.SaveChangesAsync();
        stopwatch.Stop();

        var retrievedScripts = await _dbContext.Scripts.ToListAsync();

        // Assert
        retrievedScripts.Should().HaveCount(scriptCount);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // 应该在10秒内完成
        Log($"大数据集处理成功: {scriptCount}个脚本，耗时 {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DatabaseConstraints_ShouldBeEnforced()
    {
        // Arrange
        var script = new TestScript
        {
            Name = "Test Script",
            Description = "Test Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _dbContext.Scripts.Add(script);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var duplicateScript = new TestScript
        {
            Id = script.Id, // 尝试使用相同的ID
            Name = "Duplicate Script",
            Description = "Duplicate Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var action = async () =>
        {
            _dbContext.Scripts.Add(duplicateScript);
            await _dbContext.SaveChangesAsync();
        };

        await action.Should().ThrowAsync<DbUpdateException>();
        Log("数据库约束强制执行成功");
    }

    [Fact]
    public async Task NullValueHandling_ShouldWorkCorrectly()
    {
        // Arrange
        var script = new TestScript
        {
            Name = "Test Script",
            Description = null, // 允许为null
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Act
        _dbContext.Scripts.Add(script);
        await _dbContext.SaveChangesAsync();

        var retrievedScript = await _dbContext.Scripts.FindAsync(script.Id);

        // Assert
        retrievedScript.Should().NotBeNull();
        retrievedScript.Description.Should().BeNull();
        Log("空值处理成功");
    }

    [Fact]
    public async Task DatabaseMigration_ShouldWorkCorrectly()
    {
        // Arrange
        var initialScriptCount = await _dbContext.Scripts.CountAsync();

        // Act
        // 模拟数据库迁移
        await _dbContext.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Scripts ADD COLUMN NewColumn TEXT");

        var script = new TestScript
        {
            Name = "Test Script",
            Description = "Test Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _dbContext.Scripts.Add(script);
        await _dbContext.SaveChangesAsync();

        var finalScriptCount = await _dbContext.Scripts.CountAsync();

        // Assert
        finalScriptCount.Should().Be(initialScriptCount + 1);
        Log("数据库迁移成功");
    }

    [Fact]
    public async Task DatabaseBackupAndRestore_ShouldWorkCorrectly()
    {
        // Arrange
        var originalScript = new TestScript
        {
            Name = "Original Script",
            Description = "Original Description",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _dbContext.Scripts.Add(originalScript);
        await _dbContext.SaveChangesAsync();

        // Act
        // 创建备份
        var backupFile = $"{_testDatabaseName}_backup.db";
        File.Copy($"{_testDatabaseName}.db", backupFile);

        // 删除原始数据
        _dbContext.Scripts.Remove(originalScript);
        await _dbContext.SaveChangesAsync();

        // 恢复备份
        File.Copy(backupFile, $"{_testDatabaseName}.db", true);

        // 重新创建DbContext以测试恢复
        var restoredDbContext = new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
                .Options);

        var restoredScript = await restoredDbContext.Scripts.FindAsync(originalScript.Id);

        // Assert
        restoredScript.Should().NotBeNull();
        restoredScript.Name.Should().Be(originalScript.Name);
        Log("数据库备份和恢复成功");

        // 清理备份文件
        if (File.Exists(backupFile))
        {
            File.Delete(backupFile);
        }
    }

    [Fact]
    public async Task DatabasePerformance_ShouldBeAcceptable()
    {
        // Arrange
        var iterationCount = 100;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterationCount; i++)
        {
            var script = new TestScript
            {
                Name = $"Performance Test Script {i}",
                Description = $"Description {i}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _dbContext.Scripts.Add(script);
        }

        await _dbContext.SaveChangesAsync();
        stopwatch.Stop();

        var averageTime = stopwatch.ElapsedMilliseconds / (double)iterationCount;

        // Assert
        averageTime.Should().BeLessThan(10); // 每个操作应该小于10ms
        Log($"数据库性能可接受: 平均 {averageTime:F2}ms 每个操作");
    }

    private async Task<List<string>> GetDatabaseTables()
    {
        var tables = new List<string>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
        }
        return tables;
    }
}

/// <summary>
/// 测试数据库上下文
/// </summary>
public class TestDbContext : DbContext
{
    public DbSet<TestScript> Scripts { get; set; }
    public DbSet<TestAction> Actions { get; set; }
    public DbSet<TestState> States { get; set; }
    public DbSet<TestDecisionRule> DecisionRules { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestScript>()
            .HasMany(s => s.Actions)
            .WithOne(a => a.Script)
            .HasForeignKey(a => a.ScriptId);

        modelBuilder.Entity<TestScript>()
            .HasMany(s => s.States)
            .WithOne(s => s.Script)
            .HasForeignKey(s => s.ScriptId);

        modelBuilder.Entity<TestScript>()
            .HasMany(s => s.DecisionRules)
            .WithOne(r => r.Script)
            .HasForeignKey(r => r.ScriptId);
    }
}

/// <summary>
/// 测试脚本实体
/// </summary>
public class TestScript
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TestAction> Actions { get; set; } = new List<TestAction>();
    public List<TestState> States { get; set; } = new List<TestState>();
    public List<TestDecisionRule> DecisionRules { get; set; } = new List<TestDecisionRule>();
}

/// <summary>
/// 测试动作实体
/// </summary>
public class TestAction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public int Delay { get; set; }
    public Guid ScriptId { get; set; }
    public TestScript Script { get; set; }
}

/// <summary>
/// 测试状态实体
/// </summary>
public class TestState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid ScriptId { get; set; }
    public TestScript Script { get; set; }
}

/// <summary>
/// 测试决策规则实体
/// </summary>
public class TestDecisionRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Condition { get; set; }
    public string Action { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid ScriptId { get; set; }
    public TestScript Script { get; set; }
}