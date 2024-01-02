// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Sequences;

    using System;

    /// <summary>
    /// Sequence entry point
    /// </summary>
    public static class Sequence
    {
        /// <summary>
        /// Creates a new instance of <see cref="SequenceBuilder"/>
        /// </summary>
        public static ISequenceTriggerBuilder Build(Guid? uid = null, Action<SequenceOption>? optionBuilder = null)
        {
            var option = new SequenceOption(uid, null);

            optionBuilder?.Invoke(option);

            var builder = new SequenceBuilder(uid ?? Guid.NewGuid(), option.DisplayName, option.ToDefinition());
            return builder;
        }
    }
}
