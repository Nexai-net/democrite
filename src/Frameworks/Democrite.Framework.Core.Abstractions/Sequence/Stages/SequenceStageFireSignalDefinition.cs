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
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceStageFireSignalDefinition : SequenceStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageFireSignalDefinition"/> class.
        /// </summary>
        public SequenceStageFireSignalDefinition(Guid uid,
                                                 string displayName,
                                                 AbstractType? input,
                                                 AccessExpressionDefinition signalInfo,
                                                 bool multi,
                                                 AccessExpressionDefinition? messageAccess,
                                                 DefinitionMetaData? definitionMeta)
            : base(uid, displayName, StageTypeEnum.FireSignal, input, input, definitionMeta, false)
        {
            this.SignalInfo = signalInfo;
            this.MessageAccess = messageAccess;
            this.Multi = multi;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal message.
        /// </summary>
        [DataMember]
        [Id(0)]
        public AccessExpressionDefinition SignalInfo { get; }

        /// <summary>
        /// Gets the message access.
        /// </summary>
        [DataMember]
        [Id(1)]
        public AccessExpressionDefinition? MessageAccess { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SequenceStageFireSignalDefinition"/> must send multiple message.
        /// </summary>
        [DataMember]
        [Id(2)]
        public bool Multi { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(SequenceStageDefinition other)
        {
            return other is SequenceStageFireSignalDefinition otherFire &&
                   otherFire.SignalInfo == this.SignalInfo &&
                   otherFire.MessageAccess == this.MessageAccess &&
                   otherFire.Multi == this.Multi;
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return HashCode.Combine(this.SignalInfo,
                                    this.MessageAccess,
                                    this.Multi);
        }

        #endregion
    }
}
