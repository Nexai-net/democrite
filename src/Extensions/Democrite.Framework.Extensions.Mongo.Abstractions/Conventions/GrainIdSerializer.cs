// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Conventions
{
    using Elvex.Toolbox;

    using Microsoft.Win32.SafeHandles;

    using MongoDB.Bson.IO;
    using MongoDB.Bson.Serialization;
    using Newtonsoft.Json.Linq;

    using Orleans.Runtime;
    using Orleans.Serialization.Buffers;

    using System;
    using System.Text;

    /// <summary>
    /// Bson converter for <see cref="GrainId"/>
    /// </summary>
    /// <seealso cref="IBsonSerializer{NoneType}" />
    public sealed class GrainIdSerializer : IBsonSerializer<GrainId>
    {
        //GrainIdJsonConverter
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NoneTypeStringConverter"/> class.
        /// </summary>
        static GrainIdSerializer()
        {
            Instance = new GrainIdSerializer();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NoneTypeStringConverter"/> class from being created.
        /// </summary>
        private GrainIdSerializer()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static GrainIdSerializer Instance { get; }

        /// <inheritdoc />
        public Type ValueType
        {
            get { return typeof(GrainId); }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public GrainId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return (GrainId)((IBsonSerializer)this).Deserialize(context, args);
        }

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, GrainId value)
        {
            Serialize(context, args, (object)value);
        }

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            if (value is GrainId grainId)
            {
                var type = grainId.Type.AsSpan();
                var key = grainId.Key.AsSpan();
                Span<byte> buf = stackalloc byte[type.Length + key.Length + 1];

                type.CopyTo(buf);
                buf[type.Length] = (byte)'/';
                key.CopyTo(buf[(type.Length + 1)..]);

                context.Writer.WriteString(Encoding.UTF8.GetString(buf));
            }
            else
            {
                context.Writer.WriteNull();
            }
        }

        /// <inheritdoc />
        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var buf = context.Reader.ReadString();
            return GrainId.Parse(buf);
        }

        #endregion
    }
}
