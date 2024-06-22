// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates;

    using System;

    [GenerateSerializer]
    public record struct BlackboardTemplateDefinitionSurrogate(Guid Uid,
                                                               string UniqueTemplateName,
                                                               IReadOnlyCollection<BlackboardTemplateControllerDefinition>? Controllers,
                                                               IReadOnlyCollection<IBlackboardLogicalTypeBaseRuleSurrogate> Rules,
                                                               DefinitionMetaData? DefinitionMeta,
                                                               BlackboardTemplateConfigurationDefinition? ConfigurationDefinition = null,
                                                               BlackboardStorageDefinition? DefaultStorageConfig = null);

    [RegisterConverter]
    public sealed class BlackboardTemplateDefinitionConverter : IConverter<BlackboardTemplateDefinition, BlackboardTemplateDefinitionSurrogate>
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        /// <returns></returns>
        static BlackboardTemplateDefinitionConverter()
        {
            Default = new BlackboardTemplateDefinitionConverter();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static BlackboardTemplateDefinitionConverter Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public BlackboardTemplateDefinition ConvertFromSurrogate(in BlackboardTemplateDefinitionSurrogate surrogate)
        {
            return new BlackboardTemplateDefinition(surrogate.Uid,
                                                    surrogate.UniqueTemplateName,
                                                    surrogate.Controllers,
                                                    BlackboardLogicalTypeBaseRuleSurrogateConverter.ConvertFromSurrogate(surrogate.Rules),
                                                    surrogate.DefinitionMeta,
                                                    surrogate.ConfigurationDefinition,
                                                    surrogate.DefaultStorageConfig);
        }

        /// <inheritdoc />
        public BlackboardTemplateDefinitionSurrogate ConvertToSurrogate(in BlackboardTemplateDefinition value)
        {
            return new BlackboardTemplateDefinitionSurrogate()
            {
                Uid = value.Uid,
                UniqueTemplateName = value.UniqueTemplateName,
                Controllers = value.Controllers,
                Rules = BlackboardLogicalTypeBaseRuleSurrogateConverter.ConvertToSurrogate(value.LogicalTypes).ToReadOnly(),
                DefaultStorageConfig = value.DefaultStorageConfig,
                ConfigurationDefinition = value.ConfigurationDefinition,
                DefinitionMeta = value.MetaData
            };
        }

        #endregion

    }
}
