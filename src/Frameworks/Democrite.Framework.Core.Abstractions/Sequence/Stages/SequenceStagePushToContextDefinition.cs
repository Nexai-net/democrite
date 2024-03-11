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

    /// <summary>
    /// Stage used to push a meta data in the <see cref="IExecutionContext"/>
    /// </summary>
    /// <seealso cref="SequenceStageBaseDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class SequenceStagePushToContextDefinition : SequenceStageBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStagePushToContextDefinition"/> class.
        /// </summary>
        public SequenceStagePushToContextDefinition(AbstractType? input,
                                                    AccessExpressionDefinition? accessExpression,
                                                    bool @override,
                                                    SequenceOptionStageDefinition? options = null,
                                                    Guid? uid = null) 
            : base(StageTypeEnum.PushToContext, input, input, options, false, uid)
        {
            this.AccessExpression = accessExpression;
            this.Override = @override;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the access data to push
        /// </summary>
        [DataMember]
        public AccessExpressionDefinition? AccessExpression { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SequenceStagePushToContextDefinition"/> is override.
        /// </summary>
        [DataMember]
        public bool Override { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(ISequenceStageDefinition other)
        {
            return other is SequenceStagePushToContextDefinition otherPush &&
                   this.Override == otherPush.Override &&
                   (this.AccessExpression?.Equals(otherPush.AccessExpression) ?? otherPush.AccessExpression is null);
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return HashCode.Combine(this.AccessExpression, this.Override);
        }

        #endregion
    }
}
