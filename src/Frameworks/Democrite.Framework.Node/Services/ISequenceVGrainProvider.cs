// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Service used to controler <see cref="IVGrainProvider"/> based on condition
    /// </summary>
    internal interface ISequenceVGrainProvider
    {
        /// <summary>
        /// Gets the grain provider dedicated based on
        /// </summary>
        IVGrainProvider GetGrainProvider(ref readonly SequenceStageDefinition step);
    }
}
