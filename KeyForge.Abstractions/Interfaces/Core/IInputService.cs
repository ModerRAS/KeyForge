using KeyForge.Abstractions.Models.Input;
using KeyForge.Abstractions.Models.Core;
using KeyForge.Abstractions.Models.Application;

namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 输入服务基础接口
    /// 【优化实现】统一了输入系统的抽象接口，支持跨平台部署
    /// 原实现：各平台直接实现具体的输入服务，接口不统一
    /// 优化：定义统一的输入服务接口，所有平台实现都遵循此接口
    /// </summary>
    public interface IInputService : IDisposable
    {
        /// <summary>
        /// 初始化输入服务
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 启动输入监听
        /// </summary>
        Task StartAsync();
        
        /// <summary>
        /// 停止输入监听
        /// </summary>
        Task StopAsync();
        
        /// <summary>
        /// 输入事件触发
        /// </summary>
        event EventHandler<InputEventArgs> OnInput;
        
        /// <summary>
        /// 服务状态
        /// </summary>
        ServiceStatus Status { get; }
    }
    
    /// <summary>
    /// 键盘输入服务接口
    /// </summary>
    public interface IKeyboardService : IInputService
    {
        /// <summary>
        /// 发送键盘按键
        /// </summary>
        Task<bool> SendKeyAsync(KeyCode keyCode, KeyState state);
        
        /// <summary>
        /// 发送文本输入
        /// </summary>
        Task<bool> SendTextAsync(string text);
        
        /// <summary>
        /// 按下键盘按键
        /// </summary>
        Task<bool> KeyDownAsync(KeyCode keyCode);
        
        /// <summary>
        /// 释放键盘按键
        /// </summary>
        Task<bool> KeyUpAsync(KeyCode keyCode);
    }
    
    /// <summary>
    /// 鼠标输入服务接口
    /// </summary>
    public interface IMouseService : IInputService
    {
        /// <summary>
        /// 移动鼠标到指定位置
        /// </summary>
        Task<bool> MoveToAsync(int x, int y);
        
        /// <summary>
        /// 点击鼠标按钮
        /// </summary>
        Task<bool> ClickAsync(MouseButton button, ClickType clickType = ClickType.Single);
        
        /// <summary>
        /// 按下鼠标按钮
        /// </summary>
        Task<bool> MouseDownAsync(MouseButton button);
        
        /// <summary>
        /// 释放鼠标按钮
        /// </summary>
        Task<bool> MouseUpAsync(MouseButton button);
        
        /// <summary>
        /// 滚动鼠标滚轮
        /// </summary>
        Task<bool> ScrollAsync(int delta);
        
        /// <summary>
        /// 获取当前鼠标位置
        /// </summary>
        Task<(int X, int Y)> GetPositionAsync();
    }
    
    /// <summary>
    /// 热键服务接口
    /// </summary>
    public interface IHotkeyService : IInputService
    {
        /// <summary>
        /// 注册热键
        /// </summary>
        Task<bool> RegisterHotkeyAsync(int id, KeyCode keyCode, KeyModifiers modifiers);
        
        /// <summary>
        /// 注销热键
        /// </summary>
        Task<bool> UnregisterHotkeyAsync(int id);
        
        /// <summary>
        /// 热键触发事件
        /// </summary>
        event EventHandler<HotkeyEventArgs> OnHotkey;
    }
}