using Xunit;
using FluentAssertions;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using KeyForge.Domain;
using KeyForge.Infrastructure;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Integration;

/// <summary>
/// 脚本服务集成测试
/// 原本实现：复杂的集成测试场景
/// 简化实现：基础的数据库操作测试
/// </summary>
public class ScriptServiceIntegrationTests : TestBase, IDisposable
{
    private readonly DbConnection _connection;
    private readonly ScriptDbContext _dbContext;
    private readonly IScriptRepository _repository;
    private readonly ScriptService _scriptService;

    public ScriptServiceIntegrationTests(ITestOutputHelper output) : base(output)
    {
        // 创建内存数据库连接
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // 创建DbContext
        var options = new DbContextOptionsBuilder<ScriptDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new ScriptDbContext(options);
        _dbContext.Database.EnsureCreated();

        // 创建依赖
        _repository = new ScriptRepository(_dbContext);
        _scriptService = new ScriptService(_repository);

        Log("集成测试环境初始化完成");
    }

    [Fact]
    public async Task SaveScript_WithValidScript_ShouldPersistToDatabase()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();

        // Act
        await _scriptService.SaveAsync(script);
        var savedScript = await _scriptService.GetByIdAsync(script.Id);

        // Assert
        savedScript.Should().NotBeNull();
        savedScript.Name.Should().Be(script.Name);
        savedScript.Status.Should().Be(script.Status);
        Log($"脚本保存成功: {script.Id}");
    }

    [Fact]
    public async Task GetScriptById_WithExistingId_ShouldReturnScript()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        await _scriptService.SaveAsync(script);

        // Act
        var retrievedScript = await _scriptService.GetByIdAsync(script.Id);

        // Assert
        retrievedScript.Should().NotBeNull();
        retrievedScript.Id.Should().Be(script.Id);
        retrievedScript.Name.Should().Be(script.Name);
        Log($"脚本检索成功: {script.Id}");
    }

    [Fact]
    public async Task GetScriptById_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid().ToString();

        // Act
        var script = await _scriptService.GetByIdAsync(nonExistingId);

        // Assert
        script.Should().BeNull();
        Log($"不存在ID返回null: {nonExistingId}");
    }

    [Fact]
    public async Task GetAllScripts_WithMultipleScripts_ShouldReturnAllScripts()
    {
        // Arrange
        var scripts = TestFixtures.CreateMultipleScripts(3);
        foreach (var script in scripts)
        {
            await _scriptService.SaveAsync(script);
        }

        // Act
        var allScripts = await _scriptService.GetAllAsync();

        // Assert
        allScripts.Should().HaveCount(3);
        allScripts.Should().Contain(s => s.Id == scripts[0].Id);
        allScripts.Should().Contain(s => s.Id == scripts[1].Id);
        allScripts.Should().Contain(s => s.Id == scripts[2].Id);
        Log($"获取所有脚本成功: {allScripts.Count}个");
    }

    [Fact]
    public async Task UpdateScript_WithValidData_ShouldUpdateInDatabase()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        await _scriptService.SaveAsync(script);

        // Act
        script.Activate();
        await _scriptService.UpdateAsync(script);
        var updatedScript = await _scriptService.GetByIdAsync(script.Id);

        // Assert
        updatedScript.Should().NotBeNull();
        updatedScript.Status.Should().Be(ScriptStatus.Active);
        Log($"脚本更新成功: {script.Id}");
    }

    [Fact]
    public async Task DeleteScript_WithExistingId_ShouldRemoveFromDatabase()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        await _scriptService.SaveAsync(script);

        // Act
        await _scriptService.DeleteAsync(script.Id);
        var deletedScript = await _scriptService.GetByIdAsync(script.Id);

        // Assert
        deletedScript.Should().BeNull();
        Log($"脚本删除成功: {script.Id}");
    }

    [Fact]
    public async Task SaveScript_WithDuplicateId_ShouldThrowException()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        await _scriptService.SaveAsync(script);

        // Act & Assert
        var action = async () => await _scriptService.SaveAsync(script);
        await action.Should().ThrowAsync<Exception>();
        LogError("重复ID异常处理成功");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
        Log("集成测试环境清理完成");
    }
}