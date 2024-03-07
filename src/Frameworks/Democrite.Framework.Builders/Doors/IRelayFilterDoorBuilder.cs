// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Doors
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System.Linq.Expressions;

    /// <summary>
    /// Builder that create a <see cref="RelayFilterDoorDefinition"/>
    /// </summary>
    public interface IRelayFilterDoorBuilder
    {
        /// <summary>
        /// Donts the relay the signal conent.
        /// </summary>
        IRelayFilterDoorBuilder DontRelaySignalContent();

        /// <summary>
        /// Condition a signal must validate to pass
        /// </summary>
        IDefinitionBaseBuilder<DoorDefinition> Condition<TContent>(Expression<Func<TContent, SignalMessage, bool>> filterExpression) where TContent : struct;

        /// <summary>
        /// Condition a signal must validate to pass
        /// </summary>
        IDefinitionBaseBuilder<DoorDefinition> Condition(Expression<Func<SignalMessage, bool>> filterExpression);
    }
}
