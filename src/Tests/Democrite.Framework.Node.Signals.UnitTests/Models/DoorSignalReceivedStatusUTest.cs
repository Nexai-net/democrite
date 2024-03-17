// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.UnitTests.Models
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Signals.Models;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using Newtonsoft.Json;

    using NFluent;

    using System;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Test for <see cref="DoorSignalReceivedStatus"/>
    /// </summary>
    public sealed class DoorSignalReceivedStatusUTest
    {
        #region Fields
        
        private const int NB_TEST_SIGNALS = 12;
        
        #endregion

        #region Methods

        /// <summary>
        /// Adds many <see cref="SignalMessage"/> and ensure order and property are well set
        /// </summary>
        [Fact]
        public void Add_Many_SignalEvent()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true, supportMutableValueType: true);

            var signalId = Guid.NewGuid();

            var receivedStatus = new DoorSignalReceivedStatus(signalId);

            fixture.Inject<SignalSource?>(null);

            var incommingSignals = fixture.CreateMany<SignalMessage>(NB_TEST_SIGNALS)
                                          .ToArray();

            var orderedSignals = incommingSignals.OrderBy(i => i.SendUtcTime)
                                                 .ToArray();

            Check.That(incommingSignals).Not.ContainsExactly(orderedSignals);

            foreach (var signal in incommingSignals)
                receivedStatus.Push(signal);

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(NB_TEST_SIGNALS);
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            var copyOfReceived = receivedStatus.SignalsReceivedNotConsumed.ToArray();
            Check.That(orderedSignals).ContainsExactly(copyOfReceived);

            Check.That(receivedStatus.LastSignalReceived).IsNotNull().And.IsEqualTo(orderedSignals.Last());
        }

        /// <summary>
        /// Test to serialize <see cref="DoorSignalReceivedStatus"/> using its surrogate <see cref="DoorSignalReceivedStatusSurrogate"/> and restore it.
        /// </summary>
        [Fact]
        public void Serialized_And_Resort()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true, supportMutableValueType: true);

            var signalId = Guid.NewGuid();

            var receivedStatus = new DoorSignalReceivedStatus(signalId);

            fixture.Inject<SignalSource?>(null);

            var incommingSignals = fixture.CreateMany<SignalMessage>(NB_TEST_SIGNALS)
                                          .ToArray();

            foreach (var signal in incommingSignals)
                receivedStatus.Push(signal);

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(NB_TEST_SIGNALS);
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            var signalsToRemoved = receivedStatus.SignalsReceivedNotConsumed
                                                 .Select((s, indx) => (s, indx))
                                                 .Where(kv => kv.indx % 2 == 0)
                                                 .Select(kv => kv.s)
                                                 .ToArray();

            foreach (var toRemoved in signalsToRemoved)
                receivedStatus.MarkAsUsed(toRemoved, true);

            var surrogate = DoorSignalReceivedStatusSurrogate.CreateFrom(receivedStatus);

            var str = JsonConvert.SerializeObject(surrogate, (Debugger.IsAttached ? Formatting.Indented : Formatting.None));

            Check.That(str).IsNotNull().And.Not.IsNullOrEmpty();

            var result = JsonConvert.DeserializeObject<DoorSignalReceivedStatusSurrogate>(str);

            var newReceivedStatus = new DoorSignalReceivedStatus(result);

            Check.That(newReceivedStatus).IsEqualTo(receivedStatus);
        }

        /// <summary>
        /// Add signals and mark some as used
        /// </summary>
        [Fact]
        public void Mark_As_Used()
        {
            MarkAsUsedBasedOnRetention(true);
        }

        /// <summary>
        /// Add signals and mark some as used
        /// </summary>
        [Fact]
        public void Mark_As_Used_Without_History()
        {
            MarkAsUsedBasedOnRetention(false);
        }

        /// <summary>
        /// Clears the history based on quantity.
        /// </summary>
        [Fact]
        public void Clear_History_Based_OnQuantity()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true, supportMutableValueType: true);

            var signalId = Guid.NewGuid();

            var receivedStatus = new DoorSignalReceivedStatus(signalId);

            fixture.Inject<SignalSource?>(null);

            var incommingSignals = fixture.CreateMany<SignalMessage>(NB_TEST_SIGNALS)
                                          .ToArray();

            var orderedSignals = incommingSignals.OrderBy(i => i.SendUtcTime)
                                                 .ToArray();

            Check.That(incommingSignals).Not.ContainsExactly(orderedSignals);

            foreach (var signal in incommingSignals)
            {
                receivedStatus.Push(signal);
                receivedStatus.MarkAsUsed(signal, true);
            }

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.IsEmpty();
            Check.That(receivedStatus.LastSignalReceived).IsNotNull();
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.CountIs(incommingSignals.Length);

            var retains = (int)(NB_TEST_SIGNALS / 3);
            receivedStatus.ClearHistory((uint)retains);

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.IsEmpty();
            Check.That(receivedStatus.LastSignalReceived).IsNotNull();

            // use interserct to keep the received order
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.CountIs(retains).And.ContainsExactly(incommingSignals.Intersect(orderedSignals.Take(retains)));
        }

        /// <summary>
        /// Clears the "not consumed" based on quantity.
        /// </summary>
        [Fact]
        public void Clear_NotConsumed_Based_OnQuantity()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true, supportMutableValueType: true);

            var signalId = Guid.NewGuid();

            var receivedStatus = new DoorSignalReceivedStatus(signalId);

            fixture.Inject<SignalSource?>(null);

            var incommingSignals = fixture.CreateMany<SignalMessage>(NB_TEST_SIGNALS)
                                          .ToArray();

            var orderedSignals = incommingSignals.OrderBy(i => i.SendUtcTime)
                                                 .ToArray();

            Check.That(incommingSignals).Not.ContainsExactly(orderedSignals);

            foreach (var signal in incommingSignals)
                receivedStatus.Push(signal);

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(incommingSignals.Length);
            Check.That(receivedStatus.LastSignalReceived).IsNotNull();
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.IsEmpty();

            var retains = (int)(NB_TEST_SIGNALS / 3);
            receivedStatus.ClearNotConsumed((uint)retains);

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.Contains(orderedSignals.TakeLast(retains));
            Check.That(receivedStatus.LastSignalReceived).IsNotNull();
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.IsEmpty();
        }

        /// <summary>
        /// Clear up history and not consumed received signals based on max retention period
        /// </summary>
        [Fact]
        public void Retention_Period_Clear_History_And_NotConsumed()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true, supportMutableValueType: true);

            var signalId = Guid.NewGuid();

            var receivedStatus = new DoorSignalReceivedStatus(signalId);

            fixture.Inject<SignalSource?>(null);

            var incommingSignals = fixture.CreateMany<SignalMessage>(NB_TEST_SIGNALS)
                                          .ToArray();

            var orderedSignals = incommingSignals.OrderBy(i => i.SendUtcTime)
                                                 .ToArray();

            Check.That(incommingSignals).Not.ContainsExactly(orderedSignals);

            foreach (var signal in incommingSignals)
                receivedStatus.Push(signal);

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(NB_TEST_SIGNALS);
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            var signalsToRemoved = receivedStatus.SignalsReceivedNotConsumed
                                                 .Select((s, indx) => (s, indx))
                                                 .Where(kv => kv.indx % 2 == 0)
                                                 .Select(kv => kv.s)
                                                 .ToArray();

            foreach (var toRemoved in signalsToRemoved)
                receivedStatus.MarkAsUsed(toRemoved, true);

            var mid = NB_TEST_SIGNALS / 2;
            var midIndx = mid - 1;
            var mediateDate = orderedSignals[midIndx].SendUtcTime + ((orderedSignals[midIndx + 1].SendUtcTime - orderedSignals[midIndx].SendUtcTime) / 2.0);

            receivedStatus.ClearHistoryAndNotConsumed(mediateDate);

            var remains = receivedStatus.SignalsReceivedNotConsumed
                                        .Concat(receivedStatus.SignalsReceivedHistory)
                                        .Distinct()
                                        .ToArray();

            Check.That(remains).CountIs(mid);
        }

        #region Tools

        /// <summary>
        /// Add signals and mark some as used
        /// </summary>
        private void MarkAsUsedBasedOnRetention(bool retantionHistory)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true, supportMutableValueType: true);

            var signalId = Guid.NewGuid();

            var receivedStatus = new DoorSignalReceivedStatus(signalId);

            fixture.Inject<SignalSource?>(null);

            var incommingSignals = fixture.CreateMany<SignalMessage>(NB_TEST_SIGNALS)
                                          .ToArray();

            var orderedSignals = incommingSignals.OrderBy(i => i.SendUtcTime)
                                                 .ToArray();

            Check.That(incommingSignals).Not.ContainsExactly(orderedSignals);

            foreach (var signal in incommingSignals)
                receivedStatus.Push(signal);

            Check.That(receivedStatus.SignalsReceivedNotConsumed).IsNotNull().And.CountIs(NB_TEST_SIGNALS);
            Check.That(receivedStatus.SignalsReceivedHistory).IsNotNull().And.CountIs(0);

            var signalsToRemoved = receivedStatus.SignalsReceivedNotConsumed
                                                 .Select((s, indx) => (s, indx))
                                                 .Where(kv => kv.indx % 2 == 0)
                                                 .Select(kv => kv.s)
                                                 .ToArray();

            foreach (var toRemoved in signalsToRemoved)
                receivedStatus.MarkAsUsed(toRemoved, retantionHistory);

            var remainsNotConsumed = receivedStatus.SignalsReceivedNotConsumed.ToArray();
            Check.That(remainsNotConsumed).IsNotNull()
                                          .And
                                          .CountIs(NB_TEST_SIGNALS - signalsToRemoved.Length)
                                          .And
                                          .ContainsExactly(orderedSignals.Except(signalsToRemoved).ToArray());

            var history = receivedStatus.SignalsReceivedHistory.ToArray();

            if (retantionHistory)
            {
                Check.That(history).IsNotNull()
                                   .And
                                   .IsEquivalentTo(signalsToRemoved);
            }
            else
            {
                Check.That(history).IsNotNull().And.IsEmpty();
            }
        }

        #endregion

        #endregion
    }
}
