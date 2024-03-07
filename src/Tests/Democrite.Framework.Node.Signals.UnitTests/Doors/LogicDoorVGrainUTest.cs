// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.UnitTests.Door
{
    using AutoFixture;

    using Democrite.Framework.Builders.Doors;
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Signals.Doors;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DoorTest = DoorVGrainBaseUTest<Democrite.Framework.Node.Signals.Doors.LogicalDoorVGrain, Democrite.Framework.Core.Abstractions.Doors.BooleanLogicalDoorDefinition, Democrite.Framework.Core.Abstractions.Doors.ILogicalDoorVGrain>;

    /// <summary>
    /// Test for <see cref="RelayFilterDoorVGrain"/>
    /// </summary>
    public sealed class LogicDoorVGrainUTest
    {
        #region Fields

        private readonly DoorTest _tester;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicDoorVGrainUTest"/> class.
        /// </summary>
        public LogicDoorVGrainUTest()
        {
            this._tester = new DoorTest(DefBuilder);
        }

        #region Methods

        /// <summary>
        /// Test the door subscription to all describe signals
        /// </summary>
        [Fact]
        public async Task Door_Subscribe()
        {
            await this._tester.Door_Subscribe();
        }

        /// <summary>
        /// Test if door correctly managed signal reception
        /// </summary>
        [Fact]
        public async Task Door_Signal_Received()
        {
            await this._tester.Door_Signal_Received();
        }

        #region Tools

        /// <summary>
        /// Build <see cref="BooleanLogicalDoorDefinition"/>
        /// </summary>
        private BooleanLogicalDoorDefinition DefBuilder(Fixture fixture, string methodCalling)
        {
            var signals = fixture.Create<IEnumerable<SignalId>>();
            var doors = fixture.Create<IEnumerable<DoorId>>();

            var door = Democrite.Framework.Builders.Door.Create(fixture.Create<string>());

            IDoorWithListenerBuilder? doorWithListener = null;

            foreach (var s in signals)
                doorWithListener = door.Listen(s);

            foreach (var d in doors)
                doorWithListener = door.Listen(d);

            return doorWithListener?.UseLogicalAggregator(interval: TimeSpan.FromSeconds(10))
                                    .Build() as BooleanLogicalDoorDefinition ?? throw new InvalidOperationException("Logic door unit test setup failed");
        }

        #endregion

        #endregion
    }
}
