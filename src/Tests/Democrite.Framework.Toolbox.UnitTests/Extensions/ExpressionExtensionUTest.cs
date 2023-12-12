// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Extensions
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using AutoFixture.Xunit2;

    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.UnitTests.Xunits;
    using Democrite.UnitTests.ToolKit.Helpers;

    using Newtonsoft.Json;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Xunit.Sdk;

    /// <summary>
    /// Test extension around <see cref="Expression"/>
    /// </summary>
    public sealed class ExpressionExtensionUTest
    {
        #region Fields

        private static readonly JsonSerializerSettings s_serializationSetting;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExpressionExtensionUTest"/> class.
        /// </summary>
        static ExpressionExtensionUTest()
        {
            s_serializationSetting = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Test <see cref="ExpressionExtensions.Serialize{TInput}(this Expression{Func{TInput, bool}} expression)"/>
        /// one condition simple
        /// </summary>
        [Fact]
        public void ConditionSerialization_Simple()
        {
            var sucessStr = "sucess";
            var failStr = "fail";

            ConditionSerializationTest((string? item) => item == "sucess", new[] { sucessStr }, new[] { failStr, string.Empty, null });
            ConditionSerializationTest((string? item) => item == sucessStr, new[] { sucessStr }, new[] { failStr, string.Empty, null });
            ConditionSerializationTest((string? item) => !string.IsNullOrEmpty(item), new[] { sucessStr, failStr }, new[] { string.Empty, null });
        }

        /// <summary>
        /// Test <see cref="ExpressionExtensions.Serialize{TInput}(this Expression{Func{TInput, bool}} expression)"/>
        /// multiple simple condition and
        /// </summary>
        [Fact]
        public void ConditionSerialization_Simple_And()
        {
            var sucessStr = "succeed";
            var failStartStr = "success";
            var failEndStr = "feed";

            ConditionSerializationTest((string? item) => item != null && item.StartsWith("s") && item.EndsWith("d"), new[] { sucessStr }, new[] { failStartStr, failEndStr, string.Empty, null });
            ConditionSerializationTest((string? item) => item != null && (item.StartsWith("s") || item.EndsWith("d")), new[] { sucessStr, failStartStr, failEndStr }, new[] { string.Empty, null });
        }

        /// <summary>
        /// one condition simple that contains math operator
        /// </summary>
        [Theory]
        [EnumData<MathOperatorEnum>()]
        public void ConditionSerialization_Simple_MathOperator(MathOperatorEnum mathOperator)
        {
            Expression<Func<int, bool>> expression;

            switch (mathOperator)
            {
                case MathOperatorEnum.Sum:
                    expression = (i) => i + 2 < 10;
                    break;

                case MathOperatorEnum.Sub:
                    expression = (i) => i - 2 < 10;
                    break;

                case MathOperatorEnum.Multiply:
                    expression = (i) => i * 2 < 10;
                    break;

                case MathOperatorEnum.Modulo:
                    expression = (i) => i % 2 == 0;
                    break;

                case MathOperatorEnum.Divide:
                    expression = (i) => i / 2 < 10;
                    break;

                case MathOperatorEnum.None:
                    // All
#pragma warning disable IDE0047 // Remove unnecessary parentheses
                    expression = (i) => (((i + 2 - 4) * 2) / 4) % 2 == 0;
#pragma warning restore IDE0047 // Remove unnecessary parentheses
                    break;

                default:
                    throw new NotSupportedException("Math operator not managed " + mathOperator);
            }

            var func = expression.Compile();

            var results = Enumerable.Range(0, 1000)
                                    .GroupBy(i => func(i))   
                                    .ToDictionary(k => k.Key, v => v.ToList());

            Check.That(results).CountIs(2);
            Check.That(results[true]).Not.IsEmpty();
            Check.That(results[false]).Not.IsEmpty();

            ConditionSerializationTest<int>(expression, results[true], results[false]);
        }

        /// <summary>
        /// Conditions the serialization each serialize part.
        /// </summary>
        [Theory]
        [InlineData(typeof(ConditionParameterDefinition))]
        [InlineData(typeof(ConditionCallDefinition))]
        [InlineData(typeof(ConditionExpressionDefinition))]
        [InlineData(typeof(ConditionGroupDefinition))]
        [InlineData(typeof(ConditionMathOperationDefinition))]
        [InlineData(typeof(ConditionMemberAccessDefinition))]
        [InlineData(typeof(ConditionOperandDefinition))]
        [InlineData(typeof(ConditionValueDefinition))]
        public void ConditionSerialization_Each_SerializePart(Type partType)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<ConditionValueDefinition>(() => new ConditionValueDefinition(typeof(int), Random.Shared.Next(0, 152)));
            fixture.Register<ConditionBaseDefinition>(() => fixture.Create<ConditionValueDefinition>());

            var inst = fixture.Create(partType, new SpecimenContext(fixture));
            Check.That(inst).IsNotNull();

            var serializationJson = JsonConvert.SerializeObject(inst, s_serializationSetting);

            Check.That(serializationJson).IsNotNull();

            var newInst = JsonConvert.DeserializeObject(serializationJson, partType, s_serializationSetting);
            
            Check.That(newInst).IsNotNull().And.IsInstanceOfType(partType);
            Check.That(newInst).IsEqualTo(inst);
        }

        #region Tools

        /// <summary>
        /// Generic condition serialization tester
        /// </summary>
        private void ConditionSerializationTest<TItem>(Expression<Func<TItem, bool>> condition,
                                                       IReadOnlyCollection<TItem> working,
                                                       IReadOnlyCollection<TItem> failing)
        {
            var serializeCond = condition.Serialize();
            Check.That(serializeCond).IsNotNull();

            var jsonSerializeCond = JsonConvert.SerializeObject(serializeCond, s_serializationSetting);
            Check.That(jsonSerializeCond).Not.IsNullOrEmpty();

            var deseralizedCond = JsonConvert.DeserializeObject<ConditionExpressionDefinition>(jsonSerializeCond, s_serializationSetting);
            Check.That(deseralizedCond).IsNotNull();

            Check.That(serializeCond).IsEqualTo(deseralizedCond);

            var originFuncCond = condition.Compile();
            var deserializedExpressionCond = deseralizedCond?.ToExpression<TItem, bool>();

            var deserializedFuncCond = deserializedExpressionCond?.Compile();

            Check.That(originFuncCond).IsNotNull();
            Check.That(deserializedFuncCond).IsNotNull();
            Check.That(deserializedExpressionCond).IsNotNull();

            foreach (var work in working)
            {
                Check.ThatCode(() => originFuncCond(work)).WhichResult().IsTrue();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Check.ThatCode(() => deserializedFuncCond(work)).WhichResult().IsTrue();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            foreach (var fail in failing)
            {
                Check.ThatCode(() => originFuncCond(fail)).WhichResult().IsFalse();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Check.ThatCode(() => deserializedFuncCond(fail)).WhichResult().IsFalse();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        #endregion

        #endregion
    }
}
