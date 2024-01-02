// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stage related to VGrain Foreach information
    /// </summary>
    /// <seealso cref="ISequenceStageDefinition" />
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class SequenceStageForeachDefinition : SequenceStageBaseDefinition, IFlowHostStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageForeachDefinition"/> class.
        /// </summary>
        public SequenceStageForeachDefinition(AbstractType? input,
                                              SequenceDefinition innerFlow,
                                              AbstractType? output,
                                              SequenceOptionStageDefinition? options = null,
                                              bool preventReturn = false,
                                              Guid? uid = null)
            : base(StageTypeEnum.Foreach, input, output, options, preventReturn, uid)
        {
            ArgumentNullException.ThrowIfNull(innerFlow);

            this.InnerFlow = innerFlow;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the flow to apply on each element
        /// </summary>
        [DataMember(IsRequired = true)]
        public SequenceDefinition InnerFlow { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(ISequenceStageDefinition other)
        {
            return other is SequenceStageForeachDefinition otherStepDef &&
                   otherStepDef.InnerFlow == this.InnerFlow;
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return this.InnerFlow.GetHashCode();
        }

        #endregion

    }
}
