using System;
using System.Collections.Generic;
using System.Linq;
using KeyForge.Domain.Entities;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 动作序列值对象
    /// </summary>
    public class ActionSequence : ValueObject
    {
        public IReadOnlyCollection<GameAction> Actions { get; }
        public int TotalDuration { get; }
        public int ActionCount { get; }

        public ActionSequence(IEnumerable<GameAction> actions)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            var actionList = actions.ToList();
            Actions = actionList.AsReadOnly();
            ActionCount = actionList.Count;
            TotalDuration = actionList.Sum(a => a.Delay);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            foreach (var action in Actions)
            {
                yield return action;
            }
        }

        public ActionSequence AddAction(GameAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var newActions = new List<GameAction>(Actions) { action };
            return new ActionSequence(newActions);
        }

        public ActionSequence RemoveAction(Guid actionId)
        {
            var action = Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null)
                throw new ArgumentException($"Action with id {actionId} not found.");

            var newActions = Actions.Where(a => a.Id != actionId).ToList();
            return new ActionSequence(newActions);
        }

        public ActionSequence InsertAction(int index, GameAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (index < 0 || index > Actions.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var newActions = new List<GameAction>(Actions);
            newActions.Insert(index, action);
            return new ActionSequence(newActions);
        }

        public ActionSequence ReorderActions(IEnumerable<Guid> orderedActionIds)
        {
            var actionDict = Actions.ToDictionary(a => a.Id);
            var newActions = orderedActionIds
                .Where(id => actionDict.ContainsKey(id))
                .Select(id => actionDict[id])
                .ToList();

            return new ActionSequence(newActions);
        }
    }
}