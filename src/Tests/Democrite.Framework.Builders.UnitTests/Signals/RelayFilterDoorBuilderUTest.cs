// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.UnitTests.Signals
{
    using AutoFixture;

    using Democrite.Framework.Builders.Doors;
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using Orleans.Runtime;

    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Test for <see cref="RelayFilterDoorBuilder"/>
    /// </summary>
    public sealed class RelayFilterDoorBuilderUTest : DoorBaseBuilderUTest<RelayFilterDoorDefinition>
    {
        #region Models

        public enum SignalCarryTypeEnum
        {
            None,
            Opt1,
            Fail,
            All,
            Poney
        }

        public record struct SignalTestMessage(Guid Uid, SignalCarryTypeEnum Type, int Value);

        #endregion

        #region Methods

        /// <summary>
        /// Test to build a door relay filter using condition about <see cref="SignalMessage"/>
        /// </summary>
        [Fact]
        public void Build_Filter_Only_On_Signal()
        {
            var signalA = Signal.Create("signal-a");

            var id = Guid.NewGuid();
            var def = Door.Create("door-test", fixUid: id, metaDataBuilder: m => m.CategoryPath("GrpA"))
                          .Listen(signalA)
                          .UseRelayFilter(r => r.Condition(s => s.From.VGrainMetaData != null && s.From.VGrainMetaData.Value.IsDemocriteSystem == false))
                          .Build();

            Check.That(def).IsNotNull().And.IsInstanceOf<RelayFilterDoorDefinition>();
            Check.That(def.Uid).IsEqualTo(id);
            Check.That(def.MetaData).IsNotNull();
            Check.That(def.MetaData!.CategoryPath).IsNotNull().And.IsEqualTo("GrpA");
            Check.That(def.Name).IsEqualTo("door-test");
            Check.That(def.VGrainInterfaceFullName).IsNotNull().And.IsEqualTo(typeof(IRelayFilterVGrain).AssemblyQualifiedName);
            Check.That(def.SignalSourceIds).IsNotNull().And.ContainsExactly(signalA.SignalId);

            var relayDef = (RelayFilterDoorDefinition)def;

            var expression = relayDef.FilterCondition.ToExpression<SignalMessage, bool>();

            Check.That(relayDef.DontRelaySignalContent).IsFalse();

            Check.That(expression).IsNotNull();

            var validSource = new SignalSource(Guid.NewGuid(),
                                               Guid.NewGuid(),
                                               "sourcSignal",
                                               false,
                                               DateTime.Now,
                                               null,
                                               new Core.Abstractions.Models.VGrainMetaData("impl".AsEnumerable(), "impl", false, null, IdFormatTypeEnum.String.AsEnumerable(), false, false, false),
                                               null);

            var invalidSource = new SignalSource(Guid.NewGuid(),
                                                 Guid.NewGuid(),
                                                 "sourcSignal",
                                                 false,
                                                 DateTime.Now,
                                                 null,
                                                 new Core.Abstractions.Models.VGrainMetaData("impl".AsEnumerable(), "impl", false, null, IdFormatTypeEnum.String.AsEnumerable(), false, false, true),
                                                 null);

            var testMsg = new SignalMessage(Guid.NewGuid(), DateTime.Now, validSource);
            var testInvalidMsg = new SignalMessage(Guid.NewGuid(), DateTime.Now, invalidSource);

            var func = expression.Compile();

            Check.ThatCode(() => func(testMsg)).WhichResult().IsTrue();
            Check.ThatCode(() => func(testInvalidMsg)).WhichResult().IsFalse();
        }

        /// <summary>
        /// Test to build a door relay filter using condition about <see cref="SignalMessage"/> with enum check
        /// </summary>
        [Fact]
        public void Build_Filter_Only_On_Signal_Enum_Filter()
        {
            var signalA = Signal.Create("signal-a");

            var def = Door.Create("door-test")
                          .Listen(signalA)
                          .UseRelayFilter<SignalTestMessage>((msg, s) => msg.Type == SignalCarryTypeEnum.Poney)
                          .Build();

            Check.That(def).IsNotNull().And.IsInstanceOf<RelayFilterDoorDefinition>();
            Check.That(def.VGrainInterfaceFullName).IsNotNull().And.IsEqualTo(typeof(IRelayFilterVGrain).AssemblyQualifiedName);
            TestRelayFilterConditions<SignalTestMessage>(def,
                                                         new SignalTestMessage(Guid.NewGuid(), SignalCarryTypeEnum.Poney, 42).AsEnumerable(),
                                                         new SignalTestMessage(Guid.NewGuid(), SignalCarryTypeEnum.Opt1, 42).AsEnumerable());
        }

        /// <summary>
        /// Test to build a door relay filter using condition about <see cref="SignalMessage{guid}"/> with <see cref="Guid"/> content without test on it.
        /// </summary>
        [Fact]
        public void Build_Filter_Only_On_Signal_With_ContentType()
        {
            var signalA = Signal.Create("signal-a");

            var id = Guid.NewGuid();
            var tecthId = Guid.NewGuid();

            var def = Door.Create("door-test", fixUid: id, metaDataBuilder: m => m.CategoryPath("GrpA"))
                          .Listen(signalA)
                          .UseRelayFilter<Guid>((id, s) => s.From.VGrainMetaData != null && s.From.VGrainMetaData.Value.IsDemocriteSystem == false)
                          .Build();

            Check.That(def).IsNotNull().And.IsInstanceOf<RelayFilterDoorDefinition>();
            Check.That(def.Uid).IsEqualTo(id);
            Check.That(def.MetaData).IsNotNull();
            Check.That(def.MetaData!.CategoryPath).IsNotNull().And.IsEqualTo("GrpA");
            Check.That(def.Name).IsEqualTo("door-test");
            Check.That(def.VGrainInterfaceFullName).IsNotNull().And.IsEqualTo(typeof(IRelayFilterVGrain).AssemblyQualifiedName);
            Check.That(def.SignalSourceIds).IsNotNull().And.ContainsExactly(signalA.SignalId);

            var relayDef = (RelayFilterDoorDefinition)def;

            var expression = relayDef.FilterCondition.ToExpression<Guid, SignalMessage, bool>();

            Check.That(expression).IsNotNull();

            var validSource = new SignalSource<Guid>(Guid.NewGuid(),
                                                     Guid.NewGuid(),
                                                     "sourcSignal",
                                                     false,
                                                     DateTime.Now,
                                                     null,
                                                     new Core.Abstractions.Models.VGrainMetaData("impl".AsEnumerable(), "impl", false, null, IdFormatTypeEnum.String.AsEnumerable(), false, false, false),
                                                     tecthId,
                                                     null);

            var invalidSource = new SignalSource(Guid.NewGuid(),
                                                 Guid.NewGuid(),
                                                 "sourcSignal",
                                                 false,
                                                 DateTime.Now,
                                                 null,
                                                 new Core.Abstractions.Models.VGrainMetaData("impl".AsEnumerable(), "impl", false, null, IdFormatTypeEnum.String.AsEnumerable(), false, false, true),
                                                 null);

            var testMsg = new SignalMessage(Guid.NewGuid(), DateTime.Now, validSource);
            var testInvalidMsg = new SignalMessage(Guid.NewGuid(), DateTime.Now, invalidSource);

            var func = expression.Compile();

            Check.ThatCode(() => func(validSource.Data, testMsg)).WhichResult().IsTrue();
            Check.ThatCode(() => func(Guid.NewGuid(), testMsg)).WhichResult().IsTrue();
            Check.ThatCode(() => func(validSource.Data, testInvalidMsg)).WhichResult().IsFalse();
        }

        /// <summary>
        /// Test to build a door relay filter using condition about <see cref="SignalMessage{guid}"/> with <see cref="Guid"/> content
        /// </summary>
        [Fact]
        public void Build_Filter_On_Full_Signal_With_ContentType()
        {
            var signalA = Signal.Create("signal-a");

            var id = Guid.NewGuid();
            var tecthId = Guid.NewGuid();

            var def = Door.Create("door-test", fixUid: id, metaDataBuilder: m => m.CategoryPath("GrpA"))
                          .Listen(signalA)
                          .UseRelayFilter<Guid>((id, s) => id == tecthId && s.From.VGrainMetaData != null && s.From.VGrainMetaData.Value.IsDemocriteSystem == false)
                          .Build();

            Check.That(def).IsNotNull().And.IsInstanceOf<RelayFilterDoorDefinition>();
            Check.That(def.Uid).IsEqualTo(id);
            Check.That(def.MetaData).IsNotNull();
            Check.That(def.MetaData!.CategoryPath).IsNotNull().And.IsEqualTo("GrpA");
            Check.That(def.Name).IsEqualTo("door-test");
            Check.That(def.VGrainInterfaceFullName).IsNotNull().And.IsEqualTo(typeof(IRelayFilterVGrain).AssemblyQualifiedName);
            Check.That(def.SignalSourceIds).IsNotNull().And.ContainsExactly(signalA.SignalId);

            var relayDef = (RelayFilterDoorDefinition)def;

            var expression = relayDef.FilterCondition.ToExpression<Guid, SignalMessage, bool>();

            Check.That(expression).IsNotNull();

            var validSource = new SignalSource<Guid>(Guid.NewGuid(),
                                                     Guid.NewGuid(),
                                                     "sourcSignal",
                                                     false,
                                                     DateTime.Now,
                                                     null,
                                                     new Core.Abstractions.Models.VGrainMetaData("impl".AsEnumerable(), "impl", false, null, IdFormatTypeEnum.String.AsEnumerable(), false, false, false),
                                                     tecthId,
                                                     null);

            var testMsg = new SignalMessage(Guid.NewGuid(), DateTime.Now, validSource);

            var func = expression.Compile();

            Check.ThatCode(() => func(validSource.Data, testMsg)).WhichResult().IsTrue();
            Check.ThatCode(() => func(Guid.NewGuid(), testMsg)).WhichResult().IsFalse();
        }

        /// <summary>
        /// Check that option <see cref="RelayFilterDoorDefinition.DontRelaySignalContent"/> is well set.
        /// </summary>
        [Fact]
        public void Build_Filter_On_Signal_Without_Relay_Content()
        {
            var signalA = Signal.Create("signal-a");

            var id = Guid.NewGuid();
            var def = Door.Create("door-test", fixUid: id, metaDataBuilder: m => m.CategoryPath("GrpA"))
                          .Listen(signalA)
                          .UseRelayFilter(r => r.DontRelaySignalContent()
                                                .Condition(s => s.From.VGrainMetaData != null && s.From.VGrainMetaData.Value.IsDemocriteSystem == false))
                          .Build();

            Check.That(def).IsNotNull().And.IsInstanceOf<RelayFilterDoorDefinition>();
            Check.That(def.Uid).IsEqualTo(id);
            Check.That(def.MetaData).IsNotNull();
            Check.That(def.MetaData!.CategoryPath).IsNotNull().And.IsEqualTo("GrpA");
            Check.That(def.Name).IsEqualTo("door-test");
            Check.That(def.VGrainInterfaceFullName).IsNotNull().And.IsEqualTo(typeof(IRelayFilterVGrain).AssemblyQualifiedName);
            Check.That(def.SignalSourceIds).IsNotNull().And.ContainsExactly(signalA.SignalId);

            var relayDef = (RelayFilterDoorDefinition)def;

            Check.That(relayDef.DontRelaySignalContent).IsTrue();
        }

        /// <summary>
        /// Check that condition using external method is not allowed because not serializable
        /// </summary>
        [Fact]
        public void Build_Invalid_Filter_On_Signal()
        {
            var signalA = Signal.Create("signal-a");

            var id = Guid.NewGuid();
            var tecthId = Guid.NewGuid();

            try
            {
                var def = Door.Create("door-test", fixUid: id, metaDataBuilder: m => m.CategoryPath("GrpA"))
                              .Listen(signalA)
                              .UseRelayFilter(s => ExternalMethod() && s.From.VGrainMetaData != null && s.From.VGrainMetaData.Value.IsDemocriteSystem == false)
                              .Build();

                Assert.Fail("Must be allowed to call an external method contextual");
            }
            catch (NotSupportedException)
            {

            }
        }

        /// <summary>
        /// Check that condition using external method is not allowed because not serializable
        /// </summary>
        /// <remarks>
        ///     Same as <see cref="Build_Invalid_Filter_On_Signal"/> but with a content Type filter
        /// </remarks>
        [Fact]
        public void Build_Invalid_Filter_With_Context_On_Signal()
        {
            var signalA = Signal.Create("signal-a");

            var id = Guid.NewGuid();
            var tecthId = Guid.NewGuid();

            try
            {
                var def = Door.Create("door-test", fixUid: id, metaDataBuilder: m => m.CategoryPath("GrpA"))
                              .Listen(signalA)
                              .UseRelayFilter<Guid>((id, s) => id == Guid.NewGuid() && ExternalMethod() && s.From.VGrainMetaData != null && s.From.VGrainMetaData.Value.IsDemocriteSystem == false)
                              .Build();

                Assert.Fail("Must be allowed to call an external method contextual");
            }
            catch (NotSupportedException)
            {

            }
        }

        #endregion

        #region Tools

        /// <summary>
        /// Tests the relay filter conditions.
        /// </summary>
        private static void TestRelayFilterConditions<TMessageCarryType>(DoorDefinition def,
                                                            IEnumerable<TMessageCarryType> success,
                                                            IEnumerable<TMessageCarryType> fail,
                                                            Func<TMessageCarryType, Fixture, SignalMessage>? signalBuilder = null)
            where TMessageCarryType : struct
        {
            var fixture = ObjectTestHelper.PrepareFixture();

            if (signalBuilder is null)
                signalBuilder = (c, f) => DefaultSignalBuilder<TMessageCarryType>(c, f);

            var relayDef = (RelayFilterDoorDefinition)def;
            var expression = relayDef.FilterCondition.ToExpression<TMessageCarryType, SignalMessage, bool>();

            Check.That(expression).IsNotNull();

            var func = expression.Compile();
            Check.That(func).IsNotNull();

            foreach (var result in success)
            {
                var msg = signalBuilder(result, fixture);
                Check.ThatCode(() => func(result, msg)).DoesNotThrow()
                                                       .And
                                                       .WhichResult()
                                                       .IsTrue();
            }

            foreach (var result in fail)
            {
                var msg = signalBuilder(result, fixture);
                Check.ThatCode(() => func(result, msg)).DoesNotThrow()
                                                       .And
                                                       .WhichResult()
                                                       .IsFalse();
            }
        }

        private static SignalMessage DefaultSignalBuilder<TTMessageCarryType>(TTMessageCarryType data, Fixture fixture)
            where TTMessageCarryType : struct
        {
            return new SignalMessage(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     new SignalSource<TTMessageCarryType>(fixture.Create<Guid>(),
                                                                          fixture.Create<Guid>(),
                                                                          fixture.Create<string>(),
                                                                          false,
                                                                          fixture.Create<DateTime>(),
                                                                          fixture.Create<GrainId>(),
                                                                          fixture.Create<VGrainMetaData>(),
                                                                          data));
        }

        private bool ExternalMethod()
        {
            return false;
        }

        /// <inheritdoc />
        protected override DoorDefinition BuildSimpleDefinition(IDoorWithListenerBuilder rootBuilder)
        {
            return rootBuilder.UseRelayFilter(s => s.From != null)
                              .Build();
        }

        #endregion
    }
}
