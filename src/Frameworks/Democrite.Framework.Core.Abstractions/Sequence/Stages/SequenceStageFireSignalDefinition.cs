// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class SequenceStageFireSignalDefinition : SequenceStageBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageFireSignalDefinition"/> class.
        /// </summary>
        public SequenceStageFireSignalDefinition(AbstractType? input,
                                                 string? signalName,
                                                 Guid? signalId,
                                                 bool multi,
                                                 AccessExpressionDefinition? messageAccess,
                                                 Guid? uid = null)
            : base(StageTypeEnum.FireSignal, input, input, null, false, uid)
        {
            this.SignalName = signalName;
            this.SignalId = signalId;
            this.MessageAccess = messageAccess;
            this.Multi = multi;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal message.
        /// </summary>
        [DataMember]
        public string? SignalName { get; }

        /// <summary>
        /// Gets the signal identifier.
        /// </summary>
        [DataMember] 
        public Guid? SignalId { get; }

        /// <summary>
        /// Gets the message access.
        /// </summary>
        [DataMember] 
        public AccessExpressionDefinition? MessageAccess { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SequenceStageFireSignalDefinition"/> must send multiple message.
        /// </summary>
        [DataMember] 
        public bool Multi { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(ISequenceStageDefinition other)
        {
            return other is SequenceStageFireSignalDefinition otherFire &&
                   otherFire.SignalId == this.SignalId &&
                   otherFire.SignalName == this.SignalName &&
                   otherFire.MessageAccess == this.MessageAccess &&
                   otherFire.Multi == this.Multi;
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return HashCode.Combine(this.SignalName,
                                    this.SignalId,
                                    this.MessageAccess,
                                    this.Multi);
        }

        #endregion
    }
}
