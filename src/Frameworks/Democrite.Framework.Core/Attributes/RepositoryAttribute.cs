// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using Democrite.Framework.Core.Attributes;

    using Orleans;
    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Attribute used to specify witch storage you want service <see cref="IReadOnlyRepository{TEntity}"/> dedicated
    /// </summary>
    /// <seealso cref="IAttributeToFactoryMapper{StorageRepositoryAttribute}" />
    /// <seealso cref="IFacetMetadata" />
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RepositoryAttribute : Attribute, IFacetMetadata, IRepositoryParameterConfiguration
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryAttribute"/> class.
        /// </summary>
        public RepositoryAttribute(string storageName, string? configurationName = null, bool preventAnyKindOfDiscriminatorUsage = false)
        {
            this.StorageName = storageName;
            this.ConfigurationName = configurationName;
            this.PreventAnyKindOfDiscriminatorUsage = preventAnyKindOfDiscriminatorUsage;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string StorageName { get; }

        /// <inheritdoc />
        public string? ConfigurationName { get; }

        /// <summary>
        /// Gets a value indicating whether prevent any kind of discriminator usage.
        /// </summary>
        public bool PreventAnyKindOfDiscriminatorUsage { get; }

        #endregion
    }
}
