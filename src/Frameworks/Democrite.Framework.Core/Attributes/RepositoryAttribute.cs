// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using Orleans;
    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Attribute used to specify witch storage you want service <see cref="IReadOnlyRepository{TEntity}"/> dedicated
    /// </summary>
    /// <seealso cref="IAttributeToFactoryMapper{StorageRepositoryAttribute}" />
    /// <seealso cref="IFacetMetadata" />
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RepositoryAttribute : Attribute, IFacetMetadata
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryAttribute"/> class.
        /// </summary>
        public RepositoryAttribute(string stateName, string storageName)
        {
            this.StorageName = storageName;
            this.StateName = stateName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the storage.
        /// </summary>
        public string StorageName { get; }

        /// <summary>
        /// Gets the name of the state.
        /// </summary>
        public string StateName { get; }

        #endregion
    }
}
