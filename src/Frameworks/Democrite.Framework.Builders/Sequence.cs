// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Sequences;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// Sequence entry point
    /// </summary>
    public static class Sequence
    {
        /// <summary>
        /// Creates a new instance of <see cref="SequenceBuilder"/>
        /// </summary>
        public static ISequenceTriggerBuilder Build(string displayName, Guid? fixUid = null, Action<ISequenceOptionBuilder>? optionBuilder = null)
        {
            SequenceOptionDefinition? option = null;

            if (optionBuilder is not null)
            {
                var optBuilder = new SequenceOptionBuilder();
                optionBuilder(optBuilder);

                option = optBuilder.Build();
            }

            var builder = new SequenceBuilder(fixUid ?? Guid.NewGuid(), "SEQ:" + displayName, option);
            return builder;
        }
    }
}
