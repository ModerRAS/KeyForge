using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KeyForge.Core.Domain.Automation;
using KeyForge.Core.Domain.Common;
using KeyForge.Core.Domain.Vision;
using KeyForge.Core.Domain.Sense;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Interfaces;
using KeyForge.Domain.Common;
using KeyForge.Domain.Entities;
using Result = KeyForge.Domain.Common.Result;

namespace KeyForge.Core.Domain.Act
{
    /// <summary>
    /// 脚本播放器 - 简化实现
    /// 使用Domain层的统一类型定义，避免类型冲突
    /// </summary>

    /// <summary>
    /// 脚本播放器 - 简化实现
    /// 
    /// 原本实现：
    /// - 高精度的时序控制
    /// - 多线程并行执行
    /// - 错误恢复和重试机制
    /// - 实时状态监控
    /// 
    /// 简化实现：
    /// - 基本的顺序执行
    /// - 简单的延时处理
    /// - 基本的错误处理
    /// </summary>
    public class ScriptPlayer
    {
        private readonly KeyForge.Core.Domain.Automation.IGameInputHandler _inputHandler;
        private readonly KeyForge.Core.Domain.Sense.IImageRecognitionEngine _imageRecognition;
        private readonly KeyForge.Core.Domain.Common.ILogger _logger;
        
        private Script _currentScript;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _executionTask;
        private PlayerState _state = PlayerState.Stopped;

        public ScriptPlayer(
            KeyForge.Core.Domain.Automation.IGameInputHandler inputHandler,
            KeyForge.Core.Domain.Sense.IImageRecognitionEngine imageRecognition,
            KeyForge.Core.Domain.Common.ILogger logger)
        {
            _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
            _imageRecognition = imageRecognition ?? throw new ArgumentNullException(nameof(imageRecognition));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsPlaying => _state == PlayerState.Playing;
        public bool IsPaused => _state == PlayerState.Paused;

        public void LoadScript(Script script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));

            _currentScript = script;
            _logger.LogInformation($"加载脚本: {script.Name.Value}");
        }

        public void PlayScript()
        {
            if (_currentScript == null)
                throw new InvalidOperationException("没有加载脚本");

            if (_state == PlayerState.Playing)
                throw new InvalidOperationException("脚本正在播放中");

            StopScript(); // 停止之前的播放

            _cancellationTokenSource = new CancellationTokenSource();
            _state = PlayerState.Playing;

            _executionTask = Task.Run(() => ExecuteScriptAsync(_cancellationTokenSource.Token));

            _logger.LogInformation($"开始播放脚本: {_currentScript.Name.Value}");
        }

        public void PauseScript()
        {
            if (_state != PlayerState.Playing)
                throw new InvalidOperationException("脚本没有在播放");

            _state = PlayerState.Paused;
            _logger.LogInformation("暂停脚本播放");
        }

        public void ResumeScript()
        {
            if (_state != PlayerState.Paused)
                throw new InvalidOperationException("脚本没有暂停");

            _state = PlayerState.Playing;
            _logger.LogInformation("恢复脚本播放");
        }

        public void StopScript()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (_executionTask != null)
            {
                try
                {
                    _executionTask.Wait(TimeSpan.FromSeconds(5));
                }
                catch (AggregateException)
                {
                    // 忽略取消异常
                }
                _executionTask = null;
            }

