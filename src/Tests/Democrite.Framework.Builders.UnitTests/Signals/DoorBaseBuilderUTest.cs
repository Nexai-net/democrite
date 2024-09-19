// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.UnitTests.Signals
{
    using Democrite.Framework.Builders.Doors;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using NFluent;

    /// <summary>
    /// Base class of door builders use to ensure default value are well setups
    /// </summary>
    public abstract class DoorBaseBuilderUTest<TDoorDefinition>
        where TDoorDefinition : DoorDefinition
    {
        /// <summary>
        /// Doors the build default values.
        /// </summary>
        [Fact]
        public void DoorBuild_DefaultValues()
        {
            var name = "name" + Guid.NewGuid();
            var id = Guid.NewGuid();

            var rootSimple = Door.Create(name, fixUid: id)
                                 .Listen(new SignalId(Guid.NewGuid(), "first"))
                                 .Listen(new SignalId(Guid.NewGuid(), "second"));
                                 
            var simpleDefinition = BuildSimpleDefinition(rootSimple) as TDoorDefinition;

            Check.That(simpleDefinition).IsNotNull();
            Check.That(simpleDefinition!.VGrainInterfaceFullName).IsNotNull().And.IsNotEmpty();
            Check.That(simpleDefinition!.SignalSourceIds).IsNotNull();
            Check.That(simpleDefinition!.DoorSourceIds).IsNotNull();
            Check.That(simpleDefinition!.DoorId.Uid).IsEqualTo(simpleDefinition.Uid);
            Check.That(simpleDefinition!.DoorId.Name).IsNotNull().And.IsNotEmpty();
            Check.That(simpleDefinition!.HistoryMaxRetention).IsEqualTo(DoorDefinition.DEFAULT_HISTORY_RETENTION);
            Check.That(simpleDefinition!.NotConsumedMaxRetiention).IsEqualTo(DoorDefinition.DEFAULT_NOT_CONSUMED_RETENTION);
            Check.That(simpleDefinition!.RetentionMaxDelay).IsEqualTo(DoorDefinition.DEFAULT_RETENTION_MAX_DELAY);
        }

        /// <summary>
        /// Builds the simple definition <see cref="TDoorDefinition"/> using a maximum of default values
        /// </summary>
        protected abstract DoorDefinition BuildSimpleDefinition(IDoorWithListenerBuilder doorBuilder);
    }
}
