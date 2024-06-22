// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stage that call another sequence
    /// </summary>
    /// <seealso cref="SequenceStageDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceStageNestedSequenceCallDefinition : SequenceStageDefinition 
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageNestedSequenceCallDefinition"/> class.
        /// </summary>
        public SequenceStageNestedSequenceCallDefinition(Guid uid,
                                                         string displayName,
                                                         AbstractType? input,
                                                         AbstractType? output,
                                                         Guid sequenceId,
                                                         bool relayInput,
                                                         AccessExpressionDefinition? sequenceInput,
                                                         AbstractMethod? setMethod,
                                                         ExecutionCustomizationDescriptions? customizationDescriptions,
                                                         DefinitionMetaData? metaData,
                                                         bool preventReturn = false)
            : base(uid, displayName, StageTypeEnum.NestedSequenceCall, input, output, metaData, preventReturn)
        {
            this.SequenceId = sequenceId;
            this.RelayInput = relayInput;
            this.SequenceInput = sequenceInput;
            this.SetMethod = setMethod;
            this.CustomizationDescriptions = customizationDescriptions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the sequence identifier.
        /// </summary>
        [DataMember]
        [Id(0)]
        public Guid SequenceId { get; }

        /// <summary>
        /// Gets a value indicating whether [relay input].
        /// </summary>
        [DataMember]
        [Id(1)]
        public bool RelayInput { get; }

        /// <summary>
        /// Gets the sequence input.
        /// </summary>
        [DataMember]
        [Id(2)]
        public AccessExpressionDefinition? SequenceInput { get; }

        /// <summary>
        /// Gets the customization descriptions.
        /// </summary>
        [DataMember]
        [Id(3)]
        public ExecutionCustomizationDescriptions? CustomizationDescriptions { get; }

        /// <summary>
        /// Gets the set method.
        /// </summary>
        [DataMember]
        [Id(4)]
        public AbstractMethod? SetMethod { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnStageEquals(SequenceStageDefinition other)
        {
            return other is SequenceStageNestedSequenceCallDefinition otherNested &&
                   this.SequenceId == otherNested.SequenceId &&
                   this.RelayInput == otherNested.RelayInput &&
                   this.SetMethod == otherNested.SetMethod &&
                   this.SequenceInput == otherNested.SequenceInput &&
                   this.CustomizationDescriptions == otherNested.CustomizationDescriptions;
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return HashCode.Combine(this.SequenceId,
                                    this.SequenceInput,
                                    this.SetMethod,
                                    this.RelayInput,
                                    this.CustomizationDescriptions);
        }

        #endregion
    }
}
