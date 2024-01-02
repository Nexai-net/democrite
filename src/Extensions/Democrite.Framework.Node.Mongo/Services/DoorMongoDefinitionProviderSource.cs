// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Services
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.Utils;

    /// <summary>
    /// Provider source dedicated to <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="DefinitionBaseProviderSource{SequenceDefinition}" />
    internal sealed class DoorMongoDefinitionProviderSource : DefinitionBaseProviderSource<DoorDefinition>, IDoorDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorMongoDefinitionProviderSource"/> class.
        /// </summary>
        public DoorMongoDefinitionProviderSource(IMongoClientFactory mongoClientFactory,
                                                 MongoDBOptions mongoDBOptions,
                                                 string key)
            : base(mongoClientFactory, mongoDBOptions, key)
        {
        }

        #endregion
    }
}
