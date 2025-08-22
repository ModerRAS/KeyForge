using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;

namespace KeyForge.HAL.Core
{
    /// <summary>
    /// 硬件抽象层基础类
    /// 简化实现：专注于核心功能，移除复杂的性能监控和健康检查
    /// 原本实现：包含完整的性能监控、健康检查、指标收集等高级功能
    /// </summary>
    public abstract class HardwareAbstractionLayerBase : IHardwareAbstractionLayer
    {
        // 核心服务
        /// <summary>
        /// 键盘服务
        /// </summary>
        public abstract IKeyboardService Keyboard { get; }
        
        /// <summary>
        /// 鼠标服务
        /// </summary>
        public abstract IMouseService Mouse { get; }
        
        /// <summary>
        /// 屏幕服务
        /// </summary>
        public abstract IScreenService Screen { get; }
        
        /// <summary>
        /// 全局热键服务
        /// </summary>
        public abstract IGlobalHotkeyService GlobalHotkeys { get; }
        
        /// <summary>
        /// 窗口服务
        /// </summary>
        public abstract IWindowService Window { get; }
        
        /// <summary>
        /// 图像识别服务
        /// </summary>
        public abstract IImageRecognitionService ImageRecognition { get; }
        
        // 平台信息
        /// <summary>
        /// 平台信息
        /// </summary>
        public abstract PlatformInfo PlatformInfo { get; }
        
        /// <summary>
        /// HAL状态
        /// </summary>
        public HALStatus Status { get; private set; }
        
        // 事件
        /// <summary>
        /// 平台变更事件
        /// </summary>
        public event EventHandler<PlatformEventArgs> PlatformChanged;
        
        /// <summary>
        /// 硬件状态变更事件
        /// </summary>
        public event EventHandler<HardwareEventArgs> HardwareStateChanged;
        
        /// <summary>
        /// 性能报告事件
        /// </summary>
        public event EventHandler<PerformanceEventArgs> PerformanceReported;
        
        // 生命周期管理
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 初始化HAL
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            if (IsInitialized)
                return;
            
            try
            {
                Status = HALStatus.Initializing;
                
                // 检查权限
                await CheckPermissionsInternalAsync();
                
                // 初始化各个服务
                await InitializeServicesAsync();
                
                // 注册事件
                RegisterEvents();
                
                IsInitialized = true;
                Status = HALStatus.Running;
                
                // 触发初始化完成事件
                OnPlatformChanged(new PlatformEventArgs(PlatformInfo, PlatformEventType.Initialized));
            }
            catch (Exception ex)
            {
                Status = HALStatus.Error;
                throw new HALInitializationException(
                    InitializationStage.ServiceInitialization, 
                    "HAL", 
                    "Failed to initialize HAL", 
                    ex);
            }
        }
        
        /// <summary>
        /// 关闭HAL
        /// </summary>
        public virtual async Task ShutdownAsync()
        {
            if (!IsInitialized)
                return;
            
            try
            {
                Status = HALStatus.ShuttingDown;
                
                // 注销事件
                UnregisterEvents();
                
                // 关闭各个服务
                await ShutdownServicesAsync();
                
                IsInitialized = false;
                Status = HALStatus.Shutdown;
                
                // 触发关闭事件
                OnPlatformChanged(new PlatformEventArgs(PlatformInfo, PlatformEventType.Shutdown));
            }
            catch (Exception ex)
            {
                Status = HALStatus.Error;
                throw new HALInitializationException(
                    InitializationStage.ServiceInitialization, 
                    "HAL", 
                    "Failed to shutdown HAL", 
                    ex);
            }
        }
        
        // 权限管理
        /// <summary>
        /// 检查权限
        /// </summary>
        /// <returns>权限状态</returns>
        public virtual async Task<PermissionStatus> CheckPermissionsAsync()
        {
            try
            {
                return await CheckPermissionsInternalAsync();
            }
            catch (Exception ex)
            {
                throw new HALException("Failed to check permissions", ex, HardwareOperation.Unknown, PlatformInfo.Platform);
            }
        }
        
