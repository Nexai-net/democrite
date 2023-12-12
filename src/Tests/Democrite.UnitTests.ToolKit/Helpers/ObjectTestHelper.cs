// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Helpers
{
    using AutoFixture;

    using Newtonsoft.Json;

    using NFluent;

    using System.Diagnostics;
    using Newtonsoft.Json.Serialization;
    using AutoFixture.AutoNSubstitute;

    /// <summary>
    /// Helper unit test to validate serialization
    /// </summary>
    public static class ObjectTestHelper
    {
        #region methods

        /// <summary>
        /// Prepares the fixture.
        /// </summary>
        public static Fixture PrepareFixture(Fixture? fixture = null,
                                             bool supportMutableValueType = false,
                                             bool supportCyclingReference = false,
                                             bool supportServiceSubstitution = true)
        {
            fixture ??= new Fixture();

            if (supportMutableValueType)
            {
                var customization = new SupportMutableValueTypesCustomization();
                customization.Customize(fixture);
            }

            if (supportCyclingReference)
            {
                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                                 .ForEach(b => fixture.Behaviors.Remove(b));

                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            }

            if (supportServiceSubstitution)
            {
                var nsubstitute = new AutoNSubstituteCustomization();
                fixture.Customize(nsubstitute);
            }

            return fixture;
        }

        /// <summary>
        /// Ensures the surrogate is serialization and deserializable.
        /// </summary>
        public static bool IsSerializable<TType>(bool supportMutableValueType = false,
                                                 bool supportCyclingReference = false,
                                                 Func<TType, TType, bool>? overrideComparer = null)
        {
            var shouldIndent = Debugger.IsAttached ? Formatting.Indented : Formatting.None;
            var fixture = PrepareFixture(supportMutableValueType: supportMutableValueType,
                                         supportCyclingReference: supportCyclingReference);

            var data = fixture.Create<TType>();

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = shouldIndent
            };

            var serializationErrors = new List<ErrorContext>();

            jsonSerializerSettings.Error += (source, args) => serializationErrors.Add(args.ErrorContext);

            var jsonStr = JsonConvert.SerializeObject(data, jsonSerializerSettings);

            Check.WithCustomMessage("Serialization : \n" + string.Join("\n", serializationErrors))
                 .That(serializationErrors.Count).IsEqualTo(0);

            Check.That(jsonStr).Not.IsNullOrEmpty();

            var deserializedData = JsonConvert.DeserializeObject<TType>(jsonStr, jsonSerializerSettings);

            Check.WithCustomMessage("Deserialization : \n" + string.Join("\n", serializationErrors))
                 .That(serializationErrors.Count).IsEqualTo(0);

            Check.WithCustomMessage("Test type is not null or default").That(deserializedData).IsNotEqualTo(default);

            if (overrideComparer != null)
            {
                var comparer = overrideComparer(data, deserializedData!);
                return comparer;
            }

            CheckThatObjectAreEquals(data, deserializedData, shouldIndent);

            return true;
        }

        /// <summary>
        /// Simple object comparaison
        /// By default a json serialization comparaison is done
        /// </summary>
        /// <remarks>
        ///     To override to more deep quality
        /// </remarks>
        public static void CheckThatObjectAreEquals<TType>(TType data, TType deserializedData, Formatting shouldIndent)
        {
            var serializedDataJson = JsonConvert.SerializeObject(data, shouldIndent);
            var deserializedDataJson = JsonConvert.SerializeObject(deserializedData, shouldIndent);

            Check.That(serializedDataJson).IsEqualTo(deserializedDataJson);
        }

        #endregion
    }
}
