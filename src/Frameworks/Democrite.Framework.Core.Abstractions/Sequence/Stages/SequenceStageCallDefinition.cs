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
    /// Stage related to VGrain call information
    /// </summary>
    /// <seealso cref="ISequenceStageDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class SequenceStageCallDefinition : SequenceStageBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageCallDefinition"/> class.
        /// </summary>
        public SequenceStageCallDefinition(AbstractType? input,
                                           ConcretType vgrainType,
                                           AbstractMethod callMethodDefinition,
                                           AbstractType? output,
                                           AccessExpressionDefinition? configuration,
                                           SequenceOptionStageDefinition? options = null,
                                           bool preventReturn = false,
                                           Guid? uid = null)
            : base(StageTypeEnum.Call, input, output, options, preventReturn, uid)
        {
            ArgumentNullException.ThrowIfNull(callMethodDefinition);
            ArgumentNullException.ThrowIfNull(vgrainType);

            this.CallMethodDefinition = callMethodDefinition;

            this.VGrainType = vgrainType;

            this.Configuration = configuration;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the method information.
        /// </summary>
        [DataMember]
        public AbstractMethod CallMethodDefinition { get; }

        /// <summary>
        /// Gets the type of the vgrain.
        /// </summary>
        [DataMember]
        public ConcretType VGrainType { get; }

        /// <summary>
        /// Gets the context information.
        /// </summary>
        [DataMember]
        public AccessExpressionDefinition? Configuration { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override bool OnStageEquals(ISequenceStageDefinition other)
        {
            return other is SequenceStageCallDefinition callOther &&
                   callOther.CallMethodDefinition == this.CallMethodDefinition &&
                   callOther.VGrainType.Equals((AbstractType)this.VGrainType);
        }

        /// <inheritdoc/>
        protected override int OnStageGetHashCode()
        {
            return HashCode.Combine(this.CallMethodDefinition, this.VGrainType);
        }

        #endregion
    }
}
