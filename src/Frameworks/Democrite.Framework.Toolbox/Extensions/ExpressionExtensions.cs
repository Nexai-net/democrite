// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections.Immutable;
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
        private static readonly IReadOnlyDictionary<MathOperatorEnum, Func<Expression?, Expression, Expression>> s_mathOperatorExpressionBuild;
        private static readonly IReadOnlyDictionary<int, Type> s_functionTemplateTypes;
        private static readonly IReadOnlyDictionary<int, Type> s_actionTemplateTypes;

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

            s_mathOperatorExpressionBuild = new Dictionary<MathOperatorEnum, Func<Expression?, Expression, Expression>>()
            {
                [MathOperatorEnum.Multiply] = (left, right) => Expression.Multiply(left!, right),
                [MathOperatorEnum.Modulo] = (left, right) => Expression.Modulo(left!, right),
                [MathOperatorEnum.Sum] = (left, right) => Expression.Add(left!, right),
                [MathOperatorEnum.Sub] = (left, right) => Expression.Subtract(left!, right),
                [MathOperatorEnum.Divide] = (left, right) => Expression.Divide(left!, right),
            };

            var delegateType = typeof(Delegate);

            s_actionTemplateTypes = typeof(Action).Assembly.GetTypes()
                                                           .Where(t => t.IsAssignableTo(delegateType) &&
                                                                       (t.Name.StartsWith(nameof(Action)) ||
                                                                        t.Name.StartsWith(nameof(Action) + "`")))
                                                           .GroupBy(t => t.GetGenericArguments().Length)
                                                           .ToImmutableDictionary(k => k.Key, v => v.First());

            s_functionTemplateTypes = typeof(Action).Assembly.GetTypes()
                                                             .Where(t => t.IsAssignableTo(delegateType) && t.Name.StartsWith(nameof(Func<int>) + "`"))
                                                             .GroupBy(t => t.GetGenericArguments().Length - 1)
                                                             .ToImmutableDictionary(k => k.Key, v => v.First());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes predicate function
        /// </summary>
        /// <remarks>
        ///     Tolerate only some action to be able to execute the predicte on all condition and different computeur
        /// </remarks>
        public static ConditionExpressionDefinition Serialize<TInputA, TInputB>(this Expression<Func<TInputA, TInputB, bool>> expression)
        {
            return SerializeLambda(expression);
        }

        /// <summary>
        /// Serializes predicate function
        /// </summary>
        /// <remarks>
        ///     Tolerate only some action to be able to execute the predicte on all condition and different computeur
        /// </remarks>
        public static ConditionExpressionDefinition Serialize<TInput>(this Expression<Func<TInput, bool>> expression)
        {
            return SerializeLambda(expression);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="LambdaExpression"/>.
        /// </summary>
        public static LambdaExpression ToExpressionDelegate(this ConditionExpressionDefinition expressionDefinition)
        {
            return ToExpressionDelegate(expressionDefinition, null);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="LambdaExpression"/>.
        /// </summary>
        public static LambdaExpression ToExpressionDelegateWithResult<TResult>(this ConditionExpressionDefinition expressionDefinition)
        {
            return ToExpressionDelegate(expressionDefinition, typeof(TResult));
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="LambdaExpression"/>.
        /// </summary>
        public static LambdaExpression ToExpressionDelegate(this ConditionExpressionDefinition expressionDefinition, Type? returnType = null)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            var inputs = expressionDefinition.Parameters
                                             .Select(p => (Expression.Parameter(p.Type.ToType(), p.Name), definition: p))
                                             .ToDictionary(k => k.Item1, v => v.definition);

            var body = SerializeConditionsToExpression(expressionDefinition.Condition, inputs);

            var orderedInputs = inputs.OrderBy(kv => kv.Value.Order).Select(kv => kv.Key).ToArray();

            Type? delegateType = null;

            var delegateModels = s_actionTemplateTypes;
            var delegateGenericTypes = orderedInputs.Select(s => s.Type).ToArray();

            var hasReturnType = returnType != null && returnType != typeof(void);

            if (hasReturnType)
            {
                delegateModels = s_functionTemplateTypes;
                delegateGenericTypes = delegateGenericTypes.Append(returnType!).ToArray();
            }

            if (delegateModels.TryGetValue(inputs.Count, out var delegateModelType))
                delegateType = delegateModelType.MakeGenericType(delegateGenericTypes);

            if (delegateType == null)
                throw new InvalidDataException("Couldn't found a delegate template model in action or func. May be due to arguments numbers");

            return LambdaExpression.Lambda(delegateType, body, orderedInputs);
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="Expression{Func{TInput, TReturn}}"/>.
        /// </summary>
        public static Expression<Func<TInput, TReturn>> ToExpression<TInput, TReturn>(this ConditionExpressionDefinition expressionDefinition)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            return ToExpressionDelegate<Func<TInput, TReturn>>(expressionDefinition, typeof(TReturn));
        }

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <see cref="Expression{Func{TInput, TReturn}}"/>.
        /// </summary>
        public static Expression<Func<TInputA, TInputB, TReturn>> ToExpression<TInputA, TInputB, TReturn>(this ConditionExpressionDefinition expressionDefinition)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            return ToExpressionDelegate<Func<TInputA, TInputB, TReturn>>(expressionDefinition, typeof(TReturn));
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

            throw new NotImplementedException("Operand not managed " + expressionType);
        }

        /// <summary>
        /// Converts <see cref="ExpressionType"/> to math operator <see cref="MathOperatorEnum"/>
        /// </summary>
        public static MathOperatorEnum ToMathOperand(this ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Subtract:
                    return MathOperatorEnum.Sub;

                case ExpressionType.Multiply:
                    return MathOperatorEnum.Multiply;

                case ExpressionType.Modulo:
                    return MathOperatorEnum.Modulo;

                case ExpressionType.Divide:
                    return MathOperatorEnum.Divide;

                case ExpressionType.Add:
                    return MathOperatorEnum.Sum;
            }

            throw new NotImplementedException("Math operator not managed " + expressionType);
        }

        #region Tools

        /// <summary>
        /// Converts back <see cref="ConditionExpressionDefinition"/> to executable <typeparamref name="TDelegate"/>
        /// </summary>
        private static Expression<TDelegate> ToExpressionDelegate<TDelegate>(this ConditionExpressionDefinition expressionDefinition, Type returnType)
        {
            ArgumentNullException.ThrowIfNull(expressionDefinition);

            var lambdaExpression = ToExpressionDelegate(expressionDefinition, returnType);
            var funcExpression = (Expression<TDelegate>)lambdaExpression;

            return funcExpression;
        }

        /// <summary>
        /// Serializes predicate function
        /// </summary>
        /// <remarks>
        ///     Tolerate only some action to be able to execute the predicte on all condition and different computeur
        /// </remarks>
        private static ConditionExpressionDefinition SerializeLambda(this LambdaExpression expression)
        {
            var inputParameter = expression.Parameters
                                           .Select((p, indx) => (parameter: p, definition: new ConditionParameterDefinition(Guid.NewGuid(), p.Name ?? string.Empty, p.Type.GetAbstractType(), (ushort)indx)))
                                           .ToImmutableDictionary(kv => kv.parameter, kv => kv.definition);

            var condition = SerializeConditions(expression.Body, inputParameter)!;

            return new ConditionExpressionDefinition(inputParameter.Values, condition);
        }

        /// <summary>
        /// Recusive methods that parcour the expression tree and convert it to <see cref="ConditionBaseDefinition"/> tree serializable
        /// </summary>
        private static ConditionBaseDefinition? SerializeConditions(Expression body, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            switch (body.NodeType)
            {
                case ExpressionType.AndAlso:
                    return new ConditionGroupDefinition(LogicEnum.And, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!,
                    });

                case ExpressionType.OrElse:
                    return new ConditionGroupDefinition(LogicEnum.Or, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!,
                    });

                case ExpressionType.ExclusiveOr:
                    return new ConditionGroupDefinition(LogicEnum.ExclusiveOr, new[]
                    {
                        SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions)!,
                        SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!,
                    });

                case ExpressionType.Not:
                    return new ConditionOperandDefinition(null,
                                                          OperandEnum.Not,
                                                          SerializeConditions(((UnaryExpression)body).Operand, sourceInputExpressions)!);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return new ConditionOperandDefinition(SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions),
                                                          body.NodeType.ToOperand(),
                                                          SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!);

                case ExpressionType.Call:
                    return SerializeCallCondition((MethodCallExpression)body, sourceInputExpressions);

                case ExpressionType.MemberAccess:
                    var memberAccess = (MemberExpression)body;

                    if (memberAccess.Expression != null && memberAccess.Expression.NodeType == ExpressionType.Constant)
                    {
                        // with value source is instant must indicate the expression context
                        var context = (ConstantExpression)memberAccess.Expression;
                        var cstValue = context.Type.GetField(memberAccess.Member.Name)?.GetValue(context.Value);
                        return new ConditionValueDefinition(((FieldInfo)memberAccess.Member).FieldType.GetAbstractType(), cstValue);
                    }

                    var instance = SerializeCallInstance(memberAccess.Expression, sourceInputExpressions);
                    return new ConditionMemberAccessDefinition(instance, memberAccess.Member.Name);

                case ExpressionType.Constant:
                    var cst = (ConstantExpression)body;
                    return new ConditionValueDefinition(cst.Type.GetAbstractType(), cst.Value);

                case ExpressionType.Parameter:
                    return SerializeCallInstance(body, sourceInputExpressions);

                case ExpressionType.Add:
                case ExpressionType.Modulo:
                case ExpressionType.Divide:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                    return new ConditionMathOperationDefinition(SerializeConditions(((BinaryExpression)body).Left, sourceInputExpressions),
                                                                body.NodeType.ToMathOperand(),
                                                                SerializeConditions(((BinaryExpression)body).Right, sourceInputExpressions)!);
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
        private static ConditionBaseDefinition SerializeCallCondition(MethodCallExpression body, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            var instance = SerializeCallInstance(body.Object, sourceInputExpressions);
            var arguments = body.Arguments?.Select(a => SerializeConditions(a, sourceInputExpressions)!).ToArray() ?? EnumerableHelper<ConditionBaseDefinition>.ReadOnlyArray;

            return new ConditionCallDefinition(instance,
                                               body.Method.DeclaringType?.FullName + "." + body.Method.Name,
                                               arguments);
        }

        /// <summary>
        /// Serializes the call instance.
        /// </summary>
        private static ConditionBaseDefinition? SerializeCallInstance(Expression? instance, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            if (instance == null)
                return null;

            if (instance is ParameterExpression paramExpression && sourceInputExpressions.TryGetValue(paramExpression, out var paramExpressionDefinition))
                return paramExpressionDefinition;

            // Recursion throught members
            if (instance.NodeType == ExpressionType.MemberAccess)
                return SerializeConditions(instance, sourceInputExpressions);

            throw new NotSupportedException(instance + " is not allowed as Call host.");
        }

        /// <summary>
        /// Convert back <see cref="ConditionBaseDefinition"/> to <see cref="Expression"/>
        /// </summary>
        private static Expression SerializeConditionsToExpression(ConditionBaseDefinition body, IReadOnlyDictionary<ParameterExpression, ConditionParameterDefinition> sourceInputExpressions)
        {
            if (body is ConditionParameterDefinition paramDef)
                return sourceInputExpressions.First(kv => kv.Value == paramDef).Key;

            if (body is ConditionValueDefinition value)
                return Expression.Constant(value.Value, value.Type.ToType());

            if (body is ConditionMemberAccessDefinition member)
            {
                var instance = member.Instance is null ? null : SerializeConditionsToExpression(member.Instance, sourceInputExpressions);

                var memberName = instance?.Type.GetMembers().FirstOrDefault(m => m.Name == member.MemberName);
                ArgumentNullException.ThrowIfNull(memberName);
                return Expression.MakeMemberAccess(instance, memberName);
            }

            if (body is ConditionCallDefinition call)
            {
                var instance = call.Instance is null ? null : SerializeConditionsToExpression(call.Instance, sourceInputExpressions);

                var arguments = call.Arguments?.Select(a => SerializeConditionsToExpression(a, sourceInputExpressions)).ToArray() ?? EnumerableHelper<Expression>.ReadOnlyArray;
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
                var expressions = grp.Conditions.Select(c => SerializeConditionsToExpression(c, sourceInputExpressions)).ToArray();

                var action = s_logicalExpressionBuild[grp.Logic];

                return expressions.Aggregate((left, right) => left == null ? right : action(left, right));
            }

            if (body is ConditionOperandDefinition op)
            {
                var leftExpressions = op.Left is null ? null : SerializeConditionsToExpression(op.Left, sourceInputExpressions);

                Debug.Assert(op.Right is not null);
                var rightExpressions = SerializeConditionsToExpression(op.Right, sourceInputExpressions);

                var action = s_operandExpressionBuild[op.Operand];

                return action(leftExpressions, rightExpressions);
            }

            if (body is ConditionMathOperationDefinition mathOp)
            {
                var leftExpressions = mathOp.Left is null ? null : SerializeConditionsToExpression(mathOp.Left, sourceInputExpressions);

                Debug.Assert(mathOp.Right is not null);
                var rightExpressions = SerializeConditionsToExpression(mathOp.Right, sourceInputExpressions);

                var action = s_mathOperatorExpressionBuild[mathOp.MathOperator];

                return action(leftExpressions, rightExpressions);
            }

            throw new NotSupportedException("Serialize expression restoration not restorable");
        }

        #endregion

        #endregion
    }
}
