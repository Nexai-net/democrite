// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Tests
{
    using AutoFixture;

    using Castle.DynamicProxy.Internal;

    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Surrogates;

    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using System;
    using System.Diagnostics;
    using System.Reflection;

    public abstract class ExceptionSerializationTester
    {
        #region Fields
        
        private static readonly MethodInfo s_genericTestImplMethod;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExceptionSerializationTester"/> class.
        /// </summary>
        static ExceptionSerializationTester()
        {
            var implMethod = typeof(ExceptionSerializationTester).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                                                 .First(m => m.Name == nameof(Exception_Serialization_Through_Surrogate_Specialized));

            Debug.Assert(implMethod != null);

            s_genericTestImplMethod = implMethod;
        }

        #endregion

        /// <summary>
        /// Ensure democrite system exception have correctly surrogate to serialized
        /// </summary>
        protected void Exception_Serialization_Through_Surrogate(Type exceptionType)
        {
            var assembly = exceptionType.Assembly;

            Check.ThatCode(() => exceptionType.IsAssignableTo(typeof(IDemocriteException))).WhichResult().IsTrue();
            Check.ThatCode(() => exceptionType.IsAssignableTo(typeof(Exception))).WhichResult().IsTrue();

            var converters = assembly.GetTypes()
                                     .Where(t => t.IsClass == true && t.IsAbstract == false)
                                     .Select(cnv => new
                                     {
                                         Converter = cnv,
                                         ConvertInterFace = cnv.GetAllInterfaces().Where(i => i.Name.StartsWith("IConverter"))
                                                                                  .FirstOrDefault(i => i.Namespace == "Orleans" &&
                                                                                                       i.ContainsGenericParameters == false &&
                                                                                                       i.GetGenericArguments().Length == 2 &&
                                                                                                       i.GetGenericArguments().First() == exceptionType)
                                     })
                                     .Where(kv => kv.ConvertInterFace != null)
                                     .Select(kv => new
                                     {
                                         Converter = kv.Converter,
                                         Surrogate = kv.ConvertInterFace!.GetGenericArguments().Last()
                                     }).FirstOrDefault();

            Check.WithCustomMessage("No Surrogate and converter found for exception type " + exceptionType).That(converters).IsNotNull();
            Check.That(converters!.Converter).IsNotNull();
            Check.That(converters!.Surrogate).IsNotNull();

            var specTestMethod = s_genericTestImplMethod.MakeGenericMethod(exceptionType, converters.Surrogate, converters.Converter);
            Debug.Assert(specTestMethod != null);

            var registerAttribute = converters.Converter.GetCustomAttribute<RegisterConverterAttribute>();

            Check.WithCustomMessage("Converter must have the attribute RegisterConverterAttribute").That(registerAttribute).IsNotNull();

            var SurrogateAttribute = converters.Surrogate.GetCustomAttribute<GenerateSerializerAttribute>();

            Check.WithCustomMessage("Surrogate must have the attribute GenerateSerializerAttribute").That(SurrogateAttribute).IsNotNull();

            specTestMethod!.Invoke(this, new object?[0]);
        }

        private void Exception_Serialization_Through_Surrogate_Specialized<TSource, TSurrogate, TConverter>()
            where TSource : class, IEquatable<TSource>
            where TConverter : IConverter<TSource, TSurrogate>, new()
            where TSurrogate : struct
        {
            var tester = new SurrogateBaseTest<TSource, TSurrogate, TConverter>(sourceCreation: (fixture) =>
                                                                                {
                                                                                    fixture.Register<AbstractType>(() => ObjectTestHelper.GenerateRandomAbstractType());
                                                                                    fixture.Register<ConcretBaseType>(() => (ConcretBaseType)ObjectTestHelper.GenerateRandomAbstractType());
                                                                                    fixture.Register<IConcretTypeSurrogate>(() => ConcretBaseTypeConverter.ConvertToSurrogate((ConcretBaseType)ObjectTestHelper.GenerateRandomAbstractType()));
                                                                                    OnSourceCreationSetup<TSource, TSurrogate, TConverter>(fixture);
                                                                                    return fixture.Create<TSource>();
                                                                                }, 
                                                                                surrogateCreation: (fixture) =>
                                                                                {
                                                                                    fixture.Register<AbstractType>(() => ObjectTestHelper.GenerateRandomAbstractType());
                                                                                    fixture.Register<ConcretBaseType>(() => (ConcretBaseType)ObjectTestHelper.GenerateRandomAbstractType());
                                                                                    fixture.Register<IConcretTypeSurrogate>(() => ConcretBaseTypeConverter.ConvertToSurrogate((ConcretBaseType)ObjectTestHelper.GenerateRandomAbstractType()));
                                                                                    OnSurrogateCreationSetup<TSource, TSurrogate, TConverter>(fixture);
                                                                                    return fixture.Create<TSurrogate>();
                                                                                });
            
            using (var scope = Check.StartBatch(nameof(SurrogateBaseTest<TSource, TSurrogate, TConverter>) +
                                                "." +
                                                nameof(SurrogateBaseTest<TSource, TSurrogate, TConverter>.Ensure_Surrogate_Serialization)))
            {
                tester.Ensure_Surrogate_Serialization();
            }

            using (var scope = Check.StartBatch(nameof(SurrogateBaseTest<TSource, TSurrogate, TConverter>) +
                                                "." +
                                                nameof(SurrogateBaseTest<TSource, TSurrogate, TConverter>.Ensure_Source_Is_Serializable_Using_Surrogate_And_Converter)))
            {
                tester.Ensure_Source_Is_Serializable_Using_Surrogate_And_Converter();
            }
        }

        protected virtual void OnSurrogateCreationSetup<TSource, TSurrogate, TConverter>(Fixture fixture)
            where TSource : class, IEquatable<TSource>
            where TSurrogate : struct
            where TConverter : IConverter<TSource, TSurrogate>, new()
        {
        }

        protected virtual void OnSourceCreationSetup<TSource, TSurrogate, TConverter>(Fixture fixture)
            where TSource : class, IEquatable<TSource>
            where TSurrogate : struct
            where TConverter : IConverter<TSource, TSurrogate>, new()
        {
        }
    }
}
