// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Abstractions.Expressions;
    using Democrite.Framework.Toolbox.Abstractions.Models;
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
                                              AbstractType? outputForeach,
                                              AccessExpressionDefinition? memberAccess,
                                              AbstractMethod? setMethod,
                                              SequenceOptionStageDefinition? options = null,
                                              bool preventReturn = false,
                                              Guid? uid = null)
            : base(StageTypeEnum.Foreach, input, output, options, preventReturn, uid)
        {
            ArgumentNullException.ThrowIfNull(innerFlow);

            this.InnerFlow = innerFlow;
            this.MemberAccess = memberAccess;
            this.SetMethod = setMethod;
            this.OutputForeach = outputForeach;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the flow to apply on each element
        /// </summary>
        [DataMember(IsRequired = true)]
        public SequenceDefinition InnerFlow { get; }

        /// <summary>
        /// Gets the member access.
        /// </summary>
        [DataMember]
        public AccessExpressionDefinition? MemberAccess { get; }

        /// <summary>
        /// Gets the set method.
        /// </summary>
        [DataMember]
        public AbstractMethod? SetMethod { get; }

        /// <summary>
        /// Gets the output foreach.
        /// </summary>
        [DataMember]
        public AbstractType? OutputForeach { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(ISequenceStageDefinition other)
        {
            return other is SequenceStageForeachDefinition otherStepDef &&
                   otherStepDef.InnerFlow == this.InnerFlow &&
                   otherStepDef.MemberAccess == this.MemberAccess &&
                   otherStepDef.SetMethod == this.SetMethod &&
                   otherStepDef.OutputForeach == this.OutputForeach;
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return HashCode.Combine(this.InnerFlow,
                                    this.MemberAccess,
                                    this.SetMethod, 
                                    this.OutputForeach);
        }

        #endregion

    }
}
