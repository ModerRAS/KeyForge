using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using KeyForge.Domain.Services;
using KeyForge.Domain.Aggregates;
using KeyForge.Infrastructure.Data;
using KeyForge.Infrastructure.Extensions;

namespace KeyForge.Infrastructure.Services
{
    /// <summary>
    /// KeyForge应用程序主服务
    /// 
    /// 原本实现：完整的应用程序生命周期管理，支持多种运行模式
    /// 简化实现：基本的服务协调和启动管理
    /// </summary>
    public class KeyForgeApplicationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KeyForgeApplicationService> _logger;
        private readonly IConfiguration _configuration;
        private CancellationTokenSource? _cancellationTokenSource;

        public KeyForgeApplicationService(
            IServiceProvider serviceProvider,
            ILogger<KeyForgeApplicationService> _logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._logger = _logger ?? throw new ArgumentNullException(nameof(_logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KeyForge应用程序启动中...");

            try
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // 初始化数据库
                await _serviceProvider.InitializeKeyForgeDatabaseAsync();

                // 启动后台服务
                await StartBackgroundServicesAsync(_cancellationTokenSource.Token);

                _logger.LogInformation("KeyForge应用程序启动完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KeyForge应用程序启动失败");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KeyForge应用程序停止中...");

            try
            {
                _cancellationTokenSource?.Cancel();

                // 清理资源
                await CleanupResourcesAsync();

                _logger.LogInformation("KeyForge应用程序停止完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KeyForge应用程序停止失败");
            }
        }

        /// <summary>
        /// 执行完整的工作流程：Sense -> Judge -> Act
        /// </summary>
        public async Task<WorkflowResult> ExecuteWorkflowAsync(WorkflowRequest request)
        {
            try
            {
                _logger.LogInformation($"开始执行工作流程: {request.WorkflowName}");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // 1. Sense阶段 - 感知屏幕状态
                var senseResult = await ExecuteSensePhaseAsync(request);
                
                // 2. Judge阶段 - 基于识别结果进行决策
                var judgmentResult = await ExecuteJudgePhaseAsync(request, senseResult);
                
                // 3. Act阶段 - 执行决策结果
                var executionResult = await ExecuteActPhaseAsync(request, judgmentResult);

                stopwatch.Stop();

                var workflowResult = new WorkflowResult(
                    request.WorkflowName,
                    true,
                    Duration.FromMilliseconds(stopwatch.ElapsedMilliseconds),
                    senseResult,
                    judgmentResult,
                    executionResult
                );

                _logger.LogInformation($"工作流程执行完成: {request.WorkflowName}, 耗时: {workflowResult.TotalDuration.TotalMilliseconds:F2}ms");

                return workflowResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"工作流程执行失败: {request.WorkflowName}");
                return new WorkflowResult(
                    request.WorkflowName,
                    false,
                    Duration.Zero,
                    null,
                    null,
                    null,
                    ex.Message
                );
            }
        }

        private async Task<SenseResult> ExecuteSensePhaseAsync(WorkflowRequest request)
        {
            using var scope = _serviceProvider.CreateScope();
            var senseService = scope.ServiceProvider.GetRequiredService<ISenseService>();

            var senseRequest = new SenseRequest(
                $"{request.WorkflowName}_Sense",
                request.Templates,
                request.Region,
                request.RecognitionParameters,
                request.WindowHandle,
                request.UseGrayscale,
                request.EnableImageEnhancement,
                request.Metadata
            );

            return await senseService.SenseAsync(senseRequest);
        }

        private async Task<JudgmentResult> ExecuteJudgePhaseAsync(WorkflowRequest request, SenseResult senseResult)
        {
            using var scope = _serviceProvider.CreateScope();
            var judgeService = scope.ServiceProvider.GetRequiredService<IJudgeService>();

            var judgmentRequest = new JudgmentRequest(
                $"{request.WorkflowName}_Judge",
                request.RuleIds,
                request.StateMachineId,
                new List<SenseResult> { senseResult },
                request.Variables,
                request.Metadata,
                request.DefaultActions
            );

            return await judgeService.JudgeAsync(judgmentRequest);
        }

        private async Task<ExecutionResult> ExecuteActPhaseAsync(WorkflowRequest request, JudgmentResult judgmentResult)
        {
            using var scope = _serviceProvider.CreateScope();
            var actService = scope.ServiceProvider.GetRequiredService<IActService>();

            // 将决策结果转换为动作序列
            var actions = ConvertDecisionToActions(judgmentResult.Decision);

            var executionRequest = new ExecutionRequest(
                $"{request.WorkflowName}_Act",
                actions,
                request.ErrorHandlingStrategy,
                request.CancellationToken
            );

            return await actService.ExecuteAsync(executionRequest);
        }

        private List<GameAction> ConvertDecisionToActions(Decision decision)
        {
            var actions = new List<GameAction>();

            foreach (var actionName in decision.ActionsToExecute)
            {
                var action = CreateActionFromName(actionName);
                if (action != null)
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        private GameAction? CreateActionFromName(string actionName)
        {
            // 简化实现：根据动作名称创建对应的GameAction
            // 在实际实现中，应该有更复杂的动作映射逻辑
            
            return actionName.ToLower() switch
            {
                "press_enter" => new GameAction(
                    ActionId.New(),
                    ActionType.KeyDown,
                    KeyCode.Enter
                ),
                "press_space" => new GameAction(
                    ActionId.New(),
                    ActionType.KeyDown,
                    KeyCode.Space
                ),
                "click_left" => new GameAction(
                    ActionId.New(),
                    ActionType.MouseClick,
                    MouseButton.Left,
                    100, 100
                ),
                "delay_100ms" => new GameAction(
                    ActionId.New(),
                    ActionType.Delay,
                    100
                ),
                _ => null
            };
        }

        private async Task StartBackgroundServicesAsync(CancellationToken cancellationToken)
        {
            // 简化实现：启动一个后台监控任务
            // 在实际实现中，应该启动多个后台服务
            
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // 定期健康检查
                        await PerformHealthCheckAsync();
                        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常退出
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台服务异常");
                }
            }, cancellationToken);
        }

