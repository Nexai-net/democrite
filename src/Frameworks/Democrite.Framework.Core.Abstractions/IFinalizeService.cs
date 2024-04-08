// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Elvex.Toolbox.Abstractions.Supports;

    /// <summary>
    /// Service at node level that need to be finalized at stop
    /// </summary>
    /// <seealso cref="ISupportFinalization" />
    public interface IFinalizeService : ISupportFinalization
    {
    }
}
