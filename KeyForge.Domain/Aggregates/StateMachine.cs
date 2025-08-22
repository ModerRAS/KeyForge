using System;
using System.Collections.Generic;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Events;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Aggregates
{
    /// <summary>
    /// 状态机聚合根
    /// </summary>
    public class StateMachine : AggregateRoot
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public StateMachineStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public int Version { get; private set; }

        private readonly List<State> _states = new();
        public IReadOnlyCollection<State> States => _states.AsReadOnly();

        private readonly List<StateTransition> _transitions = new();
        public IReadOnlyCollection<StateTransition> Transitions => _transitions.AsReadOnly();

        private readonly List<DecisionRule> _rules = new();
        public IReadOnlyCollection<DecisionRule> Rules => _rules.AsReadOnly();

        private Guid _currentStateId;
        public State CurrentState => _states.Find(s => s.Id == _currentStateId);

        protected StateMachine() { }

        public StateMachine(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
            Status = StateMachineStatus.Draft;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Version = 1;

            // 创建初始状态
            var initialState = new State(Guid.NewGuid(), "Initial", "Initial state");
            _states.Add(initialState);
            _currentStateId = initialState.Id;

            AddDomainEvent(new StateMachineCreatedEvent(Id, Name));
        }

        public void AddState(State state)
        {
            if (state == null)
                throw new ValidationException("State cannot be null.");

            if (Status != StateMachineStatus.Draft)
                throw new BusinessRuleViolationException("Can only add states to draft state machines.");

            if (_states.Any(s => s.Name == state.Name))
                throw new BusinessRuleViolationException($"State with name '{state.Name}' already exists.");

            _states.Add(state);
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void AddTransition(StateTransition transition)
        {
            if (transition == null)
                throw new ValidationException("Transition cannot be null.");

            if (Status != StateMachineStatus.Draft)
                throw new BusinessRuleViolationException("Can only add transitions to draft state machines.");

            if (!_states.Any(s => s.Id == transition.FromStateId))
                throw new BusinessRuleViolationException("From state does not exist.");

            if (!_states.Any(s => s.Id == transition.ToStateId))
                throw new BusinessRuleViolationException("To state does not exist.");

            _transitions.Add(transition);
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void AddRule(DecisionRule rule)
        {
            if (rule == null)
                throw new ValidationException("Rule cannot be null.");

            if (Status != StateMachineStatus.Draft)
                throw new BusinessRuleViolationException("Can only add rules to draft state machines.");

            _rules.Add(rule);
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void TransitionTo(Guid toStateId, string reason = null)
        {
            if (!_states.Any(s => s.Id == toStateId))
                throw new BusinessRuleViolationException("Target state does not exist.");

            if (_currentStateId == toStateId)
                return;

            var fromStateId = _currentStateId;
            _currentStateId = toStateId;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new StateTransitionEvent(Id, fromStateId, toStateId, reason));
        }

        public bool CanTransitionTo(Guid toStateId)
        {
            return _transitions.Any(t => t.FromStateId == _currentStateId && t.ToStateId == toStateId);
        }

        public IEnumerable<StateTransition> GetAvailableTransitions()
        {
            return _transitions.Where(t => t.FromStateId == _currentStateId);
        }

        public void EvaluateRules()
        {
            if (Status != StateMachineStatus.Active)
                return;

            var currentState = CurrentState;
            if (currentState == null)
                return;

            var activeRules = _rules.Where(r => r.IsActive).OrderBy(r => r.Priority);

            foreach (var rule in activeRules)
            {
                if (rule.Evaluate(key => currentState.GetVariable(key)))
                {
                    // 找到匹配的规则，触发相应的动作
                    AddDomainEvent(new RuleTriggeredEvent(Id, rule.Id, rule.Name));
                }
            }
        }

        public void Activate()
        {
            if (Status != StateMachineStatus.Draft)
                throw new BusinessRuleViolationException("Only draft state machines can be activated.");

            if (_states.Count < 2)
                throw new BusinessRuleViolationException("State machine must have at least 2 states.");

            if (_transitions.Count == 0)
                throw new BusinessRuleViolationException("State machine must have at least 1 transition.");

            Status = StateMachineStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new StateMachineActivatedEvent(Id, Name));
        }

        public void Deactivate()
        {
            if (Status != StateMachineStatus.Active)
                throw new BusinessRuleViolationException("Only active state machines can be deactivated.");

            Status = StateMachineStatus.Inactive; // 使用Common枚举中的Inactive替代Paused
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new StateMachineDeactivatedEvent(Id, Name));
        }

        public void Reset()
        {
            var initialState = _states.FirstOrDefault(s => s.Name == "Initial");
            if (initialState != null)
            {
                _currentStateId = initialState.Id;
                UpdatedAt = DateTime.UtcNow;
                Version++;

                AddDomainEvent(new StateMachineResetEvent(Id, Name));
            }
        }
    }

    /// <summary>
    /// 状态转换实体
    /// </summary>
    public class StateTransition : Entity
    {
        public Guid FromStateId { get; private set; }
        public Guid ToStateId { get; private set; }
        public ConditionExpression Condition { get; private set; }
        public string Description { get; private set; }

        protected StateTransition() { }

        public StateTransition(Guid id, Guid fromStateId, Guid toStateId, 
            ConditionExpression condition = null, string description = null)
        {
            Id = id;
            FromStateId = fromStateId;
            ToStateId = toStateId;
            Condition = condition;
            Description = description;
        }
    }

    }