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
    public sealed class SequenceStageSelectDefinition : SequenceStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageFireSignalDefinition"/> class.
        /// </summary>
        public SequenceStageSelectDefinition(Guid uid,
                                             string displayName,
                                             AbstractType? input,
                                             AbstractType? output,
                                             AccessExpressionDefinition selectAccess,
                                             DefinitionMetaData? metaData)
            : base(uid, displayName, StageTypeEnum.Select, input, output, metaData, false)
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
        [Id(0)]
        public AccessExpressionDefinition SelectAccess { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(SequenceStageDefinition other)
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
