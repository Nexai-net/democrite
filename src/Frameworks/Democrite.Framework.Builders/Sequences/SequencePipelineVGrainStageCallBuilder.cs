// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders.Steps;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Toolbox;

    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <inheritdoc />
    internal sealed class SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, TContextInfo> : SequencePipelineVGrainStageBaseBuilder<TVGrain, TInput>,
                                                                                                  ISequencePipelineStageCallBuilder<TInput, TVGrain>,
                                                                                                  ISequencePipelineStageCallBuilder<TVGrain>,
                                                                                                  ISequencePipelineStageContextedCallBuilder<TVGrain, TContextInfo>,
                                                                                                  ISequencePipelineStageContextedCallBuilder<TVGrain, TContextInfo, TInput>
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly Action<ISequencePipelineStageConfigurator<TInput>>? _configAction;
        private readonly ISequencePipelineBaseBuilder _sequenceBuilder;
        private readonly TContextInfo? _configuration;

        private CallStepBuilder? _callDefinition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineVGrainStageCallBuilder{TWorflowStage, TInput}"/> class.
        /// </summary>
        public SequencePipelineVGrainStageCallBuilder(ISequencePipelineBaseBuilder sequenceBuilder,
                                                     Action<ISequencePipelineStageConfigurator<TInput>>? configAction,
                                                     TContextInfo? contextInfo = default)
            : base(sequenceBuilder, configAction)
        {
            this._sequenceBuilder = sequenceBuilder;
            this._configAction = configAction;
            this._configuration = contextInfo;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain> ISequencePipelineStageCallBuilder<TVGrain>.Call<TOutputMessage>(Expression<Func<TVGrain, IExecutionContext, Task<TOutputMessage>>> expr)
        {
            return (ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain>)CallRecordStep<TOutputMessage>(expr, null);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TVGrain> ISequencePipelineStageCallBuilder<TVGrain>.Call(Expression<Func<TVGrain, IExecutionContext, Task>> expr)
        {
            return CallRecordStep<NoneType>(expr, null);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain> ISequencePipelineStageCallBuilder<TInput, TVGrain>.Call<TOutputMessage>(Expression<Func<TVGrain, TInput, IExecutionContext, Task<TOutputMessage>>> expr)
        {
            return (ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain>)CallRecordStep<TOutputMessage>(expr, typeof(TInput));
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TVGrain> ISequencePipelineStageCallBuilder<TInput, TVGrain>.Call(Expression<Func<TVGrain, TInput, IExecutionContext, Task>> expr)
        {
            return CallRecordStep<NoneType>(expr, typeof(TInput));
        }

        /// <inheritdoc />
        ISequencePipelineStageContextedCallBuilder<TVGrain, TCtx> ISequencePipelineStageCallBuilder<TVGrain>.Context<TCtx>(TCtx context)
        {
            return new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, TCtx>(this._sequenceBuilder, this._configAction, context);
        }

        /// <inheritdoc />
        ISequencePipelineStageContextedCallBuilder<TVGrain, TCtx, TInput> ISequencePipelineStageCallBuilder<TInput, TVGrain>.Configure<TCtx>(TCtx context)
        {
            return new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, TCtx>(this._sequenceBuilder, this._configAction, context);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain> ISequencePipelineStageContextedCallBuilder<TVGrain, TContextInfo>.Call<TOutputMessage>(Expression<Func<TVGrain, IExecutionContext<TContextInfo>, Task<TOutputMessage>>> expr)
        {
            return (ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain>)CallRecordStep<TOutputMessage>(expr, NoneType.Trait);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain> ISequencePipelineStageContextedCallBuilder<TVGrain, TContextInfo, TInput>.Call<TOutputMessage>(Expression<Func<TVGrain, TInput, IExecutionContext<TContextInfo>, Task<TOutputMessage>>> expr)
        {
            return (ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain>)CallRecordStep<TOutputMessage>(expr, typeof(TInput));
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TVGrain> ISequencePipelineStageContextedCallBuilder<TVGrain, TContextInfo>.Call(Expression<Func<TVGrain, IExecutionContext<TContextInfo>, Task>> expr)
        {
            return CallRecordStep<NoneType>(expr, NoneType.Trait);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TVGrain> ISequencePipelineStageContextedCallBuilder<TVGrain, TContextInfo, TInput>.Call(Expression<Func<TVGrain, TInput, IExecutionContext<TContextInfo>, Task>> expr)
        {
            return CallRecordStep<NoneType>(expr, typeof(TInput));
        }

        /// <summary>
        /// Save in <see cref="T:Democrite.Framework.Core.Abstractions.Sequence.SequenceStageBaseDefinition" /> if element is root
        /// </summary>
        /// <returns></returns>
        protected override SequenceStageBaseDefinition InternalToDefinition()
        {
            ArgumentNullException.ThrowIfNull(this._callDefinition);

            var option = BuildConfigDefinition();
            return this._callDefinition.ToDefinition(option, this.ConfigPreventOutput, this._configuration);
        }

        #region Tools

        /// <Summary>
        /// Recoard a new call step
        /// </Summary>
        private ISequencePipelineStageFinalizerBuilder<TVGrain> CallRecordStep<TOutputMessage>(Expression expr, Type? input)
        {
            this._callDefinition = CallStepBuilder.FromExpression<TVGrain, TOutputMessage>(expr,
                                                                                          input,
                                                                                          this._configuration,
                                                                                          this._configuration != null ? typeof(TContextInfo) : NoneType.Trait);

            return new SequencePipelineVGrainStageBaseBuilder<TVGrain, TOutputMessage>(this);
        }

        #endregion

        #endregion
    }
}
