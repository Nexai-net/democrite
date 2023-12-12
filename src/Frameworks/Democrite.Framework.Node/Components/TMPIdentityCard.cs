// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Components
{
    using Democrite.Framework.Core.Abstractions;

    using System.Threading.Tasks;

    /// <summary>
    /// Temporary identity card used to enabled behavior through framework
    /// For know <see cref="IComponentIdentityCard"/> are mainly use to unit test purpose to disable to behavior
    /// </summary>
    /// <seealso cref="IComponentDoorIdentityCard" />
    [GenerateSerializer]
    internal class TMPIdentityCard : IComponentDoorIdentityCard
    {
        public ValueTask<bool> CanBeStimuate()
        {
            return ValueTask.FromResult(true);
        }

        public ValueTask<bool> IsEnable()
        {
            return ValueTask.FromResult(true);
        }
    }
}
