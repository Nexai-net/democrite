// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Models
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Driver;

    using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal sealed class GrainStateContainer<TState> : IEntityWithId<string>, IContainerWithDiscriminator<GrainStateContainer<TState>>
    {
        #region Fields

        private static readonly FilterDefinition<GrainStateContainer<TState>>? s_discriminator;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="GrainStateContainer{TState}"/> class.
        /// </summary>
        static GrainStateContainer()
        {
            var traits = typeof(TState);

            if (traits.IsInterface == false && traits.IsAbstract == false)
                s_discriminator = Builders<GrainStateContainer<TState>>.Filter.Regex(new StringFieldDefinition<GrainStateContainer<TState>>("_doc._t"), new BsonRegularExpression("^" + traits.Namespace + "." + traits.Name + ",*"));
        }

        #endregion

        #region Properties

        [BsonId]
        [NotNull]
        [BsonElement("_id")]
        public string Uid { get; init; }

        [BsonElement("_tag")]
        [NotNull]
        public string Etag { get; init; }

        [BsonElement("_doc")]
        public TState? State { get; init; }

        public bool RecordExists { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the discriminator filter.
        /// </summary>
        public FilterDefinition<GrainStateContainer<TState>>? DiscriminatorFilter
        {
            get { return s_discriminator; }
        }

        #endregion
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
