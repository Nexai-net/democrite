// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Serializers
{
    using MongoDB.Bson.Serialization;

    using System;

    /// <summary>
    /// Provide specific serializer for abstract type to force dynamic loading discriminator convention
    /// </summary>
    /// <seealso cref="IBsonSerializationProvider" />
    internal sealed class AbstractClassBsonSerializationProvider : IBsonSerializationProvider
    {
        private static readonly Type s_serializerTrait = typeof(AbstractClassBsonSerializer<>);

        /// <inheritdoc />
        public IBsonSerializer GetSerializer(Type type)
        {
            if (type != null && type.IsAbstract && !type.IsInterface)
            {
                return (IBsonSerializer)(Activator.CreateInstance(s_serializerTrait.MakeGenericType(type))!);
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
