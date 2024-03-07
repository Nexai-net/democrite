// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)
namespace Democrite.Framework.Node.Blackboard.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Models;

    using System.Collections.Generic;

    [GenerateSerializer]
    internal record struct BlackboardRegistryStateSurrogate(IReadOnlyCollection<BlackboardId> BoardIds);

    [RegisterConverter]
    internal sealed class BlackboardRegistryStateConverter : IConverter<BlackboardRegistryState, BlackboardRegistryStateSurrogate>
    {
        /// <inheritdoc />
        public BlackboardRegistryState ConvertFromSurrogate(in BlackboardRegistryStateSurrogate surrogate)
        {
            return new BlackboardRegistryState(surrogate.BoardIds);
        }

        /// <inheritdoc />
        public BlackboardRegistryStateSurrogate ConvertToSurrogate(in BlackboardRegistryState value)
        {
            return value.ToSurrogate();
        }
    }
}
