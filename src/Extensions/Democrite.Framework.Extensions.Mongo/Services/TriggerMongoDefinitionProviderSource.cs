// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Services
{
    using Democrite.Framework.Core.Abstractions.Triggers;

    using Microsoft.Extensions.Options;

    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.Utils;

    /// <summary>
    /// Provider source dedicated to <see cref="TriggerDefinition"/>
    /// </summary>
    /// <seealso cref="DefinitionBaseProviderSource{SequenceDefinition}" />
    internal sealed class TriggerMongoDefinitionProviderSource : DefinitionBaseProviderSource<TriggerDefinition>, ITriggerDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceMongoDefinitionProviderSource"/> class.
        /// </summary>
        public TriggerMongoDefinitionProviderSource(IMongoClientFactory mongoClientFactory,
                                                    IServiceProvider serviceProvider,
                                                    IOptions<MongoDBOptions> mongoDBOptions,
                                                    string key)
            : base(mongoClientFactory, serviceProvider, mongoDBOptions, key)
        {
        }

        #endregion
    }
}
