// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Information about target subscription of a <see cref="Signal"/> or <see cref="DoordSignal"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal sealed class SignalSubscription
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSubscription"/> class.
        /// </summary>
        public SignalSubscription(Guid uid,
                                  DedicatedGrainId<ISignalReceiver>? targetGrainId,
                                  DedicatedGrainId<ISignalReceiverReadOnly>? targetReadOnlyGrainId)
        {
            this.Uid = uid;
            this.TargetGrainId = targetGrainId;
            this.TargetReadOnlyGrainId = targetReadOnlyGrainId;
            this.IsTargetReadOnly = targetReadOnlyGrainId is not null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the subscription uid.
        /// </summary>
        [Id(0)]
        public Guid Uid { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is target read only.
        /// </summary>
        public bool IsTargetReadOnly { get; }

        /// <summary>
        /// Gets the target grain identifier.
        /// </summary>
        [Id(1)]
        public DedicatedGrainId<ISignalReceiver>? TargetGrainId { get; }

        /// <summary>
        /// Gets the readonly target grain identifier.
        /// </summary>
        [Id(2)]
        public DedicatedGrainId<ISignalReceiverReadOnly>? TargetReadOnlyGrainId { get; }

        #endregion
    }
}
