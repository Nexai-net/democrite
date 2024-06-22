// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Tests
{
    using Newtonsoft.Json;

    using NFluent;

    using System;

    public static class SerializationTester
    {
        /// <summary>
        /// Test item serialization 
        /// </summary>
        public static void SerializeTester<TItem>(TItem item)
            where TItem : class, IEquatable<TItem>
        {
            var serializeSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
            };

            var json = JsonConvert.SerializeObject(item, serializeSettings);

            Check.That(json).IsNotNull().And.IsNotEmpty();

            var deserializedDef = JsonConvert.DeserializeObject(json, serializeSettings);
            Check.That(deserializedDef).IsNotNull()
                           .And
                           .IsInstanceOfType(item.GetType())
                           .And
                           .Not.IsSameReferenceAs(item);

            // check that through re-serialization with get the same
            // Help also to get the diff
            var rejson = JsonConvert.SerializeObject(deserializedDef, serializeSettings);
            Check.That(rejson).IsEqualTo(json);

#if DEBUG
            if (item.GetHashCode() != deserializedDef!.GetHashCode())
                item.Equals(deserializedDef);
#endif

            // If failt theire the issue may be in the equality check
            Check.That(deserializedDef).IsEqualTo(item);
        }
    }
}
