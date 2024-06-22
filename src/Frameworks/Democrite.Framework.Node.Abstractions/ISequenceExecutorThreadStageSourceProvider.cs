// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Source Provider of <see cref="ISequenceExecutorThreadStageHandler"/>
    /// </summary>
    internal interface ISequenceExecutorThreadStageSourceProvider
    {
        /// <summary>
        /// Determines whether this instance can handler the specified stage.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handler the specified stage; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandler(SequenceStageDefinition stage);

        /// <summary>
        /// Provides a specified stage dedicated <see cref="ISequenceExecutorThreadStageHandler"/>
        /// </summary>
        ISequenceExecutorThreadStageHandler Provide(SequenceStageDefinition stage);
    }
}
