// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.UnitTests.Door
{
    using AutoFixture;

    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Signals.Doors;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="RelayFilterDoorVGrain"/>
    /// </summary>
    public sealed class RelayFilterDoorVGrainUTest
    {
        #region Fields

        private readonly DoorVGrainBaseUTest<RelayFilterDoorVGrain, RelayFilterDoorDefinition, IRelayFilterVGrain> _tester;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayFilterDoorVGrainUTest"/> class.
        /// </summary>
        public RelayFilterDoorVGrainUTest()
        {
            this._tester = new DoorVGrainBaseUTest<RelayFilterDoorVGrain, RelayFilterDoorDefinition, IRelayFilterVGrain>(DefBuilder);
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
        /// Build <see cref="RelayFilterDoorDefinition"/>
        /// </summary>
        private RelayFilterDoorDefinition DefBuilder(Fixture fixture, string? methodName)
        {
            const int SUCESS_MSG = DoorVGrainBaseUTest<RelayFilterDoorVGrain, RelayFilterDoorDefinition, IRelayFilterVGrain>.SUCESS_MSG;

            return new RelayFilterDoorDefinition(Guid.NewGuid(),
                                                 RefIdHelper.Generate(RefTypeEnum.Door, "door-test", "unit.test"),
                                                 fixture.Create<string>(),
                                                 null!,
                                                 fixture.Create<IEnumerable<SignalId>>(),
                                                 fixture.Create<IEnumerable<DoorId>>(),
                                                 ExpressionExtensions.Serialize((int arg, SignalMessage sm) => arg == SUCESS_MSG),
                                                 false,
                                                 null);
        }

        #endregion

        #endregion
    }
}
