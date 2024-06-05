// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class BlackboardStorageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardStorageDefinition"/> class.
        /// </summary>
        public BlackboardStorageDefinition(string storageKey,
                                           string? storageConfiguration = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(storageKey);

            this.StorageKey = storageKey;
            this.StorageConfiguration = string.IsNullOrEmpty(storageConfiguration) ? DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey: storageConfiguration;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets target's storage key.
        /// </summary>
        [DataMember]
        public string StorageKey { get; }

        /// <summary>
        /// Gets targett's storage configuration.
        /// </summary>
        [DataMember]
        public string StorageConfiguration { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"{this.StorageKey}:{this.StorageConfiguration}";
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