        /// <summary>
        /// 请求权限
        /// </summary>
        /// <param name="request">权限请求</param>
        /// <returns>是否成功</returns>
        public virtual async Task<bool> RequestPermissionsAsync(PermissionRequest request)
        {
            try
            {
                return await RequestPermissionsInternalAsync(request);
            }
            catch (Exception ex)
            {
                throw new HALException("Failed to request permissions", ex, HardwareOperation.Unknown, PlatformInfo.Platform);
            }
        }
        
        // 健康检查（简化实现）
        /// <summary>
        /// 执行健康检查
        /// </summary>
        /// <returns>健康检查结果</returns>
        public virtual async Task<HealthCheckResult> PerformHealthCheckAsync()
        {
            try
            {
                // 简化的健康检查实现
                var componentStatus = new Dictionary<string, bool>();
                
                // 检查各个服务状态
                componentStatus["Keyboard"] = Keyboard != null;
                componentStatus["Mouse"] = Mouse != null;
                componentStatus["Screen"] = Screen != null;
                componentStatus["GlobalHotkeys"] = GlobalHotkeys != null;
                componentStatus["Window"] = Window != null;
                componentStatus["ImageRecognition"] = ImageRecognition != null;
                
                // 检查初始化状态
                componentStatus["Initialized"] = IsInitialized;
                
                var isHealthy = componentStatus.Values.All(status => status);
                
                return new HealthCheckResult(isHealthy, isHealthy ? "All systems operational" : "Some components are not available");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(false, $"Health check failed: {ex.Message}");
            }
        }
        
