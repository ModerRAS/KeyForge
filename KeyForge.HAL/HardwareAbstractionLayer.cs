using System;
using System.Threading.Tasks;
using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL;

/// <summary>
/// 硬件抽象层默认实现（简化版）
/// 原本实现：包含完整的质量保证和性能监控功能
/// 简化实现：只保留基本的接口实现，确保项目能够编译
/// </summary>
public class HardwareAbstractionLayer : IHardwareAbstractionLayer, IDisposable
{
    private bool _isInitialized = false;
    private readonly object _lock = new();
    
    // 基本服务
    private readonly IKeyboardService _keyboardService;
    private readonly IMouseService _mouseService;
    private readonly IScreenService _screenService;
    
    /// <summary>
    /// 初始化硬件抽象层（简化版）
    /// </summary>
    public HardwareAbstractionLayer()
    {
        _keyboardService = new Services.Windows.WindowsKeyboardService();
        _mouseService = new Services.Windows.WindowsMouseService();
        _screenService = new Services.Windows.WindowsScreenService();
    }
    
    // 服务属性
    public IKeyboardService Keyboard => _keyboardService;
    public IMouseService Mouse => _mouseService;
    public IScreenService Screen => _screenService;
    
    // 其他服务暂时返回null
    public IGlobalHotkeyService GlobalHotkey => null;
    public IWindowService Window => null;
    public IImageRecognitionService ImageRecognition => null;
    public IPerformanceMonitor PerformanceMonitor => null;
    public IQualityGateService QualityGate => null;
    public IDiagnosticsService Diagnostics => null;
    
    /// <summary>
    /// 初始化HAL
    /// </summary>
    public async Task InitializeAsync()
    {
        lock (_lock)
        {
            if (_isInitialized)
                return;
            
            _isInitialized = true;
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 获取HAL状态
    /// </summary>
    public HALStatus GetStatus()
    {
        return new HALStatus
        {
            IsInitialized = _isInitialized,
            Platform = Platform.Windows,
            LastUpdateTime = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_isInitialized)
            {
                if (_keyboardService is IDisposable disposableKeyboard)
                    disposableKeyboard.Dispose();
                if (_mouseService is IDisposable disposableMouse)
                    disposableMouse.Dispose();
                if (_screenService is IDisposable disposableScreen)
                    disposableScreen.Dispose();
                
                _isInitialized = false;
            }
        }
    }
}