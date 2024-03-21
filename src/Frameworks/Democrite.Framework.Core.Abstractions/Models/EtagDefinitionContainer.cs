// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Repositories;

    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Container used to save definition with an etag to detect update
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public class EtagDefinitionContainer : IEntityWithId<Guid>
    {
        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="EtagDefinitionContainer"/> class from being created.
        /// </summary>
        public EtagDefinitionContainer(Guid uid,
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
        [DataMember]
        public Guid Uid { get; }

        /// <summary>
        /// Gets or sets the etag.
        /// </summary>
        [DataMember]
        public string Etag { get; }

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        [DataMember]
        public string Discriminator { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified <see cref="EtagDefinitionContainer"/>.
        /// </summary>
        public static EtagDefinitionContainer<TDefinition> Create<TDefinition>(TDefinition definition)
            where TDefinition : class, IDefinition
        {
            return new EtagDefinitionContainer<TDefinition>(definition);
        }

        /// <summary>
        /// Test is the definition follow the requirement type
        /// </summary>
        public virtual bool IsDefinition<TDefinition>()
        {
            throw new NotSupportedException("Only DefinitionContainer<TDefinition> could provide the definition");
        }

        /// <summary>
        /// Gets the contain definition.
        /// </summary>
        public virtual TDefinition GetContainDefinition<TDefinition>()
        {
            throw new NotSupportedException("Only DefinitionContainer<TDefinition> could provide the definition");
        }

        #endregion
    }

    /// <summary>
    /// Container used to save definition with an etag to detect update
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class EtagDefinitionContainer<TDefinition> : EtagDefinitionContainer
        where TDefinition : class, IDefinition
    {

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="EtagDefinitionContainer{TDefinition}"/> class.
        /// </summary>
        static EtagDefinitionContainer()
        {
            DefaultDiscriminator = typeof(TDefinition).GetAbstractType().DisplayName.Replace(".", "").Trim().Replace(" ", "").Replace("-", "").Replace("_", "");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EtagDefinitionContainer{TDefinition}"/> class.
        /// </summary>
        public EtagDefinitionContainer(TDefinition definition)
            : base(definition.Uid,
                   new string(Guid.NewGuid().ToString().Replace("-", "").Take(8).ToArray()),
                   DefaultDiscriminator)
        {
            this.Definition = definition;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="EtagDefinitionContainer{TDefinition}"/> class from being created.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        internal EtagDefinitionContainer(Guid uid,
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
        [IgnoreDataMember]
        public static string DefaultDiscriminator { get; }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        [DataMember]
        public TDefinition Definition { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to containerheaderonly.
        /// </summary>
        public EtagDefinitionContainer ToContainerHeaderOnly()
        {
            return new EtagDefinitionContainer(this.Uid, this.Etag, this.Discriminator);
        }

        /// <summary>
        /// Test is the definition follow the requirement type
        /// </summary>
        public override bool IsDefinition<TRequestDefinition>()
        {
            return this.Definition is TRequestDefinition;
        }

        /// <summary>
        /// Gets the contain definition.
        /// </summary>
        public override TRequestDefinition GetContainDefinition<TRequestDefinition>()
        {
            if (this.Definition is TRequestDefinition requested)
                return requested;

            throw new InvalidCastException($"Could not cast definition type {this.Definition} to requested {typeof(TRequestDefinition)}");
        }

        #endregion
    }
}
