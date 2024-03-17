// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.UnitTests
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Node.Signals.Doors;
    using Democrite.Framework.Node.Signals.Models;
    using Democrite.UnitTests.ToolKit.Tests;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    /// <summary>
    /// Test for <see cref="DoorHandlerStateSurrogate"/>
    /// </summary>
    public sealed class DoorHandlerStateSurrogateUTest : SurrogateBaseTest<DoorHandlerState, DoorHandlerStateSurrogate, DoorHandlerStateSurrogateConverter>
    {
        #region Methods

        /// <inheritdoc />
        protected override DoorHandlerState SourceCreation(Fixture fixture)
        {
            fixture = ObjectTestHelper.PrepareFixture(supportMutableValueType: true, supportCyclingReference: true);

            var state = new DoorHandlerState(fixture.CreateMany<DoorSignalReceivedStatusSurrogate>(), null);

            var def = fixture.Create<BooleanLogicalDoorDefinition>();

            state.InitializeSignalSupport(def);

            return state;
        }

        #endregion
    }
}