        private async Task PerformHealthCheckAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<KeyForgeDbContext>();
                
                // 检查数据库连接
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogWarning("数据库连接失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查失败");
            }
        }

        private async Task CleanupResourcesAsync()
        {
            // 清理临时文件
            // 断开数据库连接
            // 停止所有正在执行的任务
            
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 工作流程请求
    /// </summary>
    public class WorkflowRequest : ValueObject
    {
        public string WorkflowName { get; }
        public List<ImageTemplate> Templates { get; }
        public ScreenRegion? Region { get; }
        public RecognitionParameters RecognitionParameters { get; }
        public List<RuleId> RuleIds { get; }
        public StateMachineId? StateMachineId { get; }
        public Dictionary<string, object> Variables { get; }
        public Dictionary<string, object> Metadata { get; }
        public List<string> DefaultActions { get; }
        public ErrorHandlingStrategy ErrorHandlingStrategy { get; }
        public IntPtr WindowHandle { get; }
        public bool UseGrayscale { get; }
        public bool EnableImageEnhancement { get; }
        public CancellationToken CancellationToken { get; }

        public WorkflowRequest(
            string workflowName,
            List<ImageTemplate> templates,
            ScreenRegion? region,
            RecognitionParameters recognitionParameters,
            List<RuleId> ruleIds,
            StateMachineId? stateMachineId,
            Dictionary<string, object> variables,
            Dictionary<string, object> metadata,
            List<string> defaultActions,
            ErrorHandlingStrategy errorHandlingStrategy,
            IntPtr windowHandle = default,
            bool useGrayscale = true,
            bool enableImageEnhancement = false,
            CancellationToken cancellationToken = default)
        {
            WorkflowName = workflowName;
            Templates = templates ?? new List<ImageTemplate>();
            Region = region;
            RecognitionParameters = recognitionParameters;
            RuleIds = ruleIds ?? new List<RuleId>();
            StateMachineId = stateMachineId;
            Variables = variables ?? new Dictionary<string, object>();
            Metadata = metadata ?? new Dictionary<string, object>();
            DefaultActions = defaultActions ?? new List<string>();
            ErrorHandlingStrategy = errorHandlingStrategy;
            WindowHandle = windowHandle;
            UseGrayscale = useGrayscale;
            EnableImageEnhancement = enableImageEnhancement;
            CancellationToken = cancellationToken;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return WorkflowName;
            yield return Templates.Count;
            yield return RecognitionParameters;
            yield return RuleIds.Count;
            yield return ErrorHandlingStrategy;
        }
    }

    /// <summary>
    /// 工作流程结果
    /// </summary>
    public class WorkflowResult : ValueObject
    {
        public string WorkflowName { get; }
        public bool Success { get; }
        public Duration TotalDuration { get; }
        public SenseResult? SenseResult { get; }
        public JudgmentResult? JudgmentResult { get; }
        public ExecutionResult? ExecutionResult { get; }
        public string? ErrorMessage { get; }

        public WorkflowResult(
            string workflowName,
            bool success,
            Duration totalDuration,
            SenseResult? senseResult,
            JudgmentResult? judgmentResult,
            ExecutionResult? executionResult,
            string? errorMessage = null)
        {
            WorkflowName = workflowName;
            Success = success;
            TotalDuration = totalDuration;
            SenseResult = senseResult;
            JudgmentResult = judgmentResult;
            ExecutionResult = executionResult;
            ErrorMessage = errorMessage;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return WorkflowName;
            yield return Success;
            yield return TotalDuration;
        }
    }

    /// <summary>
    /// 配置管理服务
    /// </summary>
    public class ConfigurationManager
    {
        private readonly IConfiguration _configuration;

        public ConfigurationManager(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public T GetValue<T>(string key, T defaultValue = default!)
        {
            return _configuration.GetValue(key, defaultValue);
        }

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name) ?? string.Empty;
        }

        public DatabaseOptions GetDatabaseOptions()
        {
            return _configuration.GetSection("Database").Get<DatabaseOptions>() 
                   ?? new DatabaseOptions();
        }

        public RecognitionParameters GetRecognitionParameters()
        {
            var parameters = _configuration.GetSection("Recognition").Get<RecognitionParameters>();
            return parameters != null ? parameters : RecognitionParameters.Default;
        }
    }
}