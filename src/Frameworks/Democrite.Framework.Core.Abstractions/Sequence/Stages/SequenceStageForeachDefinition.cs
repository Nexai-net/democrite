// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stage related to VGrain Foreach information
    /// </summary>
    /// <seealso cref="ISequenceStageDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceStageForeachDefinition : SequenceStageDefinition, IFlowHostStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageForeachDefinition"/> class.
        /// </summary>
        public SequenceStageForeachDefinition(Guid uid,
                                              string displayName,
                                              AbstractType? input,
                                              SequenceDefinition innerFlow,
                                              AbstractType? output,
                                              AbstractType? outputForeach,
                                              AccessExpressionDefinition? memberAccess,
                                              AbstractMethod? setMethod,
                                              DefinitionMetaData? metaData,
                                              bool preventReturn = false)
            : base(uid, displayName, StageTypeEnum.Foreach, input, output, metaData, preventReturn)
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
        [Id(0)]
        public SequenceDefinition InnerFlow { get; }

        /// <summary>
        /// Gets the member access.
        /// </summary>
        [DataMember]
        [Id(1)]
        public AccessExpressionDefinition? MemberAccess { get; }

        /// <summary>
        /// Gets the set method.
        /// </summary>
        [DataMember]
        [Id(2)]
        public AbstractMethod? SetMethod { get; }

        /// <summary>
        /// Gets the output foreach.
        /// </summary>
        [DataMember]
        [Id(3)]
        public AbstractType? OutputForeach { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(SequenceStageDefinition other)
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
