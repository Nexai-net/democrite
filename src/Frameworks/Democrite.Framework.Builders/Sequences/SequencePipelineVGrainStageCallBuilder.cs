// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders.Steps;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using Elvex.Toolbox;

    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <inheritdoc />
    internal sealed class SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, TConfiguration> : SequencePipelineVGrainStageBaseBuilder<TVGrain, TInput>,
                                                                                                    ISequencePipelineStageCallBuilder<TInput, TVGrain>,
                                                                                                    ISequencePipelineStageCallBuilder<TVGrain>,
                                                                                                    ISequencePipelineStageConfiguredCallBuilder<TVGrain, TConfiguration>,
                                                                                                    ISequencePipelineStageConfiguredCallBuilder<TVGrain, TConfiguration, TInput>
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly Action<ISequencePipelineStageConfigurator>? _configAction;
        private readonly ISequencePipelineBaseBuilder _sequenceBuilder;

        private readonly Expression<Func<TInput, TConfiguration>>? _configurationProvider;
        private readonly TConfiguration? _configuration;

        private CallStepBuilder? _callDefinition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineVGrainStageCallBuilder{TWorflowStage, TInput}"/> class.
        /// </summary>
        public SequencePipelineVGrainStageCallBuilder(ISequencePipelineBaseBuilder sequenceBuilder,
                                                      Action<ISequencePipelineStageConfigurator>? configAction,
                                                      TConfiguration? configuration = default,
                                                      Expression<Func<TInput, TConfiguration>>? configurationProvider = null)
            : base(sequenceBuilder, configAction)
        {
            this._sequenceBuilder = sequenceBuilder;
            this._configAction = configAction;
            this._configuration = configuration;
            this._configurationProvider = configurationProvider;
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
        public ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain> Call<TOutputMessage>(Expression<Func<TVGrain, TInput, IExecutionContext, Task<TOutputMessage>>> expr)
        {
            return (ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain>)CallRecordStep<TOutputMessage>(expr, typeof(TInput));
        }

        /// <inheritdoc />
        public ISequencePipelineStageFinalizerBuilder<TVGrain> Call(Expression<Func<TVGrain, TInput, IExecutionContext, Task>> expr)
        {
            return CallRecordStep<NoneType>(expr, typeof(TInput));
        }

        /// <inheritdoc />
        ISequencePipelineStageConfiguredCallBuilder<TVGrain, TCtx> ISequencePipelineStageCallBuilder<TVGrain>.Configure<TCtx>(TCtx context)
        {
            return new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, TCtx>(this._sequenceBuilder, this._configAction, context);
        }

        /// <inheritdoc />
        ISequencePipelineStageConfiguredCallBuilder<TVGrain, TCtx, TInput> ISequencePipelineStageCallBuilder<TInput, TVGrain>.Configure<TCtx>(TCtx context)
        {
            return new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, TCtx>(this._sequenceBuilder, this._configAction, context);
        }

        /// <summary>
        /// Add execution context configuration from input data
        /// </summary>
        public ISequencePipelineStageConfiguredCallBuilder<TVGrain, TCtx, TInput> ConfigureFromInput<TCtx>(Expression<Func<TInput, TCtx>> executionConfiguration)
        {
            return new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, TCtx>(this._sequenceBuilder, this._configAction, default, executionConfiguration);
        }

        /// <summary>
        /// Use input as execution context configuration
        /// </summary>
        /// <remarks>
        ///     Use full when configuration must depend of the input 
        /// </remarks>
        public ISequencePipelineStageConfiguredCallBuilder<TVGrain, TInput, TInput> UseInputAsConfiguration()
        {
            return ConfigureFromInput(i => i);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain> ISequencePipelineStageConfiguredCallBuilder<TVGrain, TConfiguration>.Call<TOutputMessage>(Expression<Func<TVGrain, IExecutionContext<TConfiguration>, Task<TOutputMessage>>> expr)
        {
            return (ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain>)CallRecordStep<TOutputMessage>(expr, NoneType.Trait);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain> ISequencePipelineStageConfiguredCallBuilder<TVGrain, TConfiguration, TInput>.Call<TOutputMessage>(Expression<Func<TVGrain, TInput, IExecutionContext<TConfiguration>, Task<TOutputMessage>>> expr)
        {
            return (ISequencePipelineStageFinalizerBuilder<TOutputMessage, TVGrain>)CallRecordStep<TOutputMessage>(expr, typeof(TInput));
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TVGrain> ISequencePipelineStageConfiguredCallBuilder<TVGrain, TConfiguration>.Call(Expression<Func<TVGrain, IExecutionContext<TConfiguration>, Task>> expr)
        {
            return CallRecordStep<NoneType>(expr, NoneType.Trait);
        }

        /// <inheritdoc />
        ISequencePipelineStageFinalizerBuilder<TVGrain> ISequencePipelineStageConfiguredCallBuilder<TVGrain, TConfiguration, TInput>.Call(Expression<Func<TVGrain, TInput, IExecutionContext<TConfiguration>, Task>> expr)
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

            var access = this._configurationProvider?.CreateAccess() ?? this._configuration?.CreateAccess();

            var option = BuildConfigDefinition();
            return this._callDefinition.ToDefinition<TConfiguration>(option,
                                                                     this.ConfigPreventOutput,
                                                                     access);
        }

        #region Tools

        /// <Summary>
        /// Recoard a new call step
        /// </Summary>
        private ISequencePipelineStageFinalizerBuilder<TVGrain> CallRecordStep<TOutputMessage>(Expression expr, Type? input)
        {
            this._callDefinition = CallStepBuilder.FromExpression<TVGrain, TOutputMessage>(expr,
                                                                                           input,
                                                                                           this._configuration != null ? typeof(TConfiguration) : NoneType.Trait,
                                                                                           this._configurationProvider is null ? this._configuration : null);

            return new SequencePipelineVGrainStageBaseBuilder<TVGrain, TOutputMessage>(this);
        }

        #endregion

        #endregion
    }
}
