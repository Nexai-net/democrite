// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    using Democrite.Framework.Core.Abstractions.Triggers;

    /// <summary>
    /// Common part of trigger definition
    /// </summary>
    public interface ITriggerDefinitionStreamFinalizeBuilder : ITriggerDefinitionBuilder<ITriggerDefinitionStreamFinalizeBuilder>, IDefinitionBaseBuilder<TriggerDefinition>
    {
        /// <summary>
        /// Set the maximumn of concurrent message processing; Default 1000 - Minimun 1
        /// </summary>
        IDefinitionBaseBuilder<TriggerDefinition> MaxConcurrentProcess(uint maxConcurrent);
    }
}
