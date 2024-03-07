// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Doors
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;

    /// <summary>
    /// Configure the logical behavior of the door
    /// </summary>
    public interface ILogicalDoorBuilder
    {
        /// <summary>
        /// Set the time interval when signal activation are interpret
        /// </summary>
        /// <example>
        ///     If all signal arrive in the <paramref name="interval"/> and Logical condition are simply <see cref="LogicEnum.And"/> then door signal is send.
        /// </example>
        ILogicalDoorBuilder ActiveWindowInterval(TimeSpan interval);

        /// <summary>
        /// Assigns a variable name to <paramref name="signalId"/> source; Variable used in the formula.
        /// </summary>
        ILogicalDoorBuilder ListenWindowMode(ListenWindowModeEnum windowModeEnum);

        /// <summary>
        /// Assigns a variable name to <paramref name="signalId"/> source; Variable used in the formula.
        /// </summary>
        ILogicalDoorBuilder AssignVariableName(string variableName, SignalId signalId);

        /// <summary>
        /// Assigns a variable name to <paramref name="signalDefinition"/> source; Variable used in the formula.
        /// </summary>
        ILogicalDoorBuilder AssignVariableName(string variableName, SignalDefinition signalDefinition);

        /// <summary>
        /// Assigns a variable name to <paramref name="doorDefinition"/> source; Variable used in the formula.
        /// </summary>
        ILogicalDoorBuilder AssignVariableName(string variableName, DoorDefinition doorDefinition);

        /// <summary>
        /// Assigns a variable name to <paramref name="doorId"/> source; Variable used in the formula.
        /// </summary>
        ILogicalDoorBuilder AssignVariableName(string variableName, DoorId doorId);

        /// <summary>
        /// Uses the variable this.
        /// </summary>
        ILogicalDoorBuilder UseVariableThis();

        /// <summary>
        /// Conditions express like "(A & B) | C" when operator are <br/>
        /// <c>&</c> = And <br />
        /// <c>|</c> = Or <br />
        /// <c>^</c> = Xor <br />
        /// <c>!</c> = Not <br />
        /// </summary>
        /// <remarks>
        ///     Variable 'this' represent the current door.
        ///     Could be used full to prevent sending signal if it was already in the window.
        ///     
        ///     Example: (A & B) & !this
        ///     Will send when A and B activate in the interval window except if signal was already then in the current window
        /// </remarks>
        IDefinitionBaseBuilder<DoorDefinition> Formula(string condition);
    }
}
