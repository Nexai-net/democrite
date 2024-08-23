// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    /// <summary>
    /// Define the minial option to create a repository, could be extend easily
    /// </summary>
    public record class RepositoryGetOptions(string StorageName, bool IsReadOnly, string? ConfigurationName = null, bool PreventAnyKindOfDiscriminatorUsage = false)
    {
        /// <summary>
        /// Clone with new <paramref name="IsReadOnly"/>
        /// </summary>
        public virtual RepositoryGetOptions WithIsReadOnly(bool isReadOnly)
        {
            return new RepositoryGetOptions(this.StorageName, isReadOnly, this.ConfigurationName, this.PreventAnyKindOfDiscriminatorUsage);
        }

        /// <summary>
        /// Clone with new <paramref name="preventAnyKindOfDiscriminatorUsage"/>
        /// </summary>
        public virtual RepositoryGetOptions WithPreventAnyKindOfDiscriminatorUsage(bool preventAnyKindOfDiscriminatorUsage)
        {
            return new RepositoryGetOptions(this.StorageName, this.IsReadOnly, this.ConfigurationName, preventAnyKindOfDiscriminatorUsage);
        }

        /// <summary>
        /// Clone with new <paramref name="storageName"/>
        /// </summary>
        public virtual RepositoryGetOptions WithStorageName(string storageName)
        {
            return new RepositoryGetOptions(storageName, this.IsReadOnly, this.ConfigurationName, this.PreventAnyKindOfDiscriminatorUsage);
        }

        /// <summary>
        /// Clone with new <paramref name="configurationName"/>
        /// </summary>
        public virtual RepositoryGetOptions WithConfigurationName(string configurationName)
        {
            return new RepositoryGetOptions(this.StorageName, this.IsReadOnly, configurationName, this.PreventAnyKindOfDiscriminatorUsage);
        }
    }
}
