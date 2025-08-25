using KeyForge.Abstractions.Enums;
using KeyForge.Abstractions.Interfaces.Core;
using KeyForge.Abstractions.Interfaces.HAL;
using KeyForge.Abstractions.Models.Input;
using Microsoft.Extensions.Logging;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 跨平台键盘输入服务实现
    /// 【优化实现】实现了统一的键盘输入服务，支持跨平台部署
    /// 原实现：键盘功能直接绑定到Windows API，缺乏抽象
    /// 优化：通过HAL抽象，实现了跨平台的键盘输入服务
    /// </summary>
    public class KeyboardService : IKeyboardService
    {
        private readonly ILogger<KeyboardService> _logger;
        private readonly IKeyboardHAL _keyboardHAL;
        private readonly object _lock = new object();
        private bool _isInitialized = false;
        private bool _isDisposed = false;

        public ServiceStatus Status { get; private set; } = ServiceStatus.Stopped;

        public event EventHandler<InputEventArgs>? OnInput;

        public KeyboardService(ILogger<KeyboardService> logger, IKeyboardHAL keyboardHAL)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyboardHAL = keyboardHAL ?? throw new ArgumentNullException(nameof(keyboardHAL));
        }

        public async Task<bool> InitializeAsync()
        {
            lock (_lock)
            {
                if (_isInitialized)
                {
                    _logger.LogWarning("键盘服务已经初始化");
                    return true;
                }
            }

            try
            {
                Status = ServiceStatus.Starting;
                _logger.LogInformation("初始化键盘输入服务");

                // 初始化HAL
                var halResult = await _keyboardHAL.InitializeAsync();
                if (!halResult)
                {
                    Status = ServiceStatus.Error;
                    _logger.LogError("键盘HAL初始化失败");
                    return false;
                }

                // 设置键盘钩子
                var hookResult = await _keyboardHAL.SetKeyboardHookAsync(OnKeyboardHook);
                if (!hookResult)
                {
                    _logger.LogWarning("键盘钩子设置失败，但服务仍可正常工作");
                }

                _isInitialized = true;
                Status = ServiceStatus.Running;
                _logger.LogInformation("键盘输入服务初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                _logger.LogError(ex, "键盘输入服务初始化失败");
                return false;
            }
        }

        public async Task StartAsync()
        {
            if (Status == ServiceStatus.Running)
            {
                _logger.LogWarning("键盘服务已在运行");
                return;
            }

            try
            {
                Status = ServiceStatus.Starting;
                _logger.LogInformation("启动键盘输入服务");

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
                _logger.LogInformation("键盘输入服务启动成功");
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                _logger.LogError(ex, "键盘输入服务启动失败");
            }
        }

        public async Task StopAsync()
        {
            if (Status == ServiceStatus.Stopped)
            {
                _logger.LogWarning("键盘服务已停止");
                return;
            }

            try
            {
                Status = ServiceStatus.Stopping;
                _logger.LogInformation("停止键盘输入服务");

                // 移除键盘钩子
                await _keyboardHAL.RemoveKeyboardHookAsync();

                Status = ServiceStatus.Stopped;
                _logger.LogInformation("键盘输入服务停止成功");
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                _logger.LogError(ex, "键盘输入服务停止失败");
            }
        }

        public async Task<bool> SendKeyAsync(KeyCode keyCode, KeyState state)
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("键盘服务未运行，状态：{Status}", Status);
                return false;
            }

            try
            {
                var result = await _keyboardHAL.SendKeyEventAsync((int)keyCode, state == KeyState.Down || state == KeyState.Pressed);
                
                if (result)
                {
                    _logger.LogDebug("发送键盘按键成功：KeyCode={KeyCode}, State={State}", keyCode, state);
                }
                else
                {
                    _logger.LogWarning("发送键盘按键失败：KeyCode={KeyCode}, State={State}", keyCode, state);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送键盘按键异常：KeyCode={KeyCode}, State={State}", keyCode, state);
                return false;
            }
        }

        public async Task<bool> SendTextAsync(string text)
        {
            if (Status != ServiceStatus.Running)
            {
                _logger.LogWarning("键盘服务未运行，状态：{Status}", Status);
                return false;
            }

            if (string.IsNullOrEmpty(text))
            {
                _logger.LogWarning("发送的文本为空");
                return false;
            }

            try
            {
                var result = await _keyboardHAL.SendTextAsync(text);
                
                if (result)
                {
                    _logger.LogDebug("发送文本成功：Text={Text}", text);
                }
                else
                {
                    _logger.LogWarning("发送文本失败：Text={Text}", text);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送文本异常：Text={Text}", text);
                return false;
            }
        }

        public async Task<bool> KeyDownAsync(KeyCode keyCode)
        {
            return await SendKeyAsync(keyCode, KeyState.Down);
        }

        public async Task<bool> KeyUpAsync(KeyCode keyCode)
        {
            return await SendKeyAsync(keyCode, KeyState.Up);
        }

        private void OnKeyboardHook(int keyCode, bool isKeyDown)
        {
            try
            {
                var inputEvent = new InputEventArgs
                {
                    EventType = isKeyDown ? InputEventType.KeyDown : InputEventType.KeyUp,
                    Timestamp = DateTime.Now,
                    KeyEvent = new KeyEventData
                    {
                        KeyCode = (KeyCode)keyCode,
                        KeyState = isKeyDown ? KeyState.Down : KeyState.Up,
                        Modifiers = KeyModifiers.None, // 可以扩展获取修饰键状态
                        RepeatCount = 1,
                        ScanCode = keyCode,
                        IsExtendedKey = false
                    },
                    Source = "KeyboardService",
                    Handled = false
                };

                OnInput?.Invoke(this, inputEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "键盘钩子事件处理失败：KeyCode={KeyCode}, IsKeyDown={IsKeyDown}", keyCode, isKeyDown);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                StopAsync().Wait();
                _keyboardHAL?.Dispose();
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}