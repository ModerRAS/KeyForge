using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using KeyForge.Tests.Support;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;

namespace KeyForge.Tests.BDD;

/// <summary>
/// BDD测试基类
/// 原本实现：复杂的BDD框架集成
/// 简化实现：基础的BDD测试结构
/// </summary>
public abstract class BddTestBase : TestBase
{
    protected readonly string _testDirectory;

    protected BddTestBase(ITestOutputHelper output) : base(output)
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_BDD_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        Log($"创建BDD测试目录: {_testDirectory}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                    Log($"清理BDD测试目录: {_testDirectory}");
                }
            }
            catch (Exception ex)
            {
                Log($"清理BDD测试目录失败: {ex.Message}");
            }
        }
        base.Dispose(disposing);
    }

    protected void Given(string description, Action action)
    {
        Log($"GIVEN: {description}");
        action();
        Log($"✓ {description}");
    }

    protected void When(string description, Action action)
    {
        Log($"WHEN: {description}");
        action();
        Log($"✓ {description}");
    }

    protected void Then(string description, Action action)
    {
        Log($"THEN: {description}");
        action();
        Log($"✓ {description}");
    }

    protected void Then(string description, Func<bool> condition)
    {
        Log($"THEN: {description}");
        var result = condition();
        result.Should().BeTrue();
        Log($"✓ {description}");
    }

    protected void And(string description, Action action)
    {
        Log($"AND: {description}");
        action();
        Log($"✓ {description}");
    }

    protected void And(string description, Func<bool> condition)
    {
        Log($"AND: {description}");
        var result = condition();
        result.Should().BeTrue();
        Log($"✓ {description}");
    }

    protected Script CreateTestScript()
    {
        return TestFixtures.CreateValidScript();
    }

    protected Script CreateScriptWithActions(int actionCount = 5)
    {
        return TestFixtures.CreateScriptWithActions(actionCount);
    }

    protected GameAction CreateKeyboardAction()
    {
        return TestFixtures.CreateValidGameAction();
    }

    protected GameAction CreateMouseAction()
    {
        return TestFixtures.CreateMouseAction();
    }

    protected GameAction CreateDelayAction()
    {
        return TestFixtures.CreateDelayAction();
    }
}