// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Tests
{
    using AutoFixture;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using Newtonsoft.Json;

    using NFluent;

    using System.Collections.Generic;

    using Xunit;

    /// <summary>
    /// Base class to test any surrorate to ensure all is fonctional
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TSurrogate">The type of the surrogate.</typeparam>
    /// <typeparam name="TConverter">The type of the converter.</typeparam>
    public class SurrogateBaseTest<TSource, TSurrogate, TConverter>
        where TSource : class, IEquatable<TSource>
        where TConverter : IConverter<TSource, TSurrogate>, new()
        where TSurrogate : struct
    {
        #region Fields

        private readonly Func<TSurrogate, TSurrogate, bool>? _surrogateComparer;
        private readonly Action<TSource>? _onSourcePreparatioForTesting;
        private readonly Func<Fixture, TSurrogate>? _surrogateCreation;
        private readonly Func<Fixture, TSource>? _sourceCreation;
        private readonly Formatting _shouldIndent;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SurrogateBaseTest{TSource, TSurrogate, TConverter}"/> class.
        /// </summary>
        public SurrogateBaseTest(Func<TSurrogate, TSurrogate, bool>? surrogateComparer = null,
                                 Action<TSource>? onSourcePreparatioForTesting = null,
                                 Func<Fixture, TSource>? sourceCreation = null,
                                 Func<Fixture, TSurrogate>? surrogateCreation = null)
        {
            // Provide lambda comparer all the class user to customize like those who inherite
            this._surrogateComparer = surrogateComparer;
            this._onSourcePreparatioForTesting = onSourcePreparatioForTesting;
            this._sourceCreation = sourceCreation;
            this._surrogateCreation = surrogateCreation;

            this._shouldIndent = System.Diagnostics.Debugger.IsAttached ? Formatting.Indented : Formatting.None;
        }

        #endregion

        #region methods

        /// <summary>
        /// Ensures the surrogate is serialization and deserializable.
        /// </summary>
        [Fact]
        public void Ensure_Surrogate_Serialization()
        {
            var result = ObjectTestHelper.IsSerializable<TSurrogate>(supportCyclingReference: true,
                                                                     supportMutableValueType: true,
                                                                     typeProvider: this._surrogateCreation,
                                                                     overrideComparer: CheckThatSurrogateAreEquals);

            Check.WithCustomMessage("Ensure_Surrogate_Serialization").That(result).IsTrue();
        }

        /// <summary>
        /// Ensures the <see cref="TSource"/> is serializable using <see cref="TSurrogate"/> and <see cref="TConverter"/>.
        /// </summary>
        [Fact]
        public void Ensure_Source_Is_Serializable_Using_Surrogate_And_Converter()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportServiceSubstitution: true);
            var source = SourceCreation(fixture);

            OnSourcePreparationForTesting(source);

            var converter = new TConverter();

            var surrogateSource = converter.ConvertToSurrogate(source);
            var newSource = converter.ConvertFromSurrogate(surrogateSource);

            Check.That(newSource).IsNotNull();

            Check.That(source).Not.IsSameReferenceAs(newSource);
            Check.That(source).IsEqualTo(newSource);
        }

        #region Tools

        /// <summary>
        /// Called when [source preparation for testing].
        /// </summary>
        protected virtual void OnSourcePreparationForTesting(TSource source)
        {
            this._onSourcePreparatioForTesting?.Invoke(source);
        }

        /// <summary>
        /// Simple structure check with <see cref="EqualityComparer{T}"/>
        /// By default a json serialization comparaison is done
        /// </summary>
        /// <remarks>
        ///     To override to more deep quality
        /// </remarks>
        protected virtual bool CheckThatSurrogateAreEquals(TSurrogate data, TSurrogate deserializedData)
        {
            if (this._surrogateComparer != null)
            {
                return this._surrogateComparer(data, deserializedData);
            }

            ObjectTestHelper.CheckThatObjectAreEquals(data, deserializedData, this._shouldIndent);
            return true;
        }

        /// <summary>
        /// Get a  <see cref="TSource"/> for testing purpose.
        /// </summary>
        protected virtual TSource SourceCreation(Fixture fixture)
        {
            if (this._sourceCreation != null)
                return this._sourceCreation(fixture);

            return fixture.Create<TSource>();
        }

        #endregion

        #endregion
    }
}
