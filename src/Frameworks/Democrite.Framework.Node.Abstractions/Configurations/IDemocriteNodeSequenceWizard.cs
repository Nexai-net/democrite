// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions;

    /// <summary>
    /// Wizard to configure worflows
    /// </summary>
    public interface IDemocriteNodeSequenceWizard
    {
        /// <summary>
        /// Registers a sequence definition.
        /// </summary>
        IDemocriteNodeSequenceWizard Register(params SequenceDefinition[] sequenceDefinition);

        /// <summary>
        /// Registers <typeparamref name="TSequenceModule"/>
        /// </summary>
        IDemocriteNodeSequenceWizard Register<TSequenceModule>() where TSequenceModule : ISequenceModule, new();
    }
}
