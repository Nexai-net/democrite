// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Sequence builder
    /// </summary>
    public interface ISequenceBuilder : ISequenceTriggerBuilder, IDefinitionBaseBuilder<SequenceDefinition>
    {
        #region Properties

        /// <summary>
        /// Gets the parent builder.
        /// </summary>
        ISequenceBuilder? Parent { get; }

        #endregion

    }
}
