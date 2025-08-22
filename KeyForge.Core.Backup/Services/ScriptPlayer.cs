using System;
using System.Threading;
using System.Threading.Tasks;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
    using KeyForge.Domain.Common;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 脚本播放器 - 简化实现
    /// </summary>
    public class ScriptPlayer : IScriptPlayer
    {
        private readonly IInputSimulator _inputSimulator;
        private readonly ILoggerService _logger;
        private Script _currentScript;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _playbackTask;
        private bool _isPlaying;
        private bool _isPaused;

        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        public Script CurrentScript => _currentScript;

        public ScriptPlayer(IInputSimulator inputSimulator, ILoggerService logger)
        {
            _inputSimulator = inputSimulator;
            _logger = logger;
        }

        /// <summary>
        /// 加载脚本
        /// </summary>
        public void LoadScript(Script script)
        {
            _currentScript = script ?? throw new ArgumentNullException(nameof(script));
            _logger.Info($"加载脚本: {script.Name}, 动作数量: {script.GetActionCount()}");
        }

        /// <summary>
        /// 播放脚本
        /// </summary>
        public void PlayScript()
        {
            if (_currentScript == null)
            {
                throw new InvalidOperationException("没有加载脚本");
            }

            if (_isPlaying)
            {
                if (_isPaused)
                {
                    ResumeScript();
                }
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _playbackTask = Task.Run(() => ExecuteScript(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// 暂停脚本
        /// </summary>
        public void PauseScript()
        {
            if (!_isPlaying || _isPaused)
                return;

            _isPaused = true;
            _logger.Info("脚本播放已暂停");
        }

        /// <summary>
        /// 恢复脚本
        /// </summary>
        public void ResumeScript()
        {
            if (!_isPlaying || !_isPaused)
                return;

            _isPaused = false;
            _logger.Info("脚本播放已恢复");
        }

        /// <summary>
        /// 停止脚本
        /// </summary>
        public void StopScript()
        {
            if (!_isPlaying)
                return;

            _cancellationTokenSource?.Cancel();
            
            try
            {
                _playbackTask?.Wait();
            }
            catch (OperationCanceledException)
            {
                // 正常取消，忽略异常
            }
            catch (Exception ex)
            {
                _logger.Error($"停止脚本时发生错误: {ex.Message}");
            }

            _isPlaying = false;
            _isPaused = false;
            _logger.Info("脚本播放已停止");
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        private async Task ExecuteScript(CancellationToken cancellationToken)
        {
            _isPlaying = true;
            _isPaused = false;
            
            _logger.Info($"开始执行脚本: {_currentScript.Name}");

            try
            {
                int repeatCount = 0;
                int totalRepeats = _currentScript.RepeatCount;

                while (repeatCount < totalRepeats || _currentScript.Loop)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_currentScript.Loop)
                    {
                        repeatCount++;
                        _logger.Debug($"循环播放第 {repeatCount} 次");
                    }

                    await ExecuteActions(cancellationToken);

                    if (!_currentScript.Loop)
                    {
                        repeatCount++;
                    }

                    // 如果不是循环模式且已经完成指定次数，则退出
                    if (!_currentScript.Loop && repeatCount >= totalRepeats)
                    {
                        break;
                    }
                }

                _logger.Info($"脚本执行完成: {_currentScript.Name}");
            }
            catch (OperationCanceledException)
            {
                _logger.Info("脚本执行被取消");
            }
            catch (Exception ex)
            {
                _logger.Error($"脚本执行过程中发生错误: {ex.Message}");
                throw;
            }
            finally
            {
                _isPlaying = false;
                _isPaused = false;
            }
        }

        /// <summary>
        /// 执行动作序列
        /// </summary>
        private async Task ExecuteActions(CancellationToken cancellationToken)
        {
            foreach (var action in _currentScript.Actions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 等待暂停状态结束
                while (_isPaused)
                {
                    await Task.Delay(100, cancellationToken);
                }

                try
                {
                    await ExecuteAction(action);
                }
                catch (Exception ex)
                {
                    _logger.Error($"执行动作失败: {action}, 错误: {ex.Message}");
                    // 继续执行下一个动作，不中断整个脚本
                }
            }
        }

        /// <summary>
        /// 执行单个动作
        /// </summary>
        private async Task ExecuteAction(KeyAction action)
        {
            _logger.Debug($"执行动作: {action}");

            switch (action.Type)
            {
                case ActionType.KeyDown:
                    _inputSimulator.SendKey(action.Key, KeyState.Down);
                    break;

                case ActionType.KeyUp:
                    _inputSimulator.SendKey(action.Key, KeyState.Up);
                    break;

                case ActionType.MouseDown:
                    _inputSimulator.SendMouse(action.Button, MouseState.Down);
                    break;

                case ActionType.MouseUp:
                    _inputSimulator.SendMouse(action.Button, MouseState.Up);
                    break;

                case ActionType.MouseMove:
                    _inputSimulator.MoveMouse(action.X, action.Y);
                    break;

                case ActionType.Delay:
                    if (action.Delay > 0)
                    {
                        await Task.Delay(action.Delay);
                    }
                    break;

                default:
                    _logger.Warning($"未知的动作类型: {action.Type}");
                    break;
            }

            // 执行动作后的小延迟，确保动作完成
            if (action.Type != ActionType.Delay)
            {
                await Task.Delay(10);
            }
        }
    }
}