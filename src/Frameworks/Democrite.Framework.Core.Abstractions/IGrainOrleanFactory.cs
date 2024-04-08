// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    /// <summary>
    /// Placeholder proxy of the root <see cref="IGrainFactory"/> without any redirection rules
    /// </summary>
    /// <seealso cref="Orleans.IGrainFactory" />
    /// <remarks>
    ///     Mainly used by the democrite system grain to prevent any by pass
    /// </remarks>
    public interface IGrainOrleanFactory : IGrainFactory
    {
    }
}
