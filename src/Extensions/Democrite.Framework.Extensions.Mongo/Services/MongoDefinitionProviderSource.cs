// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Extensions.Mongo.Models;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Options;

    using Orleans.Providers.MongoDB.Utils;

    /// <summary>
    /// Provider source dedicated to <see cref="SequenceDefinition"/>
    /// </summary>
    /// <seealso cref="DefinitionBaseProviderSource{SequenceDefinition}" />
    public sealed class MongoDefinitionProviderSource<TDefinition> : DefinitionBaseProviderSource<TDefinition>
        where TDefinition : class, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDefinitionProviderSource{TDefinition}"/> class.
        /// </summary>
        public MongoDefinitionProviderSource(IMongoClientFactory mongoClientFactory,
                                             IServiceProvider serviceProvider,
                                             IOptions<MongoDBDefinitionConnectionOptions> options)
            : base(mongoClientFactory, serviceProvider, options.Value.ToOption(), options.Value.Key!)
        {
        }

        #endregion
    }
}
