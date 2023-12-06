﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Triggers;

    /// <summary>
    /// Common part of trigger definition
    /// </summary>
    public interface ITriggerDefinitionFinalizeBuilder : ITriggerDefinitionBuilder, IDefinitionBaseBuilder<TriggerDefinition>
    {
        /// <summary>
        /// Configure how to pick input message that will be fire when trigged.
        /// </summary>
        ITriggerDefinitionFinalizeBuilder SetInputSource(Func<ITriggerInputBuilder, InputSourceDefinition> inputBuilders);
    }
}
