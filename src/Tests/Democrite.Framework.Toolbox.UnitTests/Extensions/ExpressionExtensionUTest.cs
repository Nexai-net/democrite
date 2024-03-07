// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Extensions
{
    using AutoFixture;
    using AutoFixture.Kernel;

    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Abstractions.Expressions;
    using Democrite.Framework.Toolbox.Models;
    using Democrite.Framework.Toolbox.UnitTests.Xunits;
    using Democrite.UnitTests.ToolKit.Helpers;

    using Newtonsoft.Json;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

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

        #region Nested

        public record struct ExpressioSimpleSampleObject(Guid uid, string name, bool value, StringComparison? StringComparison);
        public record struct ExpressionComplexSampleObject(Guid uid, string name, bool value, ExpressioSimpleSampleObject nested);

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

        //[InlineData(typeof(MemberBindingDefinition))]
        [InlineData(typeof(MemberInputConstantBindingDefinition<string>))]
        [InlineData(typeof(MemberInputConstantBindingDefinition<bool>))]
        public void Expression_serialization_part(Type partType)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            // TODO: Allow more type combinaison to improve test efficiency
            fixture.Register<AbstractType>(() => typeof(int).GetAbstractType());
            fixture.Register<ConditionValueDefinition>(() => new ConditionValueDefinition(typeof(int).GetAbstractType(), Random.Shared.Next(0, 152)));
            fixture.Register<ConditionBaseDefinition>(() => fixture.Create<ConditionValueDefinition>());

            var inst = fixture.Create(partType, new SpecimenContext(fixture));
            Check.That(inst).IsNotNull();

            var serializationJson = JsonConvert.SerializeObject(inst, s_serializationSetting);

            Check.That(serializationJson).IsNotNull();

            var newInst = JsonConvert.DeserializeObject(serializationJson, partType, s_serializationSetting);

            Check.That(newInst).IsNotNull().And.IsInstanceOfType(partType);
            Check.That(newInst).IsEqualTo(inst);
        }

        /// <summary>
        /// Members the initialize constant binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = uidValue,
                value = true,
                name = nameValue
            };
            MemberInit_Tester(expr, "Poner Rose");
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_And_Input_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();
            var uidValue = Guid.NewGuid();

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = uidValue,
                value = true,
                name = input
            };

            MemberInit_Tester(expr, "Poner Rose");
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_And_InputChainCall_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            Expression<Func<ExpressioSimpleSampleObject, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = input.uid,
                value = true,
                name = nameValue
            };

            MemberInit_Tester(expr, new ExpressioSimpleSampleObject() { uid = Guid.NewGuid() });
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Constant_And_Extern_ChainCall_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            var externInput = new ExpressioSimpleSampleObject()
            {
                uid = Guid.NewGuid(),
            };

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                uid = externInput.uid,
                value = true,
                name = nameValue
            };

            MemberInit_Tester(expr, "Poeny Rose");
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Convert_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            Expression<Func<string, ExpressioSimpleSampleObject>> expr = input => new ExpressioSimpleSampleObject()
            {
                value = true,
                StringComparison = StringComparison.OrdinalIgnoreCase
            };

            MemberInit_Tester(expr, "Poeny Rose");
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Nested_Param_Binding()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            Expression<Func<ExpressioSimpleSampleObject, ExpressionComplexSampleObject>> expr = input => new ExpressionComplexSampleObject()
            {
                uid = input.uid,
                value = true,
                name = nameValue,
                nested = input
            };

            MemberInit_Tester(expr, new ExpressioSimpleSampleObject() { uid = Guid.NewGuid() });
        }

        /// <summary>
        /// Members the initialize constant and input binding.
        /// </summary>
        [Fact]
        public void MemberInit_Nested_Build_Binding_Full()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var nameValue = fixture.Create<string>();

            var externInput = new ExpressioSimpleSampleObject()
            {
                uid = Guid.NewGuid(),
            };

            Expression<Func<ExpressioSimpleSampleObject, ExpressionComplexSampleObject>> expr = input => new ExpressionComplexSampleObject()
            {
                uid = input.uid,
                value = true,
                name = nameValue,
                nested = new ExpressioSimpleSampleObject()
                {
                    uid = externInput.uid,
                    value = true,
                    name = "Poney rose"
                }
            };

            MemberInit_Tester(expr, new ExpressioSimpleSampleObject() { uid = Guid.NewGuid() });
        }

        #region Tools

        /// <summary>
        /// Members the initialize constant binding.
        /// </summary>
        private void MemberInit_Tester<TInput, TOutput>(Expression<Func<TInput, TOutput>> expr, TInput test)
        {
            var bindingModel = expr.SerializeMemberInitialization();
            Check.That(bindingModel).IsNotNull();

            var serializationJson = JsonConvert.SerializeObject(bindingModel, s_serializationSetting);
            Check.That(serializationJson).IsNotNull();

            var newBindingModel = (MemberInitializationDefinition)JsonConvert.DeserializeObject(serializationJson, bindingModel.GetType(), s_serializationSetting)!;
            Check.That(serializationJson).IsNotNull();

            var newInitExpr = newBindingModel.ToMemberInitializationExpression<TInput, TOutput>();
            Check.That(newInitExpr).IsNotNull();

            var sourceGenerated = expr.Compile().Invoke(test);
            var generationAfter = newInitExpr.Compile().Invoke(test);

            Check.That(sourceGenerated).IsEqualTo(generationAfter);
        }

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
