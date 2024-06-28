// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions;

    /// <summary>
    /// Handler vgrain associate to a trigger kind
    /// </summary>
    public interface ITriggerHandlerVGrain : IVGrain
    {
        /// <summary>
        /// Update trigger status based on <see cref="ITriggerDefinition"/>
        /// </summary>
        Task UpdateAsync(GrainCancellationToken token);
    }
}
