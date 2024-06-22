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
    [GenerateSerializer]
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
                                              bool secureDataTansfert,
                                              DefinitionMetaData? metadata)
        {
            this.Uid = uid;
            this.MetaData = metadata;
            this.DisplayName = displayName;
            this.ConfigName = configName;
            this.ExpectedConfigurationType = expectedConfigurationType;
            this.SecureDataTansfert = secureDataTansfert;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        [Id(0)]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the name of the configuration.
        /// </summary>
        [DataMember]
        [Id(1)]
        public string ConfigName { get; }

        /// <summary>
        /// Gets the expected type of the configuration.
        /// </summary>
        [DataMember]
        [Id(2)]
        public ConcretBaseType ExpectedConfigurationType { get; }

        /// <summary>
        /// Gets a value indicating whether [secure data tansfer].
        /// </summary>
        [DataMember]
        [Id(3)]
        public bool SecureDataTansfert { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(4)]
        public Guid Uid { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(5)]
        public DefinitionMetaData? MetaData { get; }

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
