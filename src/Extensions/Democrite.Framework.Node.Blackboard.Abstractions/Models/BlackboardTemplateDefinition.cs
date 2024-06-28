// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a blackboard
    /// </summary>
    /// <seealso cref="IDefinition" />
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class BlackboardTemplateDefinition : IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateDefinition"/> class.
        /// </summary>
        public BlackboardTemplateDefinition(Guid uid,
                                            string uniqueTemplateName,
                                            IEnumerable<BlackboardTemplateControllerDefinition>? controllers,
                                            IEnumerable<BlackboardLogicalTypeBaseRule> logicalTypes,
                                            DefinitionMetaData? metaData,
                                            BlackboardTemplateConfigurationDefinition? configurationDefinition = null,
                                            BlackboardStorageDefinition? defaultStorageConfig = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(uid));
            ArgumentNullException.ThrowIfNull(nameof(uniqueTemplateName));

            this.Uid = uid;
            this.MetaData = metaData;
            this.UniqueTemplateName = uniqueTemplateName;
            this.DefaultStorageConfig = defaultStorageConfig ?? new BlackboardStorageDefinition(BlackboardConstants.BlackboardStorageRecordsKey, BlackboardConstants.BlackboardStateStorageConfigurationKey);
            this.DisplayName = "Blackboard Tpl: " + uniqueTemplateName;
            this.Controllers = controllers?.ToArray();
            this.LogicalTypes = logicalTypes?.ToArray() ?? EnumerableHelper<BlackboardLogicalTypeBaseRule>.ReadOnlyArray;
            this.ConfigurationDefinition = configurationDefinition ?? BlackboardTemplateConfigurationDefinition.DefaultConfiguration;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        public Guid Uid { get; }

        /// <inheritdoc />
        [DataMember]
        public DefinitionMetaData? MetaData { get; }

        /// <summary>
        /// Gets the unique template name
        /// </summary>
        [DataMember]
        public string UniqueTemplateName { get; }

        /// <inheritdoc />
        [DataMember]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the controller definitions.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<BlackboardTemplateControllerDefinition>? Controllers { get; }

        /// <summary>
        /// Gets the logicalTypes.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<BlackboardLogicalTypeBaseRule> LogicalTypes { get; }

        /// <summary>
        /// Gets the configuration definition.
        /// </summary>
        [DataMember]
        public BlackboardTemplateConfigurationDefinition ConfigurationDefinition { get; }

        /// <summary>
        /// Gets the default storage configuration.
        /// </summary>
        [DataMember]
        public BlackboardStorageDefinition DefaultStorageConfig { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return this.DisplayName;
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            bool isValid = true;

            if (this.Controllers is not null && this.Controllers.Any())
            {
                isValid &= this.Controllers.Select(c => c.Validate(logger, matchWarningAsError))
                                           .Aggregate(true, (acc, result) => acc & result);
            }

            return isValid;
        }

        #endregion
    }
}
