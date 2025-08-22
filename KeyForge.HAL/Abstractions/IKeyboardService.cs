namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 键盘服务接口 - 提供跨平台键盘输入功能
/// </summary>
public interface IKeyboardService
{
    /// <summary>
    /// 模拟按键按下
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否成功</returns>
    Task<bool> KeyDownAsync(KeyCode key);

    /// <summary>
    /// 模拟按键释放
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否成功</returns>
    Task<bool> KeyUpAsync(KeyCode key);

    /// <summary>
    /// 模拟按键点击（按下+释放）
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否成功</returns>
    Task<bool> KeyPressAsync(KeyCode key);

    /// <summary>
    /// 模拟文本输入
    /// </summary>
    /// <param name="text">要输入的文本</param>
    /// <param name="delay">字符间延迟（毫秒）</param>
    /// <returns>是否成功</returns>
    Task<bool> TypeTextAsync(string text, int delay = 50);

    /// <summary>
    /// 模拟组合键
    /// </summary>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <returns>是否成功</returns>
    Task<bool> SendHotkeyAsync(KeyCode[] modifiers, KeyCode key);

    /// <summary>
    /// 获取当前按键状态
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>按键状态</returns>
    KeyState GetKeyState(KeyCode key);

    /// <summary>
    /// 检查按键是否可用
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否可用</returns>
    bool IsKeyAvailable(KeyCode key);

    /// <summary>
    /// 键盘事件
    /// </summary>
    event EventHandler<KeyboardEventArgs>? KeyEvent;
}