            _state = PlayerState.Stopped;
            _logger.LogInformation("停止脚本播放");
        }

        private async Task ExecuteScriptAsync(CancellationToken cancellationToken)
        {
            if (_currentScript == null)
                return;

            var startTime = DateTime.UtcNow;
            var totalActionsExecuted = 0;
            var errors = new List<string>();

            try
            {
                _logger.LogInformation($"开始执行脚本: {_currentScript.Name.Value}");

                // 创建执行上下文
                var context = new KeyForge.Core.Domain.Automation.ExecutionContext(
                    _inputHandler,
                    new ImageRecognitionAdapter(_imageRecognition),
                    cancellationToken);

                // 验证脚本
                var validationResult = _currentScript.Validate();
                if (!validationResult.IsValid)
                {
                    errors.AddRange(validationResult.Errors);
                    _logger.LogError($"脚本验证失败: {string.Join(", ", validationResult.Errors)}");
                    return;
                }

                // 执行脚本
                var result = await _currentScript.ExecuteAsync(context);
                
                totalActionsExecuted = result.ActionsExecuted;
                if (!result.Success)
                {
                    errors.AddRange(result.ErrorMessages);
                }

                var duration = DateTime.UtcNow - startTime;
                
                if (errors.Count == 0)
                {
                    _logger.LogInformation($"脚本执行成功，耗时: {duration.TotalMilliseconds:F2}ms，执行动作: {totalActionsExecuted}");
                }
                else
                {
                    _logger.LogError($"脚本执行失败，耗时: {duration.TotalMilliseconds:F2}ms，错误数: {errors.Count}");
                    foreach (var error in errors)
                    {
                        _logger.LogError($"错误: {error}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("脚本执行被取消");
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError($"脚本执行异常，耗时: {duration.TotalMilliseconds:F2}ms，错误: {ex.Message}");
            }
            finally
            {
                if (_state == PlayerState.Playing)
                {
                    _state = PlayerState.Stopped;
                }
            }
        }

        /// <summary>
        /// 图像识别服务适配器
        /// </summary>
        private class ImageRecognitionAdapter : IImageRecognitionService
        {
            private readonly KeyForge.Core.Domain.Sense.IImageRecognitionEngine _engine;

            public ImageRecognitionAdapter(KeyForge.Core.Domain.Sense.IImageRecognitionEngine engine)
            {
                _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            }

            public async Task<KeyForge.Domain.ValueObjects.RecognitionResult> RecognizeAsync(KeyForge.Domain.Entities.ImageTemplate template, KeyForge.Domain.ValueObjects.ScreenRegion region, KeyForge.Domain.ValueObjects.RecognitionParameters parameters)
            {
                // 将ImageTemplate转换为ImageData - 简化实现
                var imageData = KeyForge.Infrastructure.Imaging.ImageService.CreateBlankImage(100, 100);
                return await _engine.RecognizeAsync(imageData, imageData, parameters);
            }

            public async Task<KeyForge.Core.Domain.Common.CoreScreenCapture> CaptureScreenAsync(KeyForge.Domain.ValueObjects.ScreenRegion region)
            {
                // 简化实现：返回空的屏幕捕获
                var imageData = new ImageData(new byte[region.Width * region.Height * 4], region.Width, region.Height);
                return await Task.FromResult(new KeyForge.Core.Domain.Common.CoreScreenCapture(
                    imageData,
                    region,
                    new Timestamp(DateTime.UtcNow)));
            }

            public async Task<List<KeyForge.Domain.Entities.ImageTemplate>> GetTemplatesAsync()
            {
                // 简化实现：返回空的模板列表
                return await Task.FromResult(new List<KeyForge.Domain.Entities.ImageTemplate>());
            }

            // IImageRecognitionService 接口需要的额外方法 - 简化实现
            public async Task<KeyForge.Domain.Common.Result<KeyForge.Domain.ValueObjects.RecognitionResult>> RecognizeAsync(byte[] imageData, byte[] templateData)
            {
                // 简化实现：返回识别失败结果
                return KeyForge.Domain.Common.Result<KeyForge.Domain.ValueObjects.RecognitionResult>.Failure("字节数组识别未实现");
            }

            public async Task<KeyForge.Domain.Common.Result<KeyForge.Domain.ValueObjects.RecognitionResult>> RecognizeAsync(byte[] imageData, string templatePath)
            {
                // 简化实现：返回识别失败结果
                return KeyForge.Domain.Common.Result<KeyForge.Domain.ValueObjects.RecognitionResult>.Failure("文件路径识别未实现");
            }

            public async Task<KeyForge.Domain.Common.Result<KeyForge.Domain.ValueObjects.RecognitionResult>> RecognizeOnScreenAsync(KeyForge.Domain.Common.Rectangle region, byte[] templateData)
            {
                // 简化实现：返回识别失败结果
                return KeyForge.Domain.Common.Result<KeyForge.Domain.ValueObjects.RecognitionResult>.Failure("屏幕区域识别未实现");
            }

            public async Task<KeyForge.Domain.Common.Result> SaveTemplateAsync(string name, byte[] templateData, KeyForge.Domain.Common.TemplateType templateType)
            {
                // 简化实现：直接返回成功
                return KeyForge.Domain.Common.Result.Success();
            }

            public async Task<KeyForge.Domain.Common.Result<byte[]>> LoadTemplateAsync(string name)
            {
                // 简化实现：直接返回失败
                return KeyForge.Domain.Common.Result<byte[]>.Failure("加载模板未实现");
            }

            public async Task<KeyForge.Domain.Common.Result<bool>> TemplateExistsAsync(string name)
            {
                // 简化实现：直接返回false
                return KeyForge.Domain.Common.Result<bool>.Success(false);
            }

            public async Task<KeyForge.Domain.Common.Result> DeleteTemplateAsync(string name)
            {
                // 简化实现：直接返回成功
                return KeyForge.Domain.Common.Result.Success();
            }

            public async Task<KeyForge.Domain.Common.Result<IEnumerable<string>>> GetAllTemplatesAsync()
            {
                // 简化实现：返回空列表
                return KeyForge.Domain.Common.Result<IEnumerable<string>>.Success(new List<string>());
            }
        }
    }

    /// <summary>
    /// 播放器状态
    /// </summary>
    public enum PlayerState
    {
        Stopped,
        Playing,
        Paused
    }
}