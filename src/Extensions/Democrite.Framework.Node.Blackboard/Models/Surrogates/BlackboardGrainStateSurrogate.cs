// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models.Surrogates
{
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    [GenerateSerializer]
    internal record struct BlackboardGrainStateSurrogate(BlackboardTemplateDefinitionSurrogate? TemplateDefinition,
                                                         BlackboardId BlackboardId,
                                                         string Name,
                                                         BlackboardRecordRegistryStateSurrogate BlackboardRecordRegistryState);

    [RegisterConverter]
    internal sealed class BlackboardGrainStateConverter : IConverter<BlackboardGrainState, BlackboardGrainStateSurrogate>
    {
        /// <inheritdoc />
        public BlackboardGrainState ConvertFromSurrogate(in BlackboardGrainStateSurrogate surrogate)
        {
            return new BlackboardGrainState((surrogate.TemplateDefinition is not null ? BlackboardTemplateDefinitionConverter.Default.ConvertFromSurrogate(surrogate.TemplateDefinition.Value) : null),
                                            surrogate.BlackboardId,
                                            surrogate.Name,
                                            BlackboardRecordRegistryStateConverter.Default.ConvertFromSurrogate(surrogate.BlackboardRecordRegistryState));
        }

        /// <inheritdoc />
        public BlackboardGrainStateSurrogate ConvertToSurrogate(in BlackboardGrainState value)
        {
            var surrogate = new BlackboardGrainStateSurrogate()
            {
                TemplateDefinition = (value.TemplateCopy is not null ? BlackboardTemplateDefinitionConverter.Default.ConvertToSurrogate(value.TemplateCopy) : null),
                BlackboardId = value.BlackboardId,
                Name = value.Name,
                BlackboardRecordRegistryState = BlackboardRecordRegistryStateConverter.Default.ConvertToSurrogate(value.Registry)
            };

            return surrogate;
        }
    }
}
