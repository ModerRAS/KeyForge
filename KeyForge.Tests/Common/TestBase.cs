using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;

namespace KeyForge.Tests.Common
{
    /// <summary>
    /// 测试基类 - 提供通用的测试设置和断言方法
    /// </summary>
    public abstract class TestBase
    {
        #region 通用断言方法

        protected static void ShouldThrowValidationException(Action action, string expectedMessage)
        {
            var exception = Assert.Throws<ValidationException>(action);
            exception.Message.Should().Be(expectedMessage);
        }

        protected static void ShouldThrowBusinessRuleViolationException(Action action, string expectedMessage)
        {
            var exception = Assert.Throws<BusinessRuleViolationException>(action);
            exception.Message.Should().Be(expectedMessage);
        }

        protected static void ShouldThrowEntityNotFoundException<T>(Action action, string expectedEntityName, Guid expectedId)
        {
            var exception = Assert.Throws<EntityNotFoundException>(action);
            exception.EntityName.Should().Be(expectedEntityName);
            exception.EntityId.Should().Be(expectedId);
        }

        protected static void ShouldThrowArgumentException(Action action, string expectedParamName)
        {
            var exception = Assert.Throws<ArgumentException>(action);
            exception.ParamName.Should().Be(expectedParamName);
        }

        #endregion

        #region 脚本相关断言

        protected static void ShouldBeValidScript(Script script)
        {
            script.Should().NotBeNull();
            script.Id.Should().NotBeEmpty();
            script.Name.Should().NotBeNullOrEmpty();
            script.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            script.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            script.Version.Should().BeGreaterThan(0);
        }

        protected static void ShouldBeInStatus(Script script, ScriptStatus expectedStatus)
        {
            script.Status.Should().Be(expectedStatus);
        }

        protected static void ShouldHaveActions(Script script, int expectedCount)
        {
            script.Actions.Should().HaveCount(expectedCount);
        }

        protected static void ShouldContainAction(Script script, ActionType expectedType)
        {
            script.Actions.Should().Contain(a => a.Type == expectedType);
        }

        protected static void ShouldHaveEstimatedDuration(Script script, TimeSpan expectedMinDuration)
        {
            script.GetEstimatedDuration().Should().BeGreaterThanOrEqualTo(expectedMinDuration);
        }

        #endregion

        #region 状态机相关断言

        protected static void ShouldBeValidStateMachine(StateMachine stateMachine)
        {
            stateMachine.Should().NotBeNull();
            stateMachine.Id.Should().NotBeEmpty();
            stateMachine.Name.Should().NotBeNullOrEmpty();
            stateMachine.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            stateMachine.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            stateMachine.Version.Should().BeGreaterThan(0);
            stateMachine.CurrentState.Should().NotBeNull();
        }

        protected static void ShouldBeInStatus(StateMachine stateMachine, StateMachineStatus expectedStatus)
        {
            stateMachine.Status.Should().Be(expectedStatus);
        }

        protected static void ShouldHaveStates(StateMachine stateMachine, int expectedCount)
        {
            stateMachine.States.Should().HaveCount(expectedCount);
        }

        protected static void ShouldHaveTransitions(StateMachine stateMachine, int expectedCount)
        {
            stateMachine.Transitions.Should().HaveCount(expectedCount);
        }

        protected static void ShouldHaveRules(StateMachine stateMachine, int expectedCount)
        {
            stateMachine.Rules.Should().HaveCount(expectedCount);
        }

        protected static void ShouldBeInState(StateMachine stateMachine, string expectedStateName)
        {
            stateMachine.CurrentState.Name.Should().Be(expectedStateName);
        }

        protected static void ShouldAllowTransition(StateMachine stateMachine, Guid toStateId)
        {
            stateMachine.CanTransitionTo(toStateId).Should().BeTrue();
        }

        protected static void ShouldNotAllowTransition(StateMachine stateMachine, Guid toStateId)
        {
            stateMachine.CanTransitionTo(toStateId).Should().BeFalse();
        }

        #endregion

        #region 图像模板相关断言

        protected static void ShouldBeValidImageTemplate(ImageTemplate template)
        {
            template.Should().NotBeNull();
            template.Id.Should().NotBeEmpty();
            template.Name.Should().NotBeNullOrEmpty();
            template.TemplateData.Should().NotBeNull().And.NotBeEmpty();
            template.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            template.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            template.Version.Should().BeGreaterThan(0);
            template.MatchThreshold.Should().BeInRange(0.0, 1.0);
        }

        protected static void ShouldBeActive(ImageTemplate template)
        {
            template.IsActive.Should().BeTrue();
        }

        protected static void ShouldBeInactive(ImageTemplate template)
        {
            template.IsActive.Should().BeFalse();
        }

        protected static void ShouldMatchTemplate(ImageTemplate template, RecognitionResult result)
        {
            template.IsMatch(result).Should().BeTrue();
        }

