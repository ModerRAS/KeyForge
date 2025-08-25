using KeyForge.Abstractions.Models.Core;
using KeyForge.Abstractions.Interfaces.Core;
using KeyForge.Abstractions.Interfaces.HAL;
using KeyForge.Abstractions.Models.Input;
using Microsoft.Extensions.Logging;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 跨平台鼠标输入服务实现
    /// 【优化实现】实现了统一的鼠标输入服务，支持跨平台部署
    /// 原实现：鼠标功能直接绑定到Windows API，缺乏抽象
    /// 优化：通过HAL抽象，实现了跨平台的鼠标输入服务
    /// </summary>
    public class MouseService : IMouseService
    {
        private readonly ILogger<MouseService> _logger;
        private readonly IMouseHAL _mouseHAL;
        private readonly object _lock = new object();
        private bool _isInitialized = false;
        private bool _isDisposed = false;

        public ServiceStatus Status { get; private set; } = ServiceStatus.Stopped;

        public event EventHandler<InputEventArgs>? OnInput;

        public MouseService(ILogger<MouseService> logger, IMouseHAL mouseHAL)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mouseHAL = mouseHAL ?? throw new ArgumentNullException(nameof(mouseHAL));
        }

        public async Task<bool> InitializeAsync()
        {
            lock (_lock)
            {
                if (_isInitialized)
                {
                    _logger.LogWarning("鼠标服务已经初始化");
                    return true;
                }
            }

            try
            {
                Status = ServiceStatus.Starting;
                _logger.LogInformation("初始化鼠标输入服务");

                // 初始化HAL
                var halResult = await _mouseHAL.InitializeAsync();
                if (!halResult)
                {
                    Status = ServiceStatus.Error;
                    _logger.LogError("鼠标HAL初始化失败");
                    return false;
                }

                // 设置鼠标钩子
                var hookResult = await _mouseHAL.SetMouseHookAsync(OnMouseHook);
                if (!hookResult)
                {
                    _logger.LogWarning("鼠标钩子设置失败，但服务仍可正常工作");
                }

                _isInitialized = true;
                Status = ServiceStatus.Running;
                _logger.LogInformation("鼠标输入服务初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                _logger.LogError(ex, "鼠标输入服务初始化失败");
                return false;
            }
        }

        public async Task StartAsync()
        {
            if (Status == ServiceStatus.Running)
            {
                _logger.LogWarning("鼠标服务已在运行");
                return;
            }

            try
            {
                Status = ServiceStatus.Starting;
                _logger.LogInformation("启动鼠标输入服务");

                if (!_isInitialized)
                {
                    var initResult = await InitializeAsync();
                    if (!initResult)
                    {
                        Status = ServiceStatus.Error;
                        return;
                    }
                }

                Status = ServiceStatus.Running;
                _logger.LogInformation("鼠标输入服务启动成功");
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                _logger.LogError(ex, "鼠标输入服务启动失败");
            }
        }

        public async Task StopAsync()
        {
            if (Status == ServiceStatus.Stopped)
            {
                _logger.LogWarning("鼠标服务已停止");
                return;
            }

            try
            {
                Status = ServiceStatus.Stopping;
                _logger.LogInformation("停止鼠标输入服务");

                // 移除鼠标钩子
                await _mouseHAL.RemoveMouseHookAsync();

                Status = ServiceStatus.Stopped;
                _logger.LogInformation("鼠标输入服务停止成功");
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                _logger.LogError(ex, "鼠标输入服务停止失败");
            }
        }

        public async Task<bool> MoveToAsync(int x, int y)
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("鼠标服务未运行，状态：{Status}", Status);
                return false;
            }

            try
            {
                var result = await _mouseHAL.MoveMouseAsync(x, y);
                
                if (result)
                {
                    _logger.LogDebug("移动鼠标成功：X={X}, Y={Y}", x, y);
                }
                else
                {
                    _logger.LogWarning("移动鼠标失败：X={X}, Y={Y}", x, y);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移动鼠标异常：X={X}, Y={Y}", x, y);
                return false;
            }
        }

        public async Task<bool> ClickAsync(MouseButton button, ClickType clickType = ClickType.Single)
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("鼠标服务未运行，状态：{Status}", Status);
                return false;
            }

            try
            {
                switch (clickType)
                {
                    case ClickType.Single:
                        return await SingleClickAsync(button);
                    case ClickType.Double:
                        return await DoubleClickAsync(button);
                    case ClickType.Triple:
                        return await TripleClickAsync(button);
                    default:
                        _logger.LogWarning("不支持的点击类型：{ClickType}", clickType);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "点击鼠标异常：Button={Button}, ClickType={ClickType}", button, clickType);
                return false;
            }
        }

        public async Task<bool> MouseDownAsync(MouseButton button)
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("鼠标服务未运行，状态：{Status}", Status);
                return false;
            }

            try
            {
                var currentPosition = await GetPositionAsync();
                var result = await _mouseHAL.SendMouseEventAsync(currentPosition.X, currentPosition.Y, (int)button, true);
                
                if (result)
                {
                    _logger.LogDebug("鼠标按下成功：Button={Button}", button);
                }
                else
                {
                    _logger.LogWarning("鼠标按下失败：Button={Button}", button);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "鼠标按下异常：Button={Button}", button);
                return false;
            }
        }

        public async Task<bool> MouseUpAsync(MouseButton button)
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("鼠标服务未运行，状态：{Status}", Status);
                return false;
            }

            try
            {
                var currentPosition = await GetPositionAsync();
                var result = await _mouseHAL.SendMouseEventAsync(currentPosition.X, currentPosition.Y, (int)button, false);
                
                if (result)
                {
                    _logger.LogDebug("鼠标释放成功：Button={Button}", button);
                }
                else
                {
                    _logger.LogWarning("鼠标释放失败：Button={Button}", button);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "鼠标释放异常：Button={Button}", button);
                return false;
            }
        }

        public async Task<bool> ScrollAsync(int delta)
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("鼠标服务未运行，状态：{Status}", Status);
                return false;
            }

            try
            {
                var result = await _mouseHAL.SendMouseWheelEventAsync(delta);
                
                if (result)
                {
                    _logger.LogDebug("鼠标滚轮成功：Delta={Delta}", delta);
                }
                else
                {
                    _logger.LogWarning("鼠标滚轮失败：Delta={Delta}", delta);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "鼠标滚轮异常：Delta={Delta}", delta);
                return false;
            }
        }

        public async Task<(int X, int Y)> GetPositionAsync()
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("鼠标服务未运行，状态：{Status}", Status);
                return (0, 0);
            }

            try
            {
                var position = await _mouseHAL.GetMousePositionAsync();
                _logger.LogDebug("获取鼠标位置成功：X={X}, Y={Y}", position.X, position.Y);
                return position;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取鼠标位置异常");
                return (0, 0);
            }
        }

        private async Task<bool> SingleClickAsync(MouseButton button)
        {
            return await MouseDownAsync(button) && await MouseUpAsync(button);
        }

        private async Task<bool> DoubleClickAsync(MouseButton button)
        {
            const int doubleClickDelay = 50; // 毫秒
            
            if (!await SingleClickAsync(button))
                return false;
            
            await Task.Delay(doubleClickDelay);
            return await SingleClickAsync(button);
        }

        private async Task<bool> TripleClickAsync(MouseButton button)
        {
            const int tripleClickDelay = 50; // 毫秒
            
            if (!await DoubleClickAsync(button))
                return false;
            
            await Task.Delay(tripleClickDelay);
            return await SingleClickAsync(button);
        }

        private void OnMouseHook(int x, int y, int mouseButton, bool isMouseDown)
        {
            try
            {
                var inputEvent = new InputEventArgs
                {
                    EventType = isMouseDown ? InputEventType.MouseDown : InputEventType.MouseUp,
                    Timestamp = DateTime.Now,
                    MouseEvent = new MouseEventData
                    {
                        Button = (MouseButton)mouseButton,
                        MouseState = isMouseDown ? MouseState.Down : MouseState.Up,
                        X = x,
                        Y = y,
                        WheelDelta = 0,
                        DeltaX = 0,
                        DeltaY = 0
                    },
                    Source = "MouseService",
                    Handled = false
                };

                OnInput?.Invoke(this, inputEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "鼠标钩子事件处理失败：X={X}, Y={Y}, Button={Button}, IsDown={IsDown}", 
                    x, y, mouseButton, isMouseDown);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                StopAsync().Wait();
                _mouseHAL?.Dispose();
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}