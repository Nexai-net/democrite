// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stage related to filter some input
    /// </summary>
    /// <seealso cref="ISequenceStageDefinition" />
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class SequenceStageFilterDefinition : SequenceStageBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageFilterDefinition"/> class.
        /// </summary>
        public SequenceStageFilterDefinition(CollectionType? input,
                                             ConditionExpressionDefinition condition,
                                             SequenceOptionStageDefinition? options = null,
                                             bool preventReturn = false,
                                             Guid? uid = null)
            : base(StageTypeEnum.Filter, input, input, options, preventReturn, uid)
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
        public ConditionExpressionDefinition Condition { get; }

        /// <summary>
        /// Gets the type of the collection item.
        /// </summary>
        [DataMember]
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
        protected override bool OnStageEquals(ISequenceStageDefinition other)
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
        /// Create from specific expression
        /// </summary>
        public static SequenceStageFilterDefinition From<TInputCollection, TInput>(Expression<Func<TInput, bool>> filter,
                                                                                   SequenceOptionStageDefinition? options = null,
                                                                                   bool preventReturn = false,
                                                                                   Guid? uid = null)
            where TInputCollection : IEnumerable<TInput>
        {
            return new SequenceStageFilterDefinition((CollectionType)typeof(TInputCollection).GetAbstractType(),
                                                     filter.Serialize(),
                                                     options,
                                                     preventReturn,
                                                     uid);
        }

        #endregion

    }
}
