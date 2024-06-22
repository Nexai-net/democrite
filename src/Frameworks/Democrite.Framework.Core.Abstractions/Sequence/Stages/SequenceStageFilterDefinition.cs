// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stage related to filter some input
    /// </summary>
    /// <seealso cref="ISequenceStageDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceStageFilterDefinition : SequenceStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageFilterDefinition"/> class.
        /// </summary>
        public SequenceStageFilterDefinition(Guid uid,
                                             CollectionType? input,
                                             string displayName,
                                             ConditionExpressionDefinition condition,
                                             DefinitionMetaData? metaData,
                                             bool preventReturn = false)
            : base(uid, displayName, StageTypeEnum.Filter, input, input, metaData, preventReturn)
        {
            ArgumentNullException.ThrowIfNull(condition);

            this.Condition = condition;

            ArgumentNullException.ThrowIfNull(input);

            this.CollectionItemType = input.ItemAbstractType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the condition to filter
        /// </summary>
        [DataMember]
        [Id(0)]
        public ConditionExpressionDefinition Condition { get; }

        /// <summary>
        /// Gets the type of the collection item.
        /// </summary>
        [DataMember]
        [Id(1)]
        public AbstractType CollectionItemType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts content condition to executable expression
        /// </summary>
        public Expression<Func<TInput, bool>> ToExpression<TInput>()
        {
            // Optimization : Could be cached
            return this.Condition.ToExpression<TInput, bool>();
        }

        /// <inheritdoc />
        protected override bool OnStageEquals(SequenceStageDefinition other)
        {
            return other is SequenceStageFilterDefinition otherStepDef &&
                   otherStepDef.Condition == this.Condition;
        }

        /// <inheritdoc />
        protected override int OnStageGetHashCode()
        {
            return this.Condition.GetHashCode();
        }

        /// <summary>
        /// Get from specific expression
        /// </summary>
        public static SequenceStageFilterDefinition From<TInputCollection, TInput>(Expression<Func<TInput, bool>> filter,
                                                                                   DefinitionMetaData? metaData = null,
                                                                                   bool preventReturn = false,
                                                                                   Guid? uid = null)
            where TInputCollection : IEnumerable<TInput>
        {
            return new SequenceStageFilterDefinition(uid ?? Guid.NewGuid(),
                                                     (CollectionType)typeof(TInputCollection).GetAbstractType(),
                                                     "Filter",
                                                     filter.Serialize(),
                                                     metaData,
                                                     preventReturn);
        }

        #endregion

    }
}
