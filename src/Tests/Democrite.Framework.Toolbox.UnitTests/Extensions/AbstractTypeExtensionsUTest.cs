// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Extensions
{
    using Democrite.Framework.Toolbox.Models;
    using Democrite.Framework.Toolbox.UnitTests.Xunits;
    using Democrite.UnitTests.ToolKit.Xunits;

    using Newtonsoft.Json;

    using NFluent;

    using System;
    using System.Reflection;

    /// <summary>
    /// Test for <see cref="AbstractTypeExtensions"/>
    /// </summary>
    public class AbstractTypeExtensionsUTest
    {
        #region Methods

        /// <summary>
        /// Test <see cref="AbstractTypeExtensions.GetAbstractType"/>
        /// </summary>
        [Theory]
        [InlineData(typeof(int), "Int32", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Task), "Task", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ValueTask), "ValueTask", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(string), "String", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Task<double>), "Task<Double>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ValueTask<double>), "ValueTask<Double>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ValueTask<Task<double>>), "ValueTask<Task<Double>>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(IConvertible), "IConvertible", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(ICheck<int>), "ICheck<Int32>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Tuple<>), "Tuple<>", AbstractTypeCategoryEnum.Generic)]
        [InlineData(typeof(Tuple<string>), "Tuple<String>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(Tuple<string, int>), "Tuple<String, Int32>", AbstractTypeCategoryEnum.Concret)]
        [InlineData(typeof(List<>), "List<>", AbstractTypeCategoryEnum.Generic)]
        [InlineData(typeof(IReadOnlyList<int>), "IReadOnlyList<Int32>", AbstractTypeCategoryEnum.Collection)]
        [InlineData(typeof(List<int>), "List<Int32>", AbstractTypeCategoryEnum.Collection)]
        [InlineData(typeof((string, int)), "ValueTuple<String, Int32>", AbstractTypeCategoryEnum.Concret)]

        public void AbstractType_TypeBuild(Type testType, string expectedDisplayName, AbstractTypeCategoryEnum categoryEnum)
        {
            var abstractType = testType.GetAbstractType();

            Check.That(abstractType).IsNotNull();
            Check.That(abstractType.DisplayName).IsEqualTo(expectedDisplayName);
            Check.That(abstractType.Category).IsEqualTo(categoryEnum);

            Check.ThatCode(() => abstractType.Equals(testType)).DoesNotThrow().And.WhichResult().IsTrue();

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };

            var typeJson = JsonConvert.SerializeObject(abstractType, settings);

            Check.That(typeJson).Not.IsNullOrEmpty();

            var deserializedAbstractType = JsonConvert.DeserializeObject(typeJson, settings);

            Check.That(deserializedAbstractType).IsNotNull()
                                                .And
                                                .Not.IsSameReferenceAs(abstractType)
                                                .And
                                                .IsEqualTo(abstractType);

            Check.ThatCode(() => abstractType.Equals(testType)).DoesNotThrow().And.WhichResult().IsTrue();
        }

        /// <summary>
        /// Test simple method of string
        /// </summary>
        [Theory]
        [GetMethodsData(types: new Type[] 
        {
            typeof(string), 
            typeof(IConvertible), 
            typeof(ICollection<double>), 
            typeof(List<double>), 
        })]
        public void AbstractType_All_Method_Of_Type(MethodInfo method)
        {
            AbstractTypeMethod(method, method.GetDisplayName());
        }

        #region Methods

        /// <summary>
        /// Test <see cref="AbstractTypeExtensions.GetAbstractType(Type)"/>
        /// </summary>
        private void AbstractTypeMethod(MethodInfo methodInfo, string expectedDisplayName)
        {
            var abstractMethod = methodInfo.GetAbstractMethod();

            Check.That(abstractMethod).IsNotNull();
            Check.That(abstractMethod.DisplayName).IsEqualTo(expectedDisplayName);

            Check.ThatCode(() => abstractMethod.Equals(methodInfo)).DoesNotThrow().And.WhichResult().IsTrue();

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };

            var typeJson = JsonConvert.SerializeObject(abstractMethod, settings);

            Check.That(typeJson).Not.IsNullOrEmpty();

            var deserializedAbstractType = JsonConvert.DeserializeObject(typeJson, settings);

            Check.That(deserializedAbstractType).IsNotNull()
                                                .And
                                                .Not.IsSameReferenceAs(abstractMethod)
                                                .And
                                                .IsEqualTo(abstractMethod);

            Check.ThatCode(() => abstractMethod.Equals(methodInfo)).DoesNotThrow().And.WhichResult().IsTrue();
        }

        #endregion

        #endregion
    }
}
