using System.Text.Json;
using KeyForge.Domain;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.ValueObjects;

namespace KeyForge.Tests.Support;

/// <summary>
/// 测试数据工厂类
/// 原本实现：复杂的数据生成逻辑
/// 简化实现：基础的测试数据生成
/// </summary>
public static class TestFixtures
{
    private static readonly Random Random = new Random();

    public static Script CreateValidScript()
    {
        return new Script(
            Guid.NewGuid(),
            "Test Script",
            "Test Description"
        );
    }

    public static Script CreateScriptWithActions(int actionCount = 5)
    {
        var script = CreateValidScript();
        
        for (int i = 0; i < actionCount; i++)
        {
            script.AddAction(new GameAction(
                Guid.NewGuid(),
                GetRandomActionType(),
                GetRandomKeyCode(),
                Random.Next(50, 200)
            ));
        }
        
        return script;
    }

    public static Script CreateLargeScript(int actionCount = 1000)
    {
        return CreateScriptWithActions(actionCount);
    }

    public static ImageTemplate CreateValidImageTemplate()
    {
        return new ImageTemplate(
            Guid.NewGuid(),
            "test_template.png",
            new System.Drawing.Rectangle(0, 0, 100, 100),
            0.8
        );
    }

    public static List<Script> CreateMultipleScripts(int count = 3)
    {
        var scripts = new List<Script>();
        
        for (int i = 0; i < count; i++)
        {
            scripts.Add(CreateValidScript());
        }
        
        return scripts;
    }

    public static string CreateValidScriptName()
    {
        return $"TestScript_{Random.Next(1000, 9999)}";
    }

    public static string CreateInvalidScriptName()
    {
        var invalidNames = new[] { "", null, "   ", "a", new string('a', 1000) };
        return invalidNames[Random.Next(invalidNames.Length)];
    }

    public static GameAction CreateValidGameAction()
    {
        return new GameAction(
            Guid.NewGuid(),
            GetRandomActionType(),
            GetRandomKeyCode(),
            Random.Next(50, 200)
        );
    }

    public static GameAction CreateMouseAction()
    {
        return new GameAction(
            Guid.NewGuid(),
            GetRandomMouseActionType(),
            GetRandomMouseButton(),
            Random.Next(0, 1920),
            Random.Next(0, 1080),
            Random.Next(50, 200)
        );
    }

    public static GameAction CreateDelayAction()
    {
        return new GameAction(
            Guid.NewGuid(),
            ActionType.Delay,
            Random.Next(100, 1000)
        );
    }

    private static ActionType GetRandomActionType()
    {
        var actionTypes = new[] { ActionType.KeyDown, ActionType.KeyUp, ActionType.MouseMove, ActionType.MouseDown, ActionType.MouseUp, ActionType.Delay };
        return actionTypes[Random.Next(actionTypes.Length)];
    }

    private static ActionType GetRandomMouseActionType()
    {
        var actionTypes = new[] { ActionType.MouseMove, ActionType.MouseDown, ActionType.MouseUp };
        return actionTypes[Random.Next(actionTypes.Length)];
    }

    private static KeyCode GetRandomKeyCode()
    {
        var keyCodes = new[] { KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.Enter, KeyCode.Space, KeyCode.Escape };
        return keyCodes[Random.Next(keyCodes.Length)];
    }

    private static MouseButton GetRandomMouseButton()
    {
        var mouseButtons = new[] { MouseButton.Left, MouseButton.Right, MouseButton.Middle };
        return mouseButtons[Random.Next(mouseButtons.Length)];
    }
}