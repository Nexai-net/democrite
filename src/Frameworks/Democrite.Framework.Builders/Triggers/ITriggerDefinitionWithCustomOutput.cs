// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System;

    public interface ITriggerDefinitionWithCustomOutput
    {
        /// <summary>
        /// Configure how to output message that will be send on fire (Sequence, Signal and/or stream)
        /// </summary>
        ITriggerDefinitionBuilder SetOutput(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> outputBuilders);

        /// <summary>
        /// Configure how to output message that will be send on fire (Sequence, Signal and/or stream)
        /// </summary>
        ITriggerDefinitionBuilder SetOutput(DataSourceDefinition outputdef);
    }
}
