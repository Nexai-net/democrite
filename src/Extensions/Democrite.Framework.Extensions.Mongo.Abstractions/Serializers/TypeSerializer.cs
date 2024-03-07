// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Serializers
{
    using MongoDB.Bson.Serialization;

    using System;

    /// <summary>
    /// Serializer <see cref="Type"/> through <see cref="Type.AssemblyQualifiedName"/>
    /// </summary>
    /// <remarks>
    ///     Attention the deserialize instance may not posses the real type
    /// </remarks>
    public sealed class TypeSerializer : IBsonSerializer, IBsonSerializer<Type>
    {
        #region Fields

        private static readonly Type s_valueType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TypeSerializer"/> class.
        /// </summary>
        static TypeSerializer()
        {
            s_valueType = typeof(Type);
        }

        #endregion

        /// <inheritdoc />
        public Type ValueType
        {
            get { return s_valueType; }
        }

        /// <inheritdoc />
        public object? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType == MongoDB.Bson.BsonType.String)
                return Type.GetType(context.Reader.ReadString());

            return null;
        }

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var write = context.Writer;
            if (value is Type type)
                write.WriteString(type.AssemblyQualifiedName);
            else
                write.WriteNull();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Type value)
        {
            ((IBsonSerializer)this).Serialize(context, args, value);
        }

#pragma warning disable CS8768 // Nullability of reference types in return type doesn't match implemented member (possibly because of nullability attributes).
        Type? IBsonSerializer<Type>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
#pragma warning restore CS8768 // Nullability of reference types in return type doesn't match implemented member (possibly because of nullability attributes).
        {
            return ((IBsonSerializer)this).Deserialize(context, args) as Type;
        }
    }
}
