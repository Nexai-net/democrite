// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Models
{
    using Orleans.Runtime;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Information about target subscription of a <see cref="Signal"/> or <see cref="DoordSignal"/>
    /// </summary>
    [Serializable]
    [Immutable]
    [ImmutableObject(true)]
    [GenerateSerializer]
    internal sealed class SignalSubscription
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSubscription"/> class.
        /// </summary>
        public SignalSubscription(Guid uid, GrainId targetGrainId)
        {
            this.Uid = uid;
            this.TargetGrainId = targetGrainId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the subscription uid.
        /// </summary>
        public Guid Uid { get; }

        /// <summary>
        /// Gets the target grain identifier.
        /// </summary>
        public GrainId TargetGrainId { get; }

        #endregion
    }
}
