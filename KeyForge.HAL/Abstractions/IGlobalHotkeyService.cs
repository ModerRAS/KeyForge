namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 全局热键服务接口 - 提供跨平台全局热键功能
/// </summary>
public interface IGlobalHotkeyService
{
    /// <summary>
    /// 注册全局热键
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <param name="callback">回调函数</param>
    /// <returns>是否成功</returns>
    Task<bool> RegisterHotkeyAsync(int id, KeyCode[] modifiers, KeyCode key, Action<HotkeyEventArgs> callback);

    /// <summary>
    /// 注销全局热键
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <returns>是否成功</returns>
    Task<bool> UnregisterHotkeyAsync(int id);

    /// <summary>
    /// 检查热键是否已注册
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <returns>是否已注册</returns>
    bool IsHotkeyRegistered(int id);

    /// <summary>
    /// 获取所有已注册的热键
    /// </summary>
    /// <returns>热键列表</returns>
    IEnumerable<HotkeyInfo> GetRegisteredHotkeys();

    /// <summary>
    /// 检查热键组合是否可用
    /// </summary>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <returns>是否可用</returns>
    bool IsHotkeyAvailable(KeyCode[] modifiers, KeyCode key);

    /// <summary>
    /// 暂停所有热键
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> SuspendAllHotkeysAsync();

    /// <summary>
    /// 恢复所有热键
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> ResumeAllHotkeysAsync();

    /// <summary>
    /// 热键事件
    /// </summary>
    event EventHandler<HotkeyEventArgs>? HotkeyPressed;
}