namespace KeyForge.Abstractions.Interfaces.HAL
{
    /// <summary>
    /// 输入硬件抽象层基础接口
    /// 【优化实现】定义了跨平台输入系统的硬件抽象层接口
    /// 原实现：各平台直接调用系统API，缺乏统一抽象
    /// 优化：通过HAL抽象，实现跨平台输入系统的统一接口
    /// </summary>
    public interface IInputHAL : IDisposable
    {
        /// <summary>
        /// 初始化输入HAL
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 获取HAL类型
        /// </summary>
        HALType HALType { get; }
        
        /// <summary>
        /// 获取HAL版本
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// HAL状态
        /// </summary>
        HALStatus Status { get; }
    }
    
    /// <summary>
    /// 键盘硬件抽象层接口
    /// </summary>
    public interface IKeyboardHAL : IInputHAL
    {
        /// <summary>
        /// 发送键盘事件
        /// </summary>
        Task<bool> SendKeyEventAsync(int keyCode, bool isKeyDown);
        
        /// <summary>
        /// 发送文本输入
        /// </summary>
        Task<bool> SendTextAsync(string text);
        
        /// <summary>
        /// 获取键盘状态
        /// </summary>
        Task<bool> GetKeyStateAsync(int keyCode);
        
        /// <summary>
        /// 设置键盘钩子
        /// </summary>
        Task<bool> SetKeyboardHookAsync(KeyboardHookCallback callback);
        
        /// <summary>
        /// 移除键盘钩子
        /// </summary>
        Task<bool> RemoveKeyboardHookAsync();
    }
    
    /// <summary>
    /// 鼠标硬件抽象层接口
    /// </summary>
    public interface IMouseHAL : IInputHAL
    {
        /// <summary>
        /// 移动鼠标
        /// </summary>
        Task<bool> MoveMouseAsync(int x, int y);
        
        /// <summary>
        /// 发送鼠标事件
        /// </summary>
        Task<bool> SendMouseEventAsync(int x, int y, int mouseButton, bool isMouseDown);
        
        /// <summary>
        /// 发送鼠标滚轮事件
        /// </summary>
        Task<bool> SendMouseWheelEventAsync(int delta);
        
        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        Task<(int X, int Y)> GetMousePositionAsync();
        
        /// <summary>
        /// 设置鼠标钩子
        /// </summary>
        Task<bool> SetMouseHookAsync(MouseHookCallback callback);
        
        /// <summary>
        /// 移除鼠标钩子
        /// </summary>
        Task<bool> RemoveMouseHookAsync();
    }
    
    /// <summary>
    /// 热键硬件抽象层接口
    /// </summary>
    public interface IHotkeyHAL : IInputHAL
    {
        /// <summary>
        /// 注册热键
        /// </summary>
        Task<bool> RegisterHotkeyAsync(int id, int keyCode, int modifiers);
        
        /// <summary>
        /// 注销热键
        /// </summary>
        Task<bool> UnregisterHotkeyAsync(int id);
        
        /// <summary>
        /// 设置热键钩子
        /// </summary>
        Task<bool> SetHotkeyHookAsync(HotkeyHookCallback callback);
        
        /// <summary>
        /// 移除热键钩子
        /// </summary>
        Task<bool> RemoveHotkeyHookAsync();
    }
    
    /// <summary>
    /// 键盘钩子回调委托
    /// </summary>
    public delegate void KeyboardHookCallback(int keyCode, bool isKeyDown);
    
    /// <summary>
    /// 鼠标钩子回调委托
    /// </summary>
    public delegate void MouseHookCallback(int x, int y, int mouseButton, bool isMouseDown);
    
    /// <summary>
    /// 热键钩子回调委托
    /// </summary>
    public delegate void HotkeyHookCallback(int id, int keyCode, int modifiers);
}