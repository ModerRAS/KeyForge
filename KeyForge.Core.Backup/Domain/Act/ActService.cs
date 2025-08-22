namespace KeyForge.Core.Domain.Act
{
    using KeyForge.Core.Domain.Automation;
    using KeyForge.Domain.Common;
using KeyForge.Domain.ValueObjects;
    using KeyForge.Core.Domain.Sense;
    using KeyForge.Domain.Common;

    /// <summary>
    /// 执行服务 - 负责执行按键和鼠标操作
    /// </summary>
    public class ActService
    {
        private readonly IGameInputHandler _inputHandler;
        private readonly IExecutionPlanner _executionPlanner;
        private readonly KeyForge.Core.Domain.Common.ILogger _logger;

        public ActService(
            IGameInputHandler inputHandler,
            IExecutionPlanner executionPlanner,
            KeyForge.Core.Domain.Common.ILogger logger)
        {
            _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
            _executionPlanner = executionPlanner ?? throw new ArgumentNullException(nameof(executionPlanner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 执行动作序列
        /// </summary>
        public async Task<Automation.ExecutionResult> ExecuteAsync(ExecutionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                _logger.LogInformation($"开始执行动作序列: {request.ExecutionName}");

                var startTime = DateTime.UtcNow;
                var totalActionsExecuted = 0;
                var errors = new List<string>();

                // 1. 规划执行计划
                var executionPlan = await _executionPlanner.PlanAsync(request.Actions);
                
                // 2. 执行动作
                foreach (var actionGroup in executionPlan.ActionGroups)
                {
                    if (request.CancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("执行被取消");
                        break;
                    }

                    var groupResult = await ExecuteActionGroupAsync(actionGroup, request);
                    totalActionsExecuted += groupResult.ActionsExecuted;
                    
                    if (!groupResult.Success)
                    {
                        errors.AddRange(groupResult.Errors);
                        
                        // 根据错误处理策略决定是否继续
                        if (request.ErrorHandlingStrategy == ErrorHandlingStrategy.StopOnError)
                        {
                            break;
                        }
                    }
                }

                var duration = Duration.FromMilliseconds((DateTime.UtcNow - startTime).TotalMilliseconds);
                
                if (errors.Count == 0)
                {
                    _logger.LogInformation($"执行成功: {request.ExecutionName}, 耗时: {duration.TotalMilliseconds:F2}ms");
                    return Automation.ExecutionResult.Success("execution", duration, totalActionsExecuted, 0);
                }
                else
                {
                    _logger.LogWarning($"执行完成但有错误: {request.ExecutionName}, 错误数: {errors.Count}");
                    return Automation.ExecutionResult.Failed("execution", duration, totalActionsExecuted, errors.Count, string.Join(", ", errors));
                }
            }
            catch (Exception ex)
            {
                var duration = Duration.FromMilliseconds((DateTime.UtcNow - startTime).TotalMilliseconds);
                var error = $"执行异常: {ex.Message}";
                _logger.LogError(ex, $"执行失败: {request.ExecutionName}");
                return Automation.ExecutionResult.Failed("execution", duration, 0, 1, error);
            }
        }

        /// <summary>
        /// 执行单个动作
        /// </summary>
        public async Task<ActionResult> ExecuteActionAsync(GameAction action, ExecutionRequest request)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                _logger.LogDebug($"执行动作: {action.GetType().Name}");

                // 执行前置延时
                if (action.Delay.TotalMilliseconds > 0)
                {
                    var delay = action.Delay.GetRandomDelay();
                    await _inputHandler.ExecuteDelay(delay);
                }

                // 执行动作
                var result = action.Execute(_inputHandler);

                if (result.Success)
                {
                    _logger.LogDebug($"动作执行成功: {action.GetType().Name}");
                }
                else
                {
                    _logger.LogWarning($"动作执行失败: {action.GetType().Name} - {result.ErrorMessage}");
                }

                return result;
            }
            catch (Exception ex)
            {
                var error = $"动作执行异常: {ex.Message}";
                _logger.LogError(ex, error);
                return ActionResult.Failed(error);
            }
        }

        /// <summary>
        /// 模拟按键输入
        /// </summary>
        public async Task<ActionResult> SimulateKeyPressAsync(VirtualKeyCode keyCode, Duration pressDuration)
        {
            try
            {
                // 按下按键
                var pressResult = await _inputHandler.ExecuteKeyboardAction(keyCode, KeyState.Press);
                if (!pressResult.Success)
                    return pressResult;

                // 等待指定时间
                await Task.Delay(pressDuration.TotalMilliseconds > 0 ? (int)pressDuration.TotalMilliseconds : 50);

                // 释放按键
                var releaseResult = await _inputHandler.ExecuteKeyboardAction(keyCode, KeyState.Release);
                
                return releaseResult.Success ? ActionResult.Successful() : releaseResult;
            }
            catch (Exception ex)
            {
                return ActionResult.Failed($"按键模拟失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 模拟鼠标移动
        /// </summary>
        public async Task<ActionResult> SimulateMouseMoveAsync(KeyForge.Domain.Common.ScreenLocation targetPosition, Duration moveDuration)
        {
            try
            {
                // 简化实现：直接移动到目标位置
                // 在实际实现中，应该实现平滑的移动轨迹
                var moveResult = await _inputHandler.ExecuteMouseAction(
                    MouseActionType.Move, 
                    MouseButton.Left, 
                    targetPosition
                );

                // 等待移动完成
                await Task.Delay(moveDuration.TotalMilliseconds > 0 ? (int)moveDuration.TotalMilliseconds : 100);

                return moveResult.Success ? ActionResult.Successful() : moveResult;
            }
            catch (Exception ex)
            {
                return ActionResult.Failed($"鼠标移动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 模拟鼠标点击
        /// </summary>
        public async Task<ActionResult> SimulateMouseClickAsync(KeyForge.Domain.Common.ScreenLocation position, MouseButton button, Duration clickDuration)
        {
            try
            {
                // 移动到目标位置
                var moveResult = await SimulateMouseMoveAsync(position, Duration.FromMilliseconds(100));
                if (!moveResult.Success)
                    return moveResult;

                // 按下鼠标按钮
                var pressResult = await _inputHandler.ExecuteMouseAction(
                    MouseActionType.Click,
                    button,
                    position
                );
                
                if (!pressResult.Success)
                    return pressResult;

                // 等待点击完成
                await Task.Delay(clickDuration.TotalMilliseconds > 0 ? (int)clickDuration.TotalMilliseconds : 50);

                return ActionResult.Successful();
            }
            catch (Exception ex)
            {
                return ActionResult.Failed($"鼠标点击失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行组合键
        /// </summary>
        public async Task<ActionResult> SimulateKeyCombinationAsync(List<VirtualKeyCode> keyCodes)
        {
            if (keyCodes == null || keyCodes.Count == 0)
                return ActionResult.Failed("按键列表不能为空");

            try
            {
                // 按下所有按键
                foreach (var keyCode in keyCodes)
                {
                    var result = await _inputHandler.ExecuteKeyboardAction(keyCode, KeyState.Press);
                    if (!result.Success)
                        return result;
                }

                // 短暂延迟
                await Task.Delay(50);

                // 释放所有按键（反向顺序）
                for (int i = keyCodes.Count - 1; i >= 0; i--)
                {
                    var result = await _inputHandler.ExecuteKeyboardAction(keyCodes[i], KeyState.Release);
                    if (!result.Success)
                        return result;
                }

                return ActionResult.Successful();
            }
            catch (Exception ex)
            {
                return ActionResult.Failed($"组合键执行失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行拖拽操作
        /// </summary>
        public async Task<ActionResult> SimulateDragAsync(KeyForge.Domain.Common.ScreenLocation from, KeyForge.Domain.Common.ScreenLocation to, Duration dragDuration)
        {
            try
            {
                // 移动到起始位置
                var moveResult = await SimulateMouseMoveAsync(from, Duration.FromMilliseconds(100));
                if (!moveResult.Success)
                    return moveResult;

                // 按下鼠标左键
                var pressResult = await _inputHandler.ExecuteMouseAction(
                    MouseActionType.DragStart,
                    MouseButton.Left,
                    from
                );
                
                if (!pressResult.Success)
                    return pressResult;

                // 移动到目标位置
                await SimulateMouseMoveAsync(to, dragDuration);

                // 释放鼠标左键
                var releaseResult = await _inputHandler.ExecuteMouseAction(
                    MouseActionType.DragEnd,
                    MouseButton.Left,
                    to
                );

                return releaseResult.Success ? ActionResult.Successful() : releaseResult;
            }
            catch (Exception ex)
            {
                return ActionResult.Failed($"拖拽操作失败: {ex.Message}");
            }
        }

        private async Task<Automation.ExecutionResult> ExecuteActionGroupAsync(ActionGroup group, ExecutionRequest request)
        {
            var groupStartTime = DateTime.UtcNow;
            var actionsExecuted = 0;
            var errors = new List<string>();

            foreach (var action in group.Actions)
            {
                if (request.CancellationToken.IsCancellationRequested)
                    break;

                var result = await ExecuteActionAsync(action, request);
                actionsExecuted++;

                if (!result.Success)
                {
                    errors.Add(result.ErrorMessage ?? "未知错误");
                    
                    if (request.ErrorHandlingStrategy == ErrorHandlingStrategy.StopOnError)
                    {
                        break;
                    }
                }
            }

            var duration = Duration.FromMilliseconds((DateTime.UtcNow - groupStartTime).TotalMilliseconds);
            return errors.Count == 0 
                ? KeyForge.Domain.ValueObjects.ExecutionResult.Success("group", duration, actionsExecuted, new Dictionary<string, object>())
                : KeyForge.Domain.ValueObjects.ExecutionResult.Failed("group", duration, actionsExecuted, errors.Count, string.Join(", ", errors));
        }
    }

    /// <summary>
    /// 执行请求 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record ExecutionRequest(
        string ExecutionName,
        List<GameAction> Actions,
        ErrorHandlingStrategy ErrorHandlingStrategy,
        CancellationToken CancellationToken
    );

    /// <summary>
    /// 错误处理策略
    /// </summary>
    public enum ErrorHandlingStrategy
    {
        StopOnError = 0,
        ContinueOnError = 1,
        RetryOnce = 2,
        SkipAndContinue = 3
    }

    /// <summary>
    /// 执行规划器接口
    /// </summary>
    public interface IExecutionPlanner
    {
        Task<ExecutionPlan> PlanAsync(List<GameAction> actions);
    }

    /// <summary>
    /// 执行计划 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record ExecutionPlan(List<ActionGroup> ActionGroups);

    /// <summary>
    /// 动作组 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record ActionGroup(List<GameAction> Actions, ExecutionStrategy Strategy);

    /// <summary>
    /// 执行策略
    /// </summary>
    public enum ExecutionStrategy
    {
        Sequential = 0,
        Parallel = 1,
        Concurrent = 2
    }
}