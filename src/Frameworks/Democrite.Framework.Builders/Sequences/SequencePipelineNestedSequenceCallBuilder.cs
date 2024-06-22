// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Core.Executions;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Build Stage that call another sequences
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <seealso cref="SequencePipelineStageBaseBuilder" />
    /// <seealso cref="ISequencePipelineNestedSequenceCallBuilder" />
    internal sealed class SequencePipelineNestedSequenceCallBuilder<TInput> : SequencePipelineStageBaseBuilder, ISequencePipelineNestedSequenceCallBuilder, ISequencePipelineNestedSequenceCallBuilder<TInput>, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly Action<IExecutionConfigurationBuilder>? _cfgBuilder;
        private readonly ISequencePipelineBaseBuilder _origin;
        private readonly Guid _sequenceId;

        private AbstractType? _output;
        private bool _relayInput;
        private AccessExpressionDefinition? _input;
        private AbstractMethod? _setMethodCall;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineNestedSequenceCallBuilder{TInput}"/> class.
        /// </summary>
        /// <param name="configAction"></param>
        public SequencePipelineNestedSequenceCallBuilder(ISequencePipelineBaseBuilder origin,
                                                         Guid sequenceId,
                                                         Action<IExecutionConfigurationBuilder>? cfgBuilder,
                                                         Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction,
                                                         Guid? fixUid)
            : base(metaDataBuilderAction, fixUid)
        {
            this._sequenceId = sequenceId;
            this._cfgBuilder = cfgBuilder;
            this._origin = origin;

            if (NoneType.IsEqualTo<TInput>() == false)
                this._input = ((Expression<Func<TInput, TInput>>)((TInput input) => input)).CreateAccess();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public ISequencePipelineBuilder ReturnNoData
        {
            get { return this._origin.EnqueueStage(FinalizeDefinition<NoneType>(false)); }
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TInput> ReturnCallInput
        {
            get { return this._origin.EnqueueStage<TInput>(FinalizeDefinition<TInput>(true)); }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISequencePipelineNestedSequenceCallBuilder SetInput<TSeqInput>(TSeqInput input)
        {
            this._input = input.CreateAccess();
            return this;
        }

        /// <inheritdoc />
        public ISequencePipelineNestedSequenceCallBuilder<TInput> OverrideInput<TSeqInput>(Expression<Func<TInput, TSeqInput>> inputProvider)
        {
            this._input = inputProvider.CreateAccess();
            return this;
        }

        /// <inheritdoc />
        ISequencePipelineNestedSequenceCallBuilder<TInput> ISequencePipelineNestedSequenceCallBaseBuilder<ISequencePipelineNestedSequenceCallBuilder<TInput>>.SetInput<TSeqInput>(TSeqInput input)
        {
            this._input = input.CreateAccess();
            return this;
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TOutput> Return<TOutput>()
        {
            return this._origin.EnqueueStage<TOutput>(FinalizeDefinition<TOutput>(false));
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TInput> ReturnUpdatedInput<TOutput>(Expression<Action<TInput, TOutput>> setMethod)
        {
            this._setMethodCall = (setMethod.Body as MethodCallExpression)?.Method.GetAbstractMethod() ?? throw new InvalidOperationException("Set Method must be a simple input.Set(Output)");
            return this._origin.EnqueueStage<TInput>(FinalizeDefinition<TInput>(true));
        }

        /// <inheritdoc />
        public SequenceStageDefinition ToDefinition()
        {
            var metaData = BuildDefinitionMetaData(out var displayName);

            ExecutionCustomizationDescriptions? customization = null;

            if (this._cfgBuilder != null)
            {
                var builder = new ExecutionConfigurationBuilder();
                this._cfgBuilder.Invoke(builder);

                // To config definition
                customization = builder.Build();
            }

            return new SequenceStageNestedSequenceCallDefinition(this.FixUid,
                                                                 displayName ?? "Nested",
                                                                 NoneType.IsEqualTo<TInput>() ? null : typeof(TInput).GetAbstractType(),
                                                                 this._output,
                                                                 this._sequenceId,
                                                                 this._relayInput,
                                                                 this._input,
                                                                 this._setMethodCall,
                                                                 customization,
                                                                 metaData,
                                                                 this._output is null);
        }

        #region Tools

        /// <summary>
        /// Builds the definition.
        /// </summary>
        private ISequencePipelineStageDefinitionProvider FinalizeDefinition<TOutput>(bool relayInput)
        {
            this._output = NoneType.IsEqualTo<TOutput>() ? null : typeof(TOutput).GetAbstractType();
            this._relayInput = relayInput;
            return this;
        }

        #endregion

        #endregion

    }
}
