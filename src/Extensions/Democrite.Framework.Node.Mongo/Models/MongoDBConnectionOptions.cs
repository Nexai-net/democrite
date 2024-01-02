﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Models
{
    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Storage a mongo connection configuration attached to a specific key
    /// </summary>
    /// <seealso cref="MongoDBOptions" />
    internal sealed class MongoDBConnectionOptions : MongoDBOptions
    {
        #region Fields

        public const string DEMOCRITE_CLUSTER = "Membership";
        public const string DEMOCRITE_REMINDER = nameof(DEMOCRITE_REMINDER);
        public const string DEMOCRITE_DEFAULT = nameof(DEMOCRITE_DEFAULT);
        public const string DEMOCRITE_VGRAIN_STATE = nameof(DEMOCRITE_VGRAIN_STATE);
        public const string DEMOCRITE_SIGNAL_STATE = nameof(DEMOCRITE_SIGNAL_STATE);
        public const string DEMOCRITE_DOOR_STATE = nameof(DEMOCRITE_DOOR_STATE);

        #endregion

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
