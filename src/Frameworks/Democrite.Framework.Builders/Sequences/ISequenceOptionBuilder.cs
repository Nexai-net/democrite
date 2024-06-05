// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Microsoft.Extensions.Logging;

    public interface ISequenceOptionBuilder
    {
        /// <summary>
        /// Set minimal log level
        /// </summary>
        ISequenceOptionBuilder MinimalLogLevel(LogLevel logLevel);

        /// <summary>
        /// Prevents the sequence executor state storage.
        /// </summary>
        ISequenceOptionBuilder PreventSequenceExecutorStateStorage();
    }
}