        protected static void ShouldNotMatchTemplate(ImageTemplate template, RecognitionResult result)
        {
            template.IsMatch(result).Should().BeFalse();
        }

        #endregion

        #region 游戏动作相关断言

        protected static void ShouldBeValidGameAction(GameAction action)
        {
            action.Should().NotBeNull();
            action.Id.Should().NotBeEmpty();
            action.Type.Should().BeDefined();
            action.Timestamp.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }

        protected static void ShouldBeKeyAction(GameAction action, KeyCode expectedKey)
        {
            action.Type.Should().BeOneOf(ActionType.KeyDown, ActionType.KeyUp);
            action.Key.Should().Be(expectedKey);
        }

        protected static void ShouldBeMouseAction(GameAction action, MouseButton expectedButton)
        {
            action.Type.Should().BeOneOf(ActionType.MouseDown, ActionType.MouseUp, ActionType.MouseMove);
            action.Button.Should().Be(expectedButton);
        }

        protected static void ShouldBeDelayAction(GameAction action, int expectedDelay)
        {
            action.Type.Should().Be(ActionType.Delay);
            action.Delay.Should().Be(expectedDelay);
        }

        #endregion

        #region 决策规则相关断言

        protected static void ShouldBeValidDecisionRule(DecisionRule rule)
        {
            rule.Should().NotBeNull();
            rule.Id.Should().NotBeEmpty();
            rule.Name.Should().NotBeNullOrEmpty();
            rule.Condition.Should().NotBeNull();
            rule.Priority.Should().BeGreaterThan(0);
        }

        protected static void ShouldBeActive(DecisionRule rule)
        {
            rule.IsActive.Should().BeTrue();
        }

        protected static void ShouldBeInactive(DecisionRule rule)
        {
            rule.IsActive.Should().BeFalse();
        }

        #endregion

        #region 异步测试辅助方法

        protected static async Task ShouldThrowValidationExceptionAsync(Func<Task> action, string expectedMessage)
        {
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.Message.Should().Be(expectedMessage);
        }

        protected static async Task ShouldThrowBusinessRuleViolationExceptionAsync(Func<Task> action, string expectedMessage)
        {
            var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(action);
            exception.Message.Should().Be(expectedMessage);
        }

        protected static async Task ShouldThrowEntityNotFoundExceptionAsync<T>(Func<Task> action, string expectedEntityName, Guid expectedId)
        {
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(action);
            exception.EntityName.Should().Be(expectedEntityName);
            exception.EntityId.Should().Be(expectedId);
        }

        protected static async Task ShouldThrowArgumentExceptionAsync(Func<Task> action, string expectedParamName)
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(action);
            exception.ParamName.Should().Be(expectedParamName);
        }

        #endregion

        #region 集合断言

        protected static void ShouldBeEquivalent<T>(IEnumerable<T> actual, IEnumerable<T> expected)
        {
            actual.Should().BeEquivalentTo(expected);
        }

        protected static void ShouldContain<T>(IEnumerable<T> collection, T expected)
        {
            collection.Should().Contain(expected);
        }

        protected static void ShouldNotContain<T>(IEnumerable<T> collection, T expected)
        {
            collection.Should().NotContain(expected);
        }

        protected static void ShouldHaveCount<T>(IEnumerable<T> collection, int expectedCount)
        {
            collection.Should().HaveCount(expectedCount);
        }

        protected static void ShouldBeEmpty<T>(IEnumerable<T> collection)
        {
            collection.Should().BeEmpty();
        }

        protected static void ShouldNotBeEmpty<T>(IEnumerable<T> collection)
        {
            collection.Should().NotBeEmpty();
        }

        #endregion

        #region 日期时间断言

        protected static void ShouldBeRecent(DateTime dateTime, TimeSpan maxAge)
        {
            var age = DateTime.UtcNow - dateTime;
            age.Should().BeLessThanOrEqualTo(maxAge);
        }

        protected static void ShouldBeBefore(DateTime dateTime, DateTime reference)
        {
            dateTime.Should().BeBefore(reference);
        }

        protected static void ShouldBeAfter(DateTime dateTime, DateTime reference)
        {
            dateTime.Should().BeAfter(reference);
        }

        #endregion

        #region 数值断言

        protected static void ShouldBeInRange<T>(T value, T min, T max) where T : IComparable<T>
        {
            value.Should().BeInRange(min, max);
        }

        protected static void ShouldBeGreaterThan<T>(T value, T expected) where T : IComparable<T>
        {
            value.Should().BeGreaterThan(expected);
        }

        protected static void ShouldBeLessThan<T>(T value, T expected) where T : IComparable<T>
        {
            value.Should().BeLessThan(expected);
        }

        protected static void ShouldBePositive<T>(T value) where T : IComparable<T>
        {
            value.Should().BeGreaterThan(default(T));
        }

        #endregion
    }
}