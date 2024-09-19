// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.UnitTests.Signals
{
    using Democrite.Framework.Builders.Doors;
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Elvex.Toolbox.Abstractions.Enums;
    using Elvex.Toolbox.Helpers;

    using NFluent;

    /// <summary>
    /// Test For <see cref="LogicalDoorBuilder"/>
    /// </summary>
    public sealed class LogicalDoorBuilderUTest : DoorBaseBuilderUTest<BooleanLogicalDoorDefinition>
    {
        #region Methods

        /// <summary>
        /// Builds the simple logic door.
        /// </summary>
        [Theory]
        [InlineData(LogicEnum.And)]
        [InlineData(LogicEnum.Or)]
        [InlineData(LogicEnum.ExclusiveOr)]
        public void Build_Simple_Logic_Door(LogicEnum logicEnum)
        {
            var internval = TimeSpan.FromSeconds(Random.Shared.Next(10, 1000));
            var signalA = Signal.Create("signal-a");
            var signalB = Signal.Create("signal-b");
            var signalC = Signal.Create("signal-c");

            var door = Door.Create("door-a", fixUid: Guid.NewGuid())
                           .Listen(signalA)
                           .Listen(signalB)
                           .Listen(signalC)
                           .UseLogicalAggregator(logicEnum, internval)
                           .Build();

            Check.That(door).IsNotNull().And.IsInstanceOf<BooleanLogicalDoorDefinition>();
            Check.That(door.VGrainInterfaceFullName).IsNotNull().And.IsEqualTo(typeof(ILogicalDoorVGrain).AssemblyQualifiedName);
            Check.That(door.DoorSourceIds).IsNotNull().And.CountIs(0);

            Check.That(door.SignalSourceIds).IsNotNull()
                                            .And
                                            .ContainsNoDuplicateItem()
                                            .And
                                            .ContainsExactly(signalA.SignalId,
                                                             signalB.SignalId,
                                                             signalC.SignalId);

            Check.That(door.DoorId).IsNotEqualTo(default);

            var logic = (BooleanLogicalDoorDefinition)door;

            Check.That(logic.VariableNames).IsNotNull()
                                           .And
                                           .ContainsExactly(new KeyValuePair<string, Guid>("A", signalA.Uid),
                                                            new KeyValuePair<string, Guid>("B", signalB.Uid),
                                                            new KeyValuePair<string, Guid>("C", signalC.Uid));

            Check.That(logic.UseCurrentDoorStatus).IsFalse();

            char operatorChar = LogicHelper.GetSymbolFrom(logicEnum) ?? throw new InvalidOperationException(logicEnum + " not managed.");

            var expectedFormula = string.Join(" " + operatorChar + " ", logic.VariableNames.Keys.OrderBy(k => k).Select(k => k));

            Check.That(logic.LogicalFormula).IsNotNull().And.IsEqualTo(expectedFormula);
        }

        /// <summary>
        /// Builds advanced logic door.
        /// </summary>
        [Fact]
        public void Build_Advanced_Logic_Door()
        {
            var internval = TimeSpan.FromSeconds(Random.Shared.Next(10, 1000));

            var expectedFormula = "A ^ B & C | !this";

            var signalA = Signal.Create("signal-a");
            var signalB = Signal.Create("signal-b");
            var signalC = Signal.Create("signal-c");

            var thisId = Guid.NewGuid();

            var door = Door.Create("door-a", fixUid: thisId)
                           .Listen(signalA)
                           .Listen(signalB)
                           .Listen(signalC)
                           .UseLogicalAggregator(b =>
                           {
                               return b.ActiveWindowInterval(internval)
                                       .UseVariableThis()
                                       .AssignVariableName("C", signalA)
                                       .AssignVariableName("B", signalB.SignalId)
                                       .AssignVariableName("A", signalC.SignalId)
                                       .Formula(expectedFormula);
                           })
                           .Build();

            Check.That(door).IsNotNull().And.IsInstanceOf<BooleanLogicalDoorDefinition>();
            Check.That(door.VGrainInterfaceFullName).IsNotNull().And.IsEqualTo(typeof(ILogicalDoorVGrain).AssemblyQualifiedName);
            Check.That(door.DoorSourceIds).IsNotNull().And.CountIs(0);

            Check.That(door.SignalSourceIds).IsNotNull()
                                            .And
                                            .ContainsNoDuplicateItem()
                                            .And
                                            .ContainsExactly(signalA.SignalId,
                                                             signalB.SignalId,
                                                             signalC.SignalId);

            Check.That(door.DoorId).IsNotEqualTo(default);

            var logic = (BooleanLogicalDoorDefinition)door;

            Check.That(logic.VariableNames).IsNotNull()
                                           .And
                                           .ContainsExactly(new KeyValuePair<string, Guid>("this", thisId),
                                                            new KeyValuePair<string, Guid>("C", signalA.Uid),
                                                            new KeyValuePair<string, Guid>("B", signalB.Uid),
                                                            new KeyValuePair<string, Guid>("A", signalC.Uid));

            Check.That(logic.UseCurrentDoorStatus).IsTrue();
            Check.That(logic.LogicalFormula).IsNotNull().And.IsEqualTo(expectedFormula);
        }

        #endregion

        #region Tools

        /// <inheritdoc />
        protected override DoorDefinition BuildSimpleDefinition(IDoorWithListenerBuilder rootBuilder)
        {
            return rootBuilder.UseLogicalAggregator()
                              .Build();
        }

        #endregion
    }
}
