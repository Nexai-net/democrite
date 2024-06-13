// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Configurations
{
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public abstract class ConfigurationBaseDefinition : ISupportDebugDisplayName, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBaseDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        protected ConfigurationBaseDefinition(Guid uid,
                                              string displayName,
                                              string configName,
                                              ConcretBaseType expectedConfigurationType,
                                              bool secureDataTansfert)
        {
            this.Uid = uid;
            this.DisplayName = displayName;
            this.ConfigName = configName;
            this.ExpectedConfigurationType = expectedConfigurationType;
            this.SecureDataTansfert = secureDataTansfert;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the name of the configuration.
        /// </summary>
        [DataMember]
        public string ConfigName { get; }

        /// <summary>
        /// Gets the expected type of the configuration.
        /// </summary>
        [DataMember]
        public ConcretBaseType ExpectedConfigurationType { get; }

        /// <summary>
        /// Gets a value indicating whether [secure data tansfer].
        /// </summary>
        [DataMember]
        public bool SecureDataTansfert { get; }

        /// <inheritdoc />
        [DataMember]
        public Guid Uid { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return "[{0}] - {1} - {2}".WithArguments(this.DisplayName, this.ConfigName, this.ExpectedConfigurationType);
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
