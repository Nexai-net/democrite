// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Repositories;

    using MongoDB.Bson.Serialization.Attributes;

    using System;
    using System.Linq;

    /// <summary>
    /// Mongo object container
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    public class DefinitionContainer : IEntityWithId<Guid>
    {
        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="DefinitionContainer"/> class from being created.
        /// </summary>
        [BsonConstructor]
        internal DefinitionContainer(Guid uid,
                                     string etag,
                                     string discriminator)
        {
            this.Uid = uid;
            this.Etag = etag;
            this.Discriminator = discriminator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the uid.
        /// </summary>
        [BsonId]
        public Guid Uid { get; }

        /// <summary>
        /// Gets or sets the etag.
        /// </summary>
        [BsonRequired]
        [BsonElement]
        public string Etag { get; }

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        [BsonRequired]
        [BsonElement]
        public string Discriminator { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified <see cref="DefinitionContainer"/>.
        /// </summary>
        public static DefinitionContainer<TDefinition> Create<TDefinition>(TDefinition definition)
            where TDefinition : class, IDefinition
        {
            return new DefinitionContainer<TDefinition>(definition);
        }

        #endregion
    }

    /// <summary>
    /// Mongo object container
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    [BsonDiscriminator(Required = false)]
    public sealed class DefinitionContainer<TDefinition> : DefinitionContainer
        where TDefinition : class, IDefinition
    {

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DefinitionContainer{TDefinition}"/> class.
        /// </summary>
        static DefinitionContainer()
        {
            DefaultDiscriminator = typeof(TDefinition).Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionContainer{TDefinition}"/> class.
        /// </summary>
        public DefinitionContainer(TDefinition definition)
            : base(definition.Uid,
                   new string(Guid.NewGuid().ToString().Replace("-", "").Take(8).ToArray()),
                   DefaultDiscriminator)
        {
            this.Definition = definition;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DefinitionContainer{TDefinition}"/> class from being created.
        /// </summary>
        [BsonConstructor]
        public DefinitionContainer(Guid uid,
                                   string etag,
                                   string discriminator,
                                   TDefinition definition)
            : base(uid, etag, discriminator)
        {
            this.Definition = definition;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the discriminator.
        /// </summary>
        public static string DefaultDiscriminator { get; }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        [BsonElement()]
        public TDefinition Definition { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to containerheaderonly.
        /// </summary>
        public DefinitionContainer ToContainerHeaderOnly()
        {
            return new DefinitionContainer(this.Uid, this.Etag, this.Discriminator);
        }

        #endregion
    }
}
