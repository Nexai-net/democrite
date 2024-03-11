// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Conventions
{
    using Elvex.Toolbox;

    using MongoDB.Bson.Serialization;

    using System;

    /// <summary>
    /// Bson converter for <see cref="NoneType"/>
    /// </summary>
    /// <seealso cref="IBsonSerializer{NoneType}" />
    public sealed class NoneTypeStringConverter : IBsonSerializer<NoneType>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NoneTypeStringConverter"/> class.
        /// </summary>
        static NoneTypeStringConverter()
        {
            Instance = new NoneTypeStringConverter();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NoneTypeStringConverter"/> class from being created.
        /// </summary>
        private NoneTypeStringConverter()
        {
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Type ValueType
        {
            get { return typeof(NoneType); }
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static NoneTypeStringConverter Instance { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public NoneType Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return NoneType.Instance;
        }

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, NoneType value)
        {
            Serialize(context, args, (object)value);
        }

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            if (value is null)
                context.Writer.WriteNull();
            else
                context.Writer.WriteString(nameof(NoneType));
        }

        /// <inheritdoc />
        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        #endregion
    }
}
