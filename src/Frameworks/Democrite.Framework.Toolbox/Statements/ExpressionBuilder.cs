// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Statements
{
    using Democrite.Framework.Toolbox.Patterns.Tree;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public sealed class ExpressionBuilder
    {
        #region Fields

        private static readonly Regex s_formulaCharNotAlphabeticalAllowed = new Regex(@"^[!&|^\(\)\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_formulaCharAllowed = new Regex(@"^[a-zA-Z!&|^\(\)\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_variableCharAllowed = new Regex(@"^[a-zA-Z]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_spaceRegex = new Regex(@"\s", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion

        #region  Methods

        /// <summary>
        /// Build logical statement A & B
        /// </summary>
        /// <remarks>
        ///     Operator must came from <see cref="LogicEnum"/> alias
        /// </remarks>
        /// <exception cref="InvalidDataException" />
        public static IBoolLogicStatement BuildBoolLogicStatement(string statement, IReadOnlyList<string> variableNames)
        {
            if (!CanBuildBoolLogicStatement(statement, variableNames, out var reason))
                throw new InvalidDataException(reason);

            // Trim
            var sourceSpan = s_spaceRegex.Replace(statement, string.Empty);

            var expressionTree = SplitStatementParts(sourceSpan);

#if DEBUG
            var rebuildStatement = RebuildStatement(expressionTree);
            if (sourceSpan != rebuildStatement)
                throw new Exception("FATAL: SplitStatementParts failed");
#endif

            return BoolLogicStatement.Create(expressionTree, variableNames);
        }

        /// <summary>
        /// Build logical statement A & B
        /// </summary>
        /// <remarks>
        ///     Operator must came from <see cref="LogicEnum"/> alias
        /// </remarks>
        public static bool CanBuildBoolLogicStatement(string statement, IReadOnlyCollection<string> variableNames, out string reason)
        {
            reason = string.Empty;

            if (!s_formulaCharAllowed.IsMatch(statement))
            {
                reason = "Invalid char used";
                return false;
            }

            var openBrasket = statement.Count(c => c == '(');
            var closedBrasket = statement.Count(c => c == ')');

            if (openBrasket != closedBrasket)
            {
                reason = openBrasket + " openBrasket vs " + closedBrasket + " closedBrasket ";
                return false;
            }

            if (variableNames.Any())
            {
                var tmpStatement = statement;
                foreach (var variable in variableNames)
                {
                    var originalSize = tmpStatement.Length;
                    tmpStatement = tmpStatement.Replace(variable, string.Empty);

                    var resultSize = tmpStatement.Length;

                    if (resultSize == originalSize)
                    {
                        reason = "variable not used " + variable;
                        return false;
                    }
                }
                if (!s_formulaCharNotAlphabeticalAllowed.IsMatch(tmpStatement))
                {
                    reason = "Invalid variable remains '" + tmpStatement + "'";
                    return false;
                }
            }

            return true;
        }

        #region Tools

        /// <summary>
        /// Create expression tree using group element '(' ')'
        /// </summary>
        private static TreeNode<string> SplitStatementParts(ReadOnlySpan<char> source)
        {
            var parts = new Queue<string>((int)source.Count('(') + 1);

            ReadOnlySpan<char> operators = "()";
            var entityStr = source;

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

            return SplitStatementPartsRec(parts);
        }

        /// <summary>
        /// Use recusive power to create expression tree using group element '(' ')'
        /// </summary>
        private static TreeNode<string> SplitStatementPartsRec(Queue<string> parts)
        {
            var sb = new StringBuilder();
            var children = new List<TreeNode<string>>();

            while (parts.Count > 0)
            {
                var part = parts.Dequeue();
                if (part == "(")
                {
                    var child = SplitStatementPartsRec(parts);

                    sb.Append("$" + children.Count);
                    children.Add(child);
                    continue;
                }

                if (part == ")")
                    break;

                sb.Append(part);
            }

            var root = new TreeNode<string>(sb.ToString());

            foreach (var child in children)
                root.AttachedChild(child);

            return root;
        }

#if DEBUG

        /// <summary>
        /// Rebuilds the statement.
        /// </summary>
        private static string RebuildStatement(TreeNode<string> root)
        {
            var statement = root.Entity;

            int indx = 0;
            foreach (var child in root.Children)
            {
                statement = statement.Replace("$" + indx, "(" + RebuildStatement(child) + ")");
                indx++;
            }

            return statement;
        }

#endif

        #endregion

        #endregion
    }
}
