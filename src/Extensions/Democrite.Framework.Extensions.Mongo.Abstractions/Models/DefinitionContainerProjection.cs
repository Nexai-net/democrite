// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Models
{
    using MongoDB.Bson.Serialization.Attributes;

    using System;

    /// <summary>
    /// Mongo object container
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    public struct DefinitionContainerProjection
    {
        #region Properties

        /// <summary>
        /// Gets or sets the uid.
        /// </summary>
        [BsonId]
        public Guid Uid { get; set; }

        /// <summary>
        /// Gets or sets the etag.
        /// </summary>
        [BsonRequired]
        [BsonElement]
        public string? Etag { get; set; }

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        [BsonRequired]
        [BsonElement]
        public string? Discriminator { get; set; }

        #endregion
    }
}
