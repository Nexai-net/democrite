// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Statements
{
    using Democrite.Framework.Toolbox.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Patterns.Tree;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal delegate bool BoolLogicStatementComputation(params bool[] args);

    internal sealed class BoolLogicStatement : IBoolLogicStatement
    {
        #region Fields

        private readonly BoolLogicStatementComputation _computeExpression;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolLogicStatement"/> class.
        /// </summary>
        public BoolLogicStatement(BoolLogicStatementComputation computeExpression)
        {
            this._computeExpression = computeExpression;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Ask(in ReadOnlySpan<bool> inputs)
        {
            // Convert span to array until it is possible to pass span to lambda
            return this._computeExpression(inputs.ToArray());
        }

        /// <summary>
        /// Creates <see cref="BoolLogicStatement"/> from the split result of <see cref="ExpressionBuilder"/>
        /// </summary>
        internal static IBoolLogicStatement Create(TreeNode<string> expression, IReadOnlyList<string> variables)
        {
            var paramExpression = Expression.Parameter(typeof(bool[]), "args");

            var executeExpression = CreateExpression(expression, variables, paramExpression);

            return new BoolLogicStatement(executeExpression.Compile());
        }

        #region Tools

        /// <summary>
        /// Creates <see cref="BoolLogicStatement"/> from the split result of <see cref="ExpressionBuilder"/>
        /// </summary>
        private static Expression<BoolLogicStatementComputation> CreateExpression(TreeNode<string> expression, IReadOnlyList<string> variables, ParameterExpression paramExpression)
        {
            var indexedVariable = variables.Select((v, i) => (v, i))
                                           .ToDictionary(kv => kv.v,
                                                         kv => (Expression)(Expression.ArrayAccess(paramExpression, Expression.Constant(kv.i))),
                                                         StringComparer.OrdinalIgnoreCase);

            int childIndx = 0;
            foreach (var child in expression.Children)
            {
                indexedVariable.Add("$" + childIndx, CreateExpression(child, variables, paramExpression).Body);
                childIndx++;
            }

            var parts = new Queue<string>(expression.Entity.Length);

#if NET8_0_OR_GREATER

#warning Use SearchValue to locate operator in the expression
#warning Use ReadOnlySpan<> extension method SPLIT

#endif
            ReadOnlySpan<char> operators = LogicHelper.LogicOperatorString;
            ReadOnlySpan<char> entityStr = expression.Entity;

            var operatorIndex = entityStr.IndexOfAny(operators);
            while (operatorIndex > -1)
            {
                if (operatorIndex > 0)
                    parts.Enqueue(entityStr.Slice(0, operatorIndex).ToString());

                parts.Enqueue(entityStr.Slice(operatorIndex, 1).ToString());

                entityStr = entityStr.Slice(operatorIndex + 1);
                operatorIndex = entityStr.IndexOfAny(operators);
            }

            if (entityStr.Length > 0)
                parts.Enqueue(entityStr.ToString());

            Expression? currentExpression = null;
            var stackOperators = new Stack<LogicEnum>();

            while (parts.Count > 0)
            {
                var current = parts.Dequeue();
                if (indexedVariable.TryGetValue(current, out var varExpression))
                {
                    if (currentExpression == null && stackOperators.Count == 0)
                    {
                        currentExpression = varExpression;
                        continue;
                    }

                    var opToApply = stackOperators.Pop();

                    if (opToApply == LogicEnum.Not)
                    {
                        var notExpression = Expression.Not(varExpression);
                        if (currentExpression == null)
                        {
                            currentExpression = notExpression;
                            continue;
                        }

                        varExpression = notExpression;
                        opToApply = stackOperators.Pop();
                    }

                    if (currentExpression == null)
                        throw new NotImplementedException("Unknow how to generate bool expressionPart with '" + current + "'");

                    switch (opToApply)
                    {
                        case LogicEnum.And:
                            currentExpression = Expression.AndAlso(currentExpression, varExpression);
                            continue;

                        case LogicEnum.Or:
                            currentExpression = Expression.OrElse(currentExpression, varExpression);
                            continue;

                        case LogicEnum.ExclusiveOr:
                            currentExpression = Expression.ExclusiveOr(currentExpression, varExpression);
                            continue;
                    }
                }
                else if (current.Length == 1)
                {
                    var currentOp = LogicHelper.GetLogicValueFrom(current[0]);

                    if (currentOp != null && currentOp.Value != LogicEnum.None)
                    {
                        stackOperators.Push(currentOp.Value);
                        continue;
                    }
                }

                throw new NotImplementedException("Unknow how to generate bool expressionPart by '" + current + "'");
            }

            return Expression.Lambda<BoolLogicStatementComputation>(currentExpression ?? Expression.Constant(true), paramExpression);
        }

        #endregion

        #endregion
    }
}