        // 性能监控（简化实现）
        /// <summary>
        /// 获取性能指标
        /// </summary>
        /// <returns>性能指标</returns>
        public virtual async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            try
            {
                // 简化的性能指标实现
                var metrics = new PerformanceMetrics();
                
                // 获取当前进程的内存使用情况
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                {
                    metrics.MemoryUsage = process.WorkingSet64 / (1024.0 * 1024.0); // MB
                }
                
                // 简单的CPU使用率估算
                metrics.CpuUsage = 0.0; // 简化实现中设为0
                
                // 添加自定义指标
                metrics.CustomMetrics["Initialized"] = IsInitialized ? 1.0 : 0.0;
                metrics.CustomMetrics["ComponentCount"] = 6.0; // 6个核心服务
                
                metrics.Tags["Platform"] = PlatformInfo.Platform.ToString();
                metrics.Tags["HALStatus"] = Status.ToString();
                
                return metrics;
            }
            catch (Exception ex)
            {
                // 返回默认指标
                return new PerformanceMetrics
                {
                    CpuUsage = 0.0,
                    MemoryUsage = 0.0,
                    CustomMetrics = new Dictionary<string, string> { { "Error", ex.Message } }
                };
            }
        }
        
        // 事件触发
        /// <summary>
        /// 触发平台变更事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected virtual void OnPlatformChanged(PlatformEventArgs e)
        {
            PlatformChanged?.Invoke(this, e);
        }
        
        /// <summary>
        /// 触发硬件状态变更事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected virtual void OnHardwareStateChanged(HardwareEventArgs e)
        {
            HardwareStateChanged?.Invoke(this, e);
        }
        
        /// <summary>
        /// 触发性能报告事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected virtual void OnPerformanceReported(PerformanceEventArgs e)
        {
            PerformanceReported?.Invoke(this, e);
        }
        
        // 抽象方法
        /// <summary>
        /// 初始化服务
        /// </summary>
        protected virtual async Task InitializeServicesAsync()
        {
            await Task.WhenAll(
                Keyboard.InitializeAsync(),
                Mouse.InitializeAsync(),
                Screen.InitializeAsync(),
                GlobalHotkeys.InitializeAsync(),
                Window.InitializeAsync(),
                ImageRecognition.InitializeAsync()
            );
        }
        
        /// <summary>
        /// 关闭服务
        /// </summary>
        protected virtual async Task ShutdownServicesAsync()
        {
            await Task.WhenAll(
                Keyboard.ShutdownAsync(),
                Mouse.ShutdownAsync(),
                Screen.ShutdownAsync(),
                GlobalHotkeys.ShutdownAsync(),
                Window.ShutdownAsync(),
                ImageRecognition.ShutdownAsync()
            );
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        protected virtual void RegisterEvents()
        {
            // 注册各个服务的事件
            Keyboard.KeyPressed += OnKeyPressed;
            Keyboard.KeyReleased += OnKeyReleased;
            Mouse.MouseMoved += OnMouseMoved;
            Mouse.MouseButtonClicked += OnMouseButtonClicked;
            Screen.ScreenChanged += OnScreenChanged;
            GlobalHotkeys.HotkeyPressed += OnHotkeyPressed;
            Window.WindowActivated += OnWindowActivated;
        }
        
        /// <summary>
        /// 注销事件
        /// </summary>
        protected virtual void UnregisterEvents()
        {
            // 注销各个服务的事件
            Keyboard.KeyPressed -= OnKeyPressed;
            Keyboard.KeyReleased -= OnKeyReleased;
            Mouse.MouseMoved -= OnMouseMoved;
            Mouse.MouseButtonClicked -= OnMouseButtonClicked;
            Screen.ScreenChanged -= OnScreenChanged;
            GlobalHotkeys.HotkeyPressed -= OnHotkeyPressed;
            Window.WindowActivated -= OnWindowActivated;
        }
        
        /// <summary>
        /// 检查权限（内部实现）
        /// </summary>
        protected virtual async Task<PermissionStatus> CheckPermissionsInternalAsync()
        {
            // 基础实现，子类可以重写
            return PermissionStatus.Granted;
        }
        
        /// <summary>
        /// 请求权限（内部实现）
        /// </summary>
        protected virtual async Task<bool> RequestPermissionsInternalAsync(PermissionRequest request)
        {
            // 基础实现，子类可以重写
            return true;
        }
        
        // 事件处理
        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            OnHardwareStateChanged(new HardwareEventArgs(HardwareOperation.KeyboardOperation, true, null, e));
        }
        
        private void OnKeyReleased(object sender, KeyEventArgs e)
        {
            OnHardwareStateChanged(new HardwareEventArgs(HardwareOperation.KeyboardOperation, true, null, e));
        }
        
        private void OnMouseMoved(object sender, MouseEventArgs e)
        {
            OnHardwareStateChanged(new HardwareEventArgs(HardwareOperation.MouseOperation, true, null, e));
        }
        
        private void OnMouseButtonClicked(object sender, MouseEventArgs e)
        {
            OnHardwareStateChanged(new HardwareEventArgs(HardwareOperation.MouseOperation, true, null, e));
        }
        
        private void OnScreenChanged(object sender, ScreenEventArgs e)
        {
            OnHardwareStateChanged(new HardwareEventArgs(HardwareOperation.ScreenOperation, true, null, e));
        }
        
        private void OnHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            OnHardwareStateChanged(new HardwareEventArgs(HardwareOperation.KeyboardOperation, true, null, e));
        }
        
        private void OnWindowActivated(object sender, WindowEventArgs e)
        {
            OnHardwareStateChanged(new HardwareEventArgs(HardwareOperation.WindowOperation, true, null, e));
        }
        
        // IDisposable实现
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否正在释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放托管资源
                if (IsInitialized)
                {
                    ShutdownAsync().Wait();
                }
                
                // 释放各个服务
                (Keyboard as IDisposable)?.Dispose();
                (Mouse as IDisposable)?.Dispose();
                (Screen as IDisposable)?.Dispose();
                (GlobalHotkeys as IDisposable)?.Dispose();
                (Window as IDisposable)?.Dispose();
                (ImageRecognition as IDisposable)?.Dispose();
            }
        }
        
        /// <summary>
        /// 析构函数
        /// </summary>
        ~HardwareAbstractionLayerBase()
        {
            Dispose(false);
        }
    }
}