// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.UnitTests.Door
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Node.Signals.Doors;
    using Democrite.UnitTests.ToolKit.Extensions;
    using Democrite.UnitTests.ToolKit.Tests;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using Microsoft.Extensions.DependencyInjection;

    using NFluent;

    using NSubstitute;
    using NSubstitute.ReceivedExtensions;

    using Orleans.Runtime;

    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Base Unit test for any door
    /// </summary>
    /// <typeparam name="TDoorGrain">The type of the door grain.</typeparam>
    public class DoorVGrainBaseUTest<TDoorGrain, TDoorDefinition, TDoorGrainInterface> : VGrainBaseUTest<TDoorGrain>
        where TDoorGrain : class, IDoorVGrain, IGrainBase, TDoorGrainInterface
        where TDoorGrainInterface : IDoorVGrain
        where TDoorDefinition : DoorDefinition
    {
        #region Fields

        public const int SUCESS_MSG = 42;

        private readonly Func<Fixture, string, TDoorDefinition>? _buildDefinition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorVGrainBaseUTest{TDoorGrain, TDoorDefinition}"/> class.
        /// </summary>
        public DoorVGrainBaseUTest(Func<Fixture, string, TDoorDefinition>? buildDefinition = null)
        {
            this._buildDefinition = buildDefinition;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Test the door subscription to all describe signal
        /// </summary>
        public async Task Door_Subscribe()
        {
            var fixture = new Fixture();

            var signals = fixture.CreateMany<SignalId>(2).ToArray();
            var doors = fixture.CreateMany<DoorId>(5).ToArray();

            var serviceCollection = new ServiceCollection();
            var door = CreateTestDoorGrain(serviceCollection,
                                           fix =>
                                           {
                                               // Register data sample
                                               fix.Inject<IEnumerable<SignalId>>(signals);
                                               fix.Inject<IEnumerable<DoorId>>(doors);
                                           },
                                           fixture);

            // Register mock service
            var provider = serviceCollection.BuildServiceProvider();
            var mockSignalService = provider.GetRequiredService<ISignalService>();

            mockSignalService.SubscribeAsync(Arg.Is<SignalId>(s => signals.Contains(s)), Arg.Any<ISignalReceiver>(), Arg.Any<CancellationToken>())
                             .Returns(Task.FromResult(new SubscriptionId(Guid.NewGuid(), true, Guid.NewGuid())));

            mockSignalService.SubscribeAsync(Arg.Is<DoorId>(s => doors.Contains(s)), Arg.Any<ISignalReceiver>(), Arg.Any<CancellationToken>())
                             .Returns(Task.FromResult(new SubscriptionId(Guid.NewGuid(), true, Guid.NewGuid())));

            await fixture.InitVGrain(door);

            await mockSignalService.Received(signals.Length)
                                   .SubscribeAsync(Arg.Any<SignalId>(), Arg.Any<ISignalReceiver>(), Arg.Any<CancellationToken>());

            await mockSignalService.Received(doors.Length)
                                   .SubscribeAsync(Arg.Any<DoorId>(), Arg.Is(door), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Test if door correctly managed signal reception
        /// </summary>
        public async Task Door_Signal_Received()
        {
            var fixture = new Fixture();

            var signal = fixture.Create<SignalId>();

            var serviceCollection = new ServiceCollection();
            var door = CreateTestDoorGrain(serviceCollection,
                                           fix =>
                                           {
                                               fix.Inject<IEnumerable<SignalId>>(signal.AsEnumerable());
                                               fix.Inject<IEnumerable<DoorId>>(EnumerableHelper<DoorId>.ReadOnly);
                                           },
                                           fixture: fixture);

            // Register mock service
            var provider = serviceCollection.BuildServiceProvider();
            var mockSignalService = provider.GetRequiredService<ISignalService>();
            var mockOutputSignal = provider.GetRequiredService<IDoorSignalVGrain>();
            var def = provider.GetRequiredService<TDoorDefinition>();
            var doorIdentityCard = provider.GetRequiredService<IComponentDoorIdentityCard>();

            mockSignalService.SubscribeAsync(signal, Arg.Any<ISignalReceiver>(), Arg.Any<CancellationToken>())
                             .Returns(Task.FromResult(new SubscriptionId(Guid.NewGuid(), true, Guid.NewGuid())));

            // Disabled door logic to apply to test multiple signal receiving without interruption
            doorIdentityCard.CanBeStimuate().Returns(false);

            await fixture.InitVGrain(door);

            await mockSignalService.Received(1)
                                   .SubscribeAsync(Arg.Any<SignalId>(), Arg.Any<ISignalReceiver>(), Arg.Any<CancellationToken>());

            // StartAt signal testing
            var state = GetDoorState(door);

            var signalStatusCollection = state?.SignalStatus.Where(s => s.SignalId != def.Uid).ToArray();

            Check.That(signalStatusCollection).IsNotNull().And.CountIs(1);

            var signalState = signalStatusCollection!.First();

            // Test before calling
            Check.That(signalState.LastSignalReceived).IsNull();
            Check.That(signalState.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(0);
            Check.That(signalState.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            // Send first signal
            var signalId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var utcNowRef = utcNow;
            var simulateSendSignal = new SignalMessage(signalId,
                                                       utcNow,
                                                       new SignalSource(signalId, signal.Uid, signal.Name, false, utcNow, null, null, null));

            await door.ReceiveSignalAsync(simulateSendSignal);

            Check.That(signalState.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(1);
            Check.That(signalState.LastSignalReceived).IsNotNull().And.IsEqualTo(simulateSendSignal);
            Check.That(signalState.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            // Send second signal
            signalId = Guid.NewGuid();
            utcNow = utcNowRef.AddSeconds(-10);

            var simulateSecondSendSignal = new SignalMessage(signalId,
                                                            utcNow,
                                                            new SignalSource(signalId, signal.Uid, signal.Name, false, utcNow, null, null, null));

            await door.ReceiveSignalAsync(simulateSecondSendSignal);

            Check.That(signalState.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(2).And.ContainsExactly(simulateSecondSendSignal, simulateSendSignal);

            // The new signal received simulate a early send then it should not replace the "LastSignalReceived"
            Check.That(signalState.LastSignalReceived).IsNotNull().And.IsEqualTo(simulateSendSignal); 
            Check.That(signalState.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            // Send third signal older than second one
            signalId = Guid.NewGuid();
            utcNow = utcNowRef.AddSeconds(-5);

            var simulateThirdSendSignal = new SignalMessage(signalId,
                                                            utcNow,
                                                            new SignalSource(signalId, signal.Uid, signal.Name, false, utcNow, null, null, null));

            await door.ReceiveSignalAsync(simulateThirdSendSignal);

            Check.That(signalState.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(3).And.ContainsExactly(simulateSecondSendSignal, simulateThirdSendSignal, simulateSendSignal);
            Check.That(signalState.LastSignalReceived).IsNotNull().And.IsEqualTo(simulateSendSignal);
            Check.That(signalState.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            // Send 4th signal new than second one
            signalId = Guid.NewGuid();
            utcNow = utcNowRef.AddSeconds(5);

            var simulateForthSendSignal = new SignalMessage(signalId,
                                                            utcNow,
                                                            new SignalSource(signalId, signal.Uid, signal.Name, false, utcNow, null, null, null));

            await door.ReceiveSignalAsync(simulateForthSendSignal);

            Check.That(signalState.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(4).And.ContainsExactly(simulateSecondSendSignal, simulateThirdSendSignal, simulateSendSignal, simulateForthSendSignal);
            Check.That(signalState.LastSignalReceived).IsNotNull().And.IsEqualTo(simulateForthSendSignal);
            Check.That(signalState.SignalsReceivedHistory).IsNotNull().And.CountIs(0);
        }

        /// <summary>
        /// Test if door correctly managed signal reception not listen
        /// </summary>
        public async Task Door_Signal_Received_Not_Listen()
        {
            var fixture = new Fixture();

            var signal = fixture.Create<SignalId>();

            var serviceCollection = new ServiceCollection();
            var door = CreateTestDoorGrain(serviceCollection,
                                            fix =>
                                            {
                                                fix.Inject<IEnumerable<SignalId>>(signal.AsEnumerable());
                                                fix.Inject<IEnumerable<DoorId>>(EnumerableHelper<DoorId>.ReadOnly);
                                            },
                                            fixture: fixture);

            // Register mock service
            var provider = serviceCollection.BuildServiceProvider();
            var mockSignalService = provider.GetRequiredService<ISignalService>();
            var mockOutputSignal = provider.GetRequiredService<IDoorSignalVGrain>();
            var def = provider.GetRequiredService<TDoorDefinition>();

            mockSignalService.SubscribeAsync(signal, Arg.Any<ISignalReceiver>(), Arg.Any<CancellationToken>())
                             .Returns(Task.FromResult(new SubscriptionId(Guid.NewGuid(), true, Guid.NewGuid())));

            await fixture.InitVGrain(door);

            await mockSignalService.Received(1)
                                   .SubscribeAsync(Arg.Any<SignalId>(), Arg.Any<ISignalReceiver>(), Arg.Any<CancellationToken>());

            // StartAt signal testing
            var state = GetDoorState(door);

            var signalStatusCollection = state?.SignalStatus.Where(s => s.SignalId != def.Uid).ToArray();

            Check.That(signalStatusCollection).IsNotNull().And.CountIs(1);

            var signalState = signalStatusCollection!.First();

            // Test before calling
            Check.That(signalState.LastSignalReceived).IsNull();
            Check.That(signalState.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(0);
            Check.That(signalState.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            var signalId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var invalidSignal = new SignalId(Guid.NewGuid(), "INVALID");

            var invalidSimulateSendSignal = new SignalMessage(signalId,
                                                              utcNow,
                                                              new SignalSource(signalId, invalidSignal.Uid, invalidSignal.Name, false, utcNow, null, null, null));

            // Send signal that doesn't pass
            await door.ReceiveSignalAsync(invalidSimulateSendSignal);

            Check.That(signalState.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(0);
            Check.That(signalState.LastSignalReceived).IsNotNull().And.IsEqualTo(null);
            Check.That(signalState.SignalsReceivedHistory).IsNotNull().And.CountIs(0);
        }

        #region Tools

        /// <summary>
        /// Gets the state of the door.
        /// </summary>
        protected virtual DoorHandlerState GetDoorState(TDoorGrain door)
        {
            return VGrainBaseUTest<TDoorGrain>.GetGrainState<DoorHandlerState>(door) ?? throw new InvalidCastException("State not founded");
        }

        /// <summary>
        /// Creates the door definition.
        /// </summary>
        protected virtual TDoorDefinition CreateDoorDefinition(Fixture fixture, string methodCalling)
        {
            if (this._buildDefinition != null)
                return this._buildDefinition.Invoke(fixture, methodCalling);
            return fixture.Create<TDoorDefinition>();
        }

        /// <summary>
        /// Creates the test door grain and setup mock services in <paramref name="mockServiceCollection"/>
        /// </summary>
        private TDoorGrain CreateTestDoorGrain(IServiceCollection mockServiceCollection,
                                               Action<Fixture>? prepareDefCreation = null,
                                               Fixture? fixture = null,
                                               [CallerMemberName] string? methodCalling = null)
        {
            fixture = ObjectTestHelper.PrepareFixture(fixture: fixture);

            var mockSignalService = Substitute.For<ISignalService>();
            var mockDefinitionProvider = Substitute.For<IDoorDefinitionProvider>();
            var mockFactoryProvider = Substitute.For<IGrainFactory>();
            var mockTimeManager = Substitute.For<ITimeManager>();
            var mockComponentIdentityCardProvider = Substitute.For<IComponentIdentitCardProviderClient>();
            var doorIdentityCard = Substitute.For<IComponentDoorIdentityCard>();

            mockComponentIdentityCardProvider.GetComponentIdentityCardAsync(Arg.Any<GrainId>(), Arg.Any<Guid>())
                                             .ReturnsForAnyArgs(doorIdentityCard);

            // Enable the component
            doorIdentityCard.IsEnable().Returns(true);

            var mockOutsignalGrain = Substitute.For<IDoorSignalVGrain>();
            mockOutsignalGrain!.Fire(Arg.Any<SignalMessage>())
                               .Returns(Guid.NewGuid());

            mockServiceCollection.AddSingleton(mockSignalService)
                                 .AddSingleton(mockDefinitionProvider)
                                 .AddSingleton(mockFactoryProvider)
                                 .AddSingleton(mockOutsignalGrain)
                                 .AddSingleton(mockTimeManager)
                                 .AddSingleton<IFixture>(fixture)
                                 .AddSingleton(mockComponentIdentityCardProvider)
                                 .AddSingleton(doorIdentityCard);

            // Register mock service
            fixture.Inject(mockSignalService);
            fixture.Inject(mockDefinitionProvider);
            fixture.Inject(mockFactoryProvider);
            fixture.Inject(mockTimeManager);

            mockTimeManager.UtcNow.Returns(DateTime.UtcNow);
            mockTimeManager.Now.Returns(DateTime.Now);
            mockTimeManager.LocalNow.Returns(DateTime.Now);

            // create door definition
            prepareDefCreation?.Invoke(fixture);

            var def = CreateDoorDefinition(fixture, methodCalling!);
            mockServiceCollection.AddSingleton<DoorDefinition>(def)
                                 .AddSingleton(def);

            mockDefinitionProvider.GetByKeyAsync(Arg.Is(def.Uid), Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult<DoorDefinition?>(def));

            var forceGrainId = GrainId.Create(GrainType.Create(typeof(TDoorGrain).Name), GrainIdKeyExtensions.CreateGuidKey(def.Uid));

            mockFactoryProvider.GetGrain<IDoorSignalVGrain>(Arg.Is(def.Uid)).Returns(mockOutsignalGrain);

            var door = fixture.CreateVGrain<TDoorGrain>(forcedGrainId: forceGrainId,
                                                        setupServiceCollection: s =>
                                                        {
                                                            foreach (var service in mockServiceCollection)
                                                                s.Add(service);
                                                        });

            return door;
        }

        #endregion

        #endregion
    }
}
