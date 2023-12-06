// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Extensions
{
    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Extensions;

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
