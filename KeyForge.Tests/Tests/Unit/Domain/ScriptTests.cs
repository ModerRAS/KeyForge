using Xunit;
using FluentAssertions;
using KeyForge.Domain;
using KeyForge.Domain.Entities;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Unit.Domain;

/// <summary>
/// 脚本领域模型单元测试
/// 原本实现：复杂的测试场景
/// 简化实现：核心功能测试
/// </summary>
public class ScriptTests : TestBase
{
    public ScriptTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CreateScript_WithValidData_ShouldCreateScript()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var scriptName = "Test Script";
        var scriptDescription = "Test Description";
        
        // Act
        var script = new Script(scriptId, scriptName, scriptDescription);
        
        // Assert
        script.Should().NotBeNull();
        script.Id.Should().Be(scriptId);
        script.Name.Should().Be(scriptName);
        script.Description.Should().Be(scriptDescription);
        script.Status.Should().Be(ScriptStatus.Draft);
        script.Actions.Should().BeEmpty();
        script.Version.Should().Be(1);
        Log($"创建脚本成功: {script.Id}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateScript_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var scriptDescription = "Test Description";
        
        // Act & Assert
        var action = () => new Script(scriptId, invalidName, scriptDescription);
        action.Should().Throw<ValidationException>();
        Log($"验证无效脚本名称: {invalidName}");
    }

    [Fact]
    public void AddAction_WithValidAction_ShouldAddToScript()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        
        // Act
        script.AddAction(action);
        
        // Assert
        script.Actions.Should().Contain(action);
        script.Actions.Count.Should().Be(1);
        script.Version.Should().Be(2);
        Log($"添加动作成功: {action.Type}");
    }

    [Fact]
    public void AddAction_WhenScriptNotInDraft_ShouldThrowException()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        
        // 先激活脚本
        script.AddAction(action); // 需要至少一个动作才能激活
        script.Activate();
        
        var anotherAction = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.B, 100);
        
        // Act & Assert
        var addAction = () => script.AddAction(anotherAction);
        addAction.Should().Throw<BusinessRuleViolationException>();
        Log($"验证非草稿状态脚本添加动作失败");
    }

    [Fact]
    public void ActivateScript_WhenInDraftWithActions_ShouldChangeStatusToActive()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        script.AddAction(action);
        
        // Act
        script.Activate();
        
        // Assert
        script.Status.Should().Be(ScriptStatus.Active);
        script.Version.Should().Be(3);
        Log($"激活脚本成功: {script.Id}");
    }

    [Fact]
    public void ActivateScript_WhenInDraftWithoutActions_ShouldThrowException()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        
        // Act & Assert
        var action = () => script.Activate();
        action.Should().Throw<BusinessRuleViolationException>();
        Log($"验证空脚本激活失败");
    }

    [Fact]
    public void DeactivateScript_WhenActive_ShouldChangeStatusToInactive()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        script.AddAction(action);
        script.Activate();
        
        // Act
        script.Deactivate();
        
        // Assert
        script.Status.Should().Be(ScriptStatus.Inactive);
        script.Version.Should().Be(4);
        Log($"停用脚本成功: {script.Id}");
    }

    [Fact]
    public void UpdateScript_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var newName = "Updated Script Name";
        var newDescription = "Updated Description";
        
        // Act
        script.Update(newName, newDescription);
        
        // Assert
        script.Name.Should().Be(newName);
        script.Description.Should().Be(newDescription);
        script.Version.Should().Be(2);
        Log($"更新脚本成功: {script.Id}");
    }

    [Fact]
    public void UpdateScript_WithInvalidName_ShouldThrowException()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        
        // Act & Assert
        var action = () => script.Update("", "Valid Description");
        action.Should().Throw<ValidationException>();
        Log($"验证无效脚本名称更新失败");
    }

    [Fact]
    public void GetEstimatedDuration_WithMultipleActions_ShouldReturnCorrectDuration()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100));
        script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, KeyCode.None, 200));
        script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.B, 150));
        
        // Act
        var estimatedDuration = script.GetEstimatedDuration();
        
        // Assert
        estimatedDuration.Should().Be(TimeSpan.FromMilliseconds(450));
        Log($"计算脚本预估时长: {estimatedDuration.TotalMilliseconds}ms");
    }

    [Fact]
    public void RemoveAction_WhenInDraft_ShouldRemoveAction()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        script.AddAction(action);
        
        // Act
        script.RemoveAction(action.Id);
        
        // Assert
        script.Actions.Should().BeEmpty();
        script.Version.Should().Be(3);
        Log($"移除动作成功: {action.Id}");
    }

    [Fact]
    public void RemoveAction_WhenNotInDraft_ShouldThrowException()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        script.AddAction(action);
        script.Activate();
        
        // Act & Assert
        var removeAction = () => script.RemoveAction(action.Id);
        removeAction.Should().Throw<BusinessRuleViolationException>();
        Log($"验证非草稿状态脚本移除动作失败");
    }

    [Fact]
    public void DeleteScript_WhenCreated_ShouldChangeStatusToDeleted()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        
        // Act
        script.Delete();
        
        // Assert
        script.Status.Should().Be(ScriptStatus.Deleted);
        script.Version.Should().Be(2);
        Log($"删除脚本成功: {script.Id}");
    }

    [Fact]
    public void GetActionSequence_ShouldReturnActionSequence()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var action1 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        var action2 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.B, 200);
        script.AddAction(action1);
        script.AddAction(action2);
        
        // Act
        var actionSequence = script.GetActionSequence();
        
        // Assert
        actionSequence.Should().NotBeNull();
        Log($"获取动作序列成功: {script.Id}");
    }

    private void AssertScriptIsValid(Script script)
    {
        script.Should().NotBeNull();
        script.Id.Should().NotBe(Guid.Empty);
        script.Name.Should().NotBeNullOrWhiteSpace();
        script.Status.Should().Be(ScriptStatus.Draft);
        script.Version.Should().BeGreaterThan(0);
    }
}