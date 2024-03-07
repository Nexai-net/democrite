// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Models
{
    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Storage a mongo connection configuration attached to a specific key
    /// </summary>
    /// <seealso cref="MongoDBOptions" />
    public sealed class MongoDBDefinitionConnectionOptions : MongoDBOptions
    {
        #region Methods

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        #endregion
    }
}
