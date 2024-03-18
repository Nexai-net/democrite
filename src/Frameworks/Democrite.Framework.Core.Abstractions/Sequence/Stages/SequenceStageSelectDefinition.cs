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
    public sealed class SequenceStageSelectDefinition : SequenceStageBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageFireSignalDefinition"/> class.
        /// </summary>
        public SequenceStageSelectDefinition(AbstractType? input,
                                             AbstractType? output,
                                             AccessExpressionDefinition selectAccess,
                                             Guid? uid = null)
            : base(StageTypeEnum.Select, input, output, null, false, uid)
        {
            ArgumentNullException.ThrowIfNull(selectAccess);

            this.SelectAccess = selectAccess;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the message access.
        /// </summary>
        [DataMember]
        public AccessExpressionDefinition SelectAccess { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(ISequenceStageDefinition other)
        {
            return other is SequenceStageSelectDefinition otherSelect &&
                   otherSelect.SelectAccess == this.SelectAccess;
            ;
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return HashCode.Combine(this.SelectAccess);
        }

        #endregion
    }
}
