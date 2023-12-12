// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Component information dedicate to door
    /// </summary>
    /// <seealso cref="IComponentIdentityCard" />
    public interface IComponentDoorIdentityCard : IComponentIdentityCard
    {
        /// <summary>
        /// Determines whether the door could be stimulate or should only accumulate inputs
        /// </summary>
        ValueTask<bool> CanBeStimuate();
    }
}
