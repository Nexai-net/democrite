﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using Microsoft.Extensions.Options;

    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.Utils;

    /// <summary>
    /// Provider source dedicated to <see cref="SequenceDefinition"/>
    /// </summary>
    /// <seealso cref="DefinitionBaseProviderSource{SequenceDefinition}" />
    internal sealed class SequenceMongoDefinitionProviderSource : DefinitionBaseProviderSource<SequenceDefinition>, ISequenceDefinitionSourceProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceMongoDefinitionProviderSource"/> class.
        /// </summary>
        public SequenceMongoDefinitionProviderSource(IMongoClientFactory mongoClientFactory,
                                                     IServiceProvider serviceProvider,
                                                     IOptions<MongoDBOptions> mongoDBOptions,
                                                     string key)
            : base(mongoClientFactory, serviceProvider, mongoDBOptions, key)
        {
        }

        #endregion
    }
}
