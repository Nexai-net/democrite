// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    /// <summary>
    /// VGrain in charge to build the re
    /// </summary>
    /// <seealso cref="IVGrain" />

    [VGrainStatelessWorker]
    public interface IBlackboardStorageRequestBuilderVGrain : IVGrain
    {
    }
}
