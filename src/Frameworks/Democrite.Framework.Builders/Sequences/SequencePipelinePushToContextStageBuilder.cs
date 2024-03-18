// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;

    using System.Linq.Expressions;

    /// <summary>
    /// Builder about <see cref="SequenceStagePushToContextDefinition"/> a stage able to push an information into <see cref="IExecutionContext"/>
    /// </summary>
    /// <seealso cref="ISequencePipelineStageDefinitionProvider" />
    internal sealed class SequencePipelinePushToContextStageBuilder<TInput> : SequencePipelineStageBaseBuilder, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly LambdaExpression _inputExpression;
        private readonly bool _override;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelinePushToContextStageBuilder"/> class.
        /// </summary>
        public SequencePipelinePushToContextStageBuilder(LambdaExpression inputExpression,
                                                         bool @override, 
                                                         Action<ISequencePipelineStageConfigurator>? configAction)
            : base(configAction)
        {
            this._inputExpression = inputExpression;
            this._override = @override;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageBaseDefinition ToDefinition()
        {
            //MemberInitializationDefinition? initMember = null;
            //string? callChain = null;

            //if (this._inputExpression is not null)
            //{
            //    if (this._inputExpression.NodeType != ExpressionType.Lambda)
            //        throw new InvalidOperationException("Only lambda type could be used as configuration provider");

            //    if (this._inputExpression.Body.NodeType == ExpressionType.MemberInit)
            //        initMember = this._inputExpression.SerializeMemberInitialization();
            //    else
            //        callChain = DynamicCallHelper.GetCallChain(this._inputExpression)!;
            //}

            var access = this._inputExpression.CreateAccess();

            return new SequenceStagePushToContextDefinition(typeof(TInput).GetAbstractType(),
                                                            access,
                                                            this._override);
        }

        #endregion
    }
}
