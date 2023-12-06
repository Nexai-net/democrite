// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Statements
{
    using Democrite.Framework.Toolbox.Statements;

    using NFluent;

    /// <summary>
    /// 
    /// </summary>
    public sealed class ExpressionBuilderUnitTest
    {
        #region Methods

        /// <summary>
        /// Test build simple bool statement
        /// </summary>
        [Theory]
        [InlineData("A      &B   ", "A", "B", true, true, true)]
        [InlineData("A      &B   ", "A", "B", true, false, false)]
        [InlineData("A      &B   ", "A", "B", false, true, false)]
        [InlineData("A      &B   ", "A", "B", false, false, false)]

        [InlineData("A      |B   ", "A", "B", true, true, true)]
        [InlineData("A      |B   ", "A", "B", true, false, true)]
        [InlineData("A      |B   ", "A", "B", false, true, true)]
        [InlineData("A      |B   ", "A", "B", false, false, false)]

        [InlineData("A      &!B   ", "A", "B", true, true, false)]
        [InlineData("A      &!B   ", "A", "B", true, false, true)]

        [InlineData("!A      &B   ", "A", "B", true, true, false)]
        [InlineData("!A      &B   ", "A", "B", false, true, true)]
        public void ExpressionBuilder_BoolStatement_Simple(string formula, string variableA, string variableB, bool valueA, bool valueB, bool result)
        {
            ExpressionBuilder_BoolStatement(formula, new[] { variableA, variableB }, new[] { valueA, valueB }, result);
        }

        /// <summary>
        /// Test build single brasket bool statement
        /// </summary>
        [Fact]
        public void ExpressionBuilder_BoolStatement_SingleBasket()
        {
            ExpressionBuilder_BoolStatement("(A      &B   )&    C",
                                            new[] { "A", "B", "C" },
                                            new[] { true, true, true },
                                            true);

            ExpressionBuilder_BoolStatement("(A      &B   )&    C",
                                            new[] { "A", "B", "C" },
                                            new[] { true, false, true },
                                            false);
        }

        /// <summary>
        /// Test build nested brasket bool statement
        /// </summary>
        [Fact]
        public void ExpressionBuilder_BoolStatement_NestedBasket()
        {
            ExpressionBuilder_BoolStatement("((A      &B   )&    C) & D",
                                            new[] { "A", "B", "C", "D" },
                                            new[] { true, true, true, true },
                                            true);
        }

        /// <summary>
        /// Test build nested brasket bool statement
        /// </summary>
        [Fact]
        public void ExpressionBuilder_BoolStatement_Complex()
        {
            //ExpressionBuilder_BoolStatement("A | (A & B) | (A | (C & B) & D)",
            //                                new[] { "A", "B", "C", "D" },
            //                                new[] { true, true, true, true },
            //                                true,
            //                                new[] { true, true, true, true });

            ExpressionBuilder_BoolStatement("((A & B & C) | (A | (C & B) & D))",
                                            new[] { "A", "B", "C", "D" },
                                            new[] { true, true, true, true },
                                            true);
        }

        #region Tools

        /// <summary>
        /// Test build simple bool statement
        /// </summary>
        private void ExpressionBuilder_BoolStatement(string formula,
                                                     IReadOnlyList<string> variableNames,
                                                     IReadOnlyCollection<bool> variableValues,
                                                     bool resultExpect)

        {
            var statement = ExpressionBuilder.BuildBoolLogicStatement(formula, variableNames);

            Check.That(statement).IsNotNull();

            var result = statement.Ask(variableValues.ToArray());

            Check.That(result).IsEqualTo(resultExpect);
        }

        #endregion

        #endregion
    }
}
