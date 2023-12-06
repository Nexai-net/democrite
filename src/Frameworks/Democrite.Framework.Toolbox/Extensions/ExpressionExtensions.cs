// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Extension around expression
    /// </summary>
    public static class ExpressionExtensions
    {
        #region Fields

        private static readonly IReadOnlyDictionary<LogicEnum, Func<Expression, Expression, Expression>> s_logicalExpressionBuild;
        private static readonly IReadOnlyDictionary<OperandEnum, Func<Expression?, Expression, Expression>> s_operandExpressionBuild;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize class <see cref="ExpressionExtensions"/>
        /// </summary>
        static ExpressionExtensions()
        {
            s_logicalExpressionBuild = new Dictionary<LogicEnum, Func<Expression, Expression, Expression>>()
            {
                [LogicEnum.Or] = (left, right) => Expression.OrElse(left, right),
                [LogicEnum.And] = (left, right) => Expression.AndAlso(left, right),
                [LogicEnum.ExclusiveOr] = (left, right) => Expression.ExclusiveOr(left, right),
            };

            s_operandExpressionBuild = new Dictionary<OperandEnum, Func<Expression?, Expression, Expression>>()
            {
                [OperandEnum.GreaterThan] = (left, right) => Expression.GreaterThan(left!, right),
                [OperandEnum.GreaterOrEqualThan] = (left, right) => Expression.GreaterThanOrEqual(left!, right),
                [OperandEnum.LesserThan] = (left, right) => Expression.LessThan(left!, right),
                [OperandEnum.LesserOrEqualThan] = (left, right) => Expression.LessThanOrEqual(left!, right),
                [OperandEnum.Equal] = (left, right) => Expression.Equal(left!, right),
                [OperandEnum.NotEqual] = (left, right) => Expression.NotEqual(left!, right),
                [OperandEnum.Different] = (left, right) => Expression.NotEqual(left!, right),
                [OperandEnum.Not] = (_, right) => Expression.Not(right),
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes predicate function
        /// </summary>
        /// <remarks>
        ///     Tolerate only some action to be able to execute the predicte on all condition and different computeur
        /// </remarks>
        public static ConditionExpressionDefinition Serialize<TInput>(this Expression<Func<TInput, bool>> expression)
        {
            var inputParameter = expression.Parameters.Single();

            var condition = SerializeConditions(expression.Body, inputParameter)!;

            return new ConditionExpressionDefinition(inputParameter.Name!, inputParameter.Type, condition);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="Expression{Func{TInput, TReturn}}"/>.
        /// </summary>
        public static Expression<Func<TInput, TReturn>> ToExpression<TInput, TReturn>(this ConditionExpressionDefinition expressionDefinition)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            var input = Expression.Parameter(expressionDefinition.InputType, expressionDefinition.InputName);

            var lambda = Expression.Lambda<Func<TInput, TReturn>>(SerializeConditionsToExpression(expressionDefinition.Condition, input), input);

            return lambda;
        }

        /// <summary>
        /// Converts <see cref="ExpressionType"/> to operand <see cref="OperandEnum"/>
        /// </summary>
        public static OperandEnum ToOperand(this ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return OperandEnum.Equal;

                case ExpressionType.NotEqual:
                    return OperandEnum.NotEqual;

                case ExpressionType.GreaterThan:
                    return OperandEnum.GreaterThan;

                case ExpressionType.GreaterThanOrEqual:
                    return OperandEnum.GreaterOrEqualThan;

                case ExpressionType.LessThan:
                    return OperandEnum.LesserThan;

                case ExpressionType.LessThanOrEqual:
                    return OperandEnum.LesserOrEqualThan;

                case ExpressionType.Not:
                    return OperandEnum.Not;
            }

            throw new NotImplementedException();
        }

        #region Tools

        /// <summary>
        /// Recusive methods that parcour the expression tree and convert it to <see cref="ConditionBaseDefinition"/> tree serializable
        /// </summary>
        private static ConditionBaseDefinition? SerializeConditions(Expression body, ParameterExpression sourceInputExpression)
        {
            switch (body.NodeType)
            {
                case ExpressionType.AndAlso:
                    return new ConditionGroupDefinition(LogicEnum.And, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpression)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpression)!,
                    });

                case ExpressionType.OrElse:
                    return new ConditionGroupDefinition(LogicEnum.Or, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpression)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpression)!,
                    });

                case ExpressionType.ExclusiveOr:
                    return new ConditionGroupDefinition(LogicEnum.ExclusiveOr, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpression)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpression)!,
                    });

                case ExpressionType.Not:
                    return new ConditionOperandDefinition(null,
                                                          OperandEnum.Not,
                                                          SerializeConditions(((UnaryExpression)body).Operand, sourceInputExpression)!);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return new ConditionOperandDefinition(SerializeConditions(((BinaryExpression)body).Left, sourceInputExpression),
                                                          body.NodeType.ToOperand(),
                                                          SerializeConditions(((BinaryExpression)body).Right, sourceInputExpression)!);

                case ExpressionType.Call:
                    return SerializeCallCondition((MethodCallExpression)body, sourceInputExpression);

                case ExpressionType.MemberAccess:
                    var memberAccess = (MemberExpression)body;

                    if (memberAccess.Expression != null && memberAccess.Expression.NodeType == ExpressionType.Constant)
                    {
                        // with value source is instant must indicate the expression context
                        var context = (ConstantExpression)memberAccess.Expression;
                        var cstValue = context.Type.GetField(memberAccess.Member.Name)?.GetValue(context.Value);
                        return new ConditionValueDefinition(((FieldInfo)memberAccess.Member).FieldType, cstValue);
                    }

                    var instance = SerializeCallInstance(memberAccess.Expression, sourceInputExpression);
                    return new ConditionMemberAccessDefinition(instance, memberAccess.Member.Name);

                case ExpressionType.Constant:
                    var cst = (ConstantExpression)body;
                    return new ConditionValueDefinition(cst.Type, cst.Value);

                case ExpressionType.Parameter:
                    return SerializeCallInstance(body, sourceInputExpression);

                case ExpressionType.And:
                    throw new NotImplementedException(body.NodeType.ToString());

                case ExpressionType.Or:
                    throw new NotImplementedException(body.NodeType.ToString());

                default:
                    throw new NotImplementedException("Action not tolerate in condition serialization : " + body.NodeType);
            }
        }

        /// <summary>
        /// Serializes the call condition.
        /// </summary>
        private static ConditionBaseDefinition SerializeCallCondition(MethodCallExpression body, ParameterExpression sourceInputExpression)
        {
            var instance = SerializeCallInstance(body.Object, sourceInputExpression);
            var arguments = body.Arguments?.Select(a => SerializeConditions(a, sourceInputExpression)!).ToArray() ?? EnumerableHelper<ConditionBaseDefinition>.ReadOnlyArray;

            return new ConditionCallDefinition(instance,
                                               body.Method.DeclaringType?.FullName + "." + body.Method.Name,
                                               arguments);
        }

        /// <summary>
        /// Serializes the call instance.
        /// </summary>
        private static ConditionBaseDefinition? SerializeCallInstance(Expression? instance, ParameterExpression sourceInputExpression)
        {
            if (instance == null)
                return null;

            if (instance == sourceInputExpression)
                return new ConditionExpressionParameterDefinition();

            // Recursion throught members
            if (instance.NodeType == ExpressionType.MemberAccess)
                return SerializeConditions(instance, sourceInputExpression);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert back <see cref="ConditionBaseDefinition"/> to <see cref="Expression"/>
        /// </summary>
        private static Expression SerializeConditionsToExpression(ConditionBaseDefinition body, ParameterExpression inputParameter)
        {
            if (body is ConditionExpressionParameterDefinition)
                return inputParameter;

            if (body is ConditionValueDefinition value)
                return Expression.Constant(value.Value, value.Type);

            if (body is ConditionMemberAccessDefinition member)
            {
                var instance = member.Instance is null ? null : SerializeConditionsToExpression(member.Instance, inputParameter);

                var memberName = instance?.Type.GetMembers().FirstOrDefault(m => m.Name == member.MemberName);
                ArgumentNullException.ThrowIfNull(memberName);
                return Expression.MakeMemberAccess(instance, memberName);
            }

            if (body is ConditionCallDefinition call)
            {
                var instance = call.Instance is null ? null : SerializeConditionsToExpression(call.Instance, inputParameter);

                var arguments = call.Arguments?.Select(a => SerializeConditionsToExpression(a, inputParameter)).ToArray() ?? EnumerableHelper<Expression>.ReadOnlyArray;
                var argumentTypes = arguments.Select(a => a.Type).ToArray();

                var callPart = call.MethodName.Split('.');
                var typeName = string.Join('.', callPart.SkipLast(1));
                var methodDeclarationType = Type.GetType(typeName) ?? throw new InvalidOperationException("Could not found type with following information '" + typeName + "'");

                string methodName = callPart.Last();

                var mthd = methodDeclarationType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                                                .Where(m => m.Name == methodName &&
                                                            m.GetParameters().Length == arguments.Length &&
                                                            m.GetParameters()
                                                             .Select((p, indx) => p.ParameterType.IsAssignableFrom(argumentTypes[indx]))
                                                             .All(t => t))
                                                .First();

                return Expression.Call(instance, mthd, arguments);
            }

            if (body is ConditionGroupDefinition grp)
            {
                var expressions = grp.Conditions.Select(c => SerializeConditionsToExpression(c, inputParameter)).ToArray();

                var action = s_logicalExpressionBuild[grp.Logic];

                return expressions.Aggregate((left, right) => left == null ? right : action(left, right));
            }

            if (body is ConditionOperandDefinition op)
            {
                var leftExpressions = op.Left is null ? null : SerializeConditionsToExpression(op.Left, inputParameter);

                Debug.Assert(op.Right is not null);
                var rightExpressions = SerializeConditionsToExpression(op.Right, inputParameter);

                var action = s_operandExpressionBuild[op.Operand];

                return action(leftExpressions, rightExpressions);
            }

            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
