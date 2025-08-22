using System;
using System.Collections.Generic;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 条件表达式值对象
    /// </summary>
    public class ConditionExpression : ValueObject
    {
        public string LeftOperand { get; }
        public ComparisonOperator Operator { get; }
        public string RightOperand { get; }
        public LogicalOperator LogicalOperator { get; }
        public ConditionExpression NextCondition { get; }

        public ConditionExpression(string leftOperand, ComparisonOperator op, string rightOperand)
        {
            if (string.IsNullOrWhiteSpace(leftOperand))
                throw new ArgumentException("Left operand cannot be empty.", nameof(leftOperand));
            if (string.IsNullOrWhiteSpace(rightOperand))
                throw new ArgumentException("Right operand cannot be empty.", nameof(rightOperand));

            LeftOperand = leftOperand;
            Operator = op;
            RightOperand = rightOperand;
            LogicalOperator = LogicalOperator.None;
            NextCondition = null;
        }

        public ConditionExpression(string leftOperand, ComparisonOperator op, string rightOperand, 
            LogicalOperator logicalOperator, ConditionExpression nextCondition)
        {
            if (string.IsNullOrWhiteSpace(leftOperand))
                throw new ArgumentException("Left operand cannot be empty.", nameof(leftOperand));
            if (string.IsNullOrWhiteSpace(rightOperand))
                throw new ArgumentException("Right operand cannot be empty.", nameof(rightOperand));
            if (logicalOperator != LogicalOperator.None && nextCondition == null)
                throw new ArgumentException("Next condition is required when logical operator is specified.", nameof(nextCondition));

            LeftOperand = leftOperand;
            Operator = op;
            RightOperand = rightOperand;
            LogicalOperator = logicalOperator;
            NextCondition = nextCondition;
        }

        public bool Evaluate(Func<string, object> valueResolver)
        {
            var leftValue = valueResolver(LeftOperand);
            var rightValue = valueResolver(RightOperand);

            var result = Compare(leftValue, rightValue, Operator);

            if (LogicalOperator == LogicalOperator.None || NextCondition == null)
                return result;

            var nextResult = NextCondition.Evaluate(valueResolver);

            return LogicalOperator == LogicalOperator.And ? result && nextResult : result || nextResult;
        }

        private bool Compare(object left, object right, ComparisonOperator op)
        {
            if (left == null || right == null)
                return false;

            switch (op)
            {
                case ComparisonOperator.Equal:
                    return left.Equals(right);
                case ComparisonOperator.NotEqual:
                    return !left.Equals(right);
                case ComparisonOperator.GreaterThan:
                    return Convert.ToDouble(left) > Convert.ToDouble(right);
                case ComparisonOperator.GreaterThanOrEqual:
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);
                case ComparisonOperator.LessThan:
                    return Convert.ToDouble(left) < Convert.ToDouble(right);
                case ComparisonOperator.LessThanOrEqual:
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);
                case ComparisonOperator.Contains:
                    return left.ToString().Contains(right.ToString());
                case ComparisonOperator.StartsWith:
                    return left.ToString().StartsWith(right.ToString());
                case ComparisonOperator.EndsWith:
                    return left.ToString().EndsWith(right.ToString());
                default:
                    return false;
            }
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return LeftOperand;
            yield return Operator;
            yield return RightOperand;
            yield return LogicalOperator;
            yield return NextCondition;
        }
    }

    /// <summary>
    /// 比较操作符枚举
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith
    }

    /// <summary>
    /// 逻辑操作符枚举
    /// </summary>
    public enum LogicalOperator
    {
        None,
        And,
        Or
    }
}