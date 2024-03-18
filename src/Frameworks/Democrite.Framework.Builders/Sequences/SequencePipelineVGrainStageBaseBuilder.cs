// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;
    using System.Diagnostics;

    /// <inheritdoc />
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class SequencePipelineVGrainStageBaseBuilder<TVGrain, TInput> : SequencePipelineStageBaseBuilder,
                                                                            
                                                                             ISequencePipelineStageBaseBuilder,
                                                                             ISequencePipelineStageFinalizerBuilder<TVGrain>,
                                                                             ISequencePipelineStageFinalizerBuilder<TInput, TVGrain>,
                                                                             ISequencePipelineStageDefinitionProvider,
                                                                            
                                                                             IInternalSequencePipelineVGrainStageBaseBuilder
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly ISequencePipelineBaseBuilder? _sequencePipelineBuilder;
        private readonly IInternalSequencePipelineVGrainStageBaseBuilder? _root;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineVGrainStageBaseBuilder{TWorflowStage, TInput}"/> class.
        /// </summary>
        public SequencePipelineVGrainStageBaseBuilder(ISequencePipelineBaseBuilder sequencePipelineBuilder, Action<ISequencePipelineStageConfigurator>? configAction)
            : base(configAction)
        {
            this._sequencePipelineBuilder = sequencePipelineBuilder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineVGrainStageBaseBuilder{TWorflowStage, TInput}"/> class.
        /// </summary>
        internal SequencePipelineVGrainStageBaseBuilder(IInternalSequencePipelineVGrainStageBaseBuilder root)
            : base(null)
        {
            this._root = root;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether [configuration prevent output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [configuration prevent output]; otherwise, <c>false</c>.
        /// </value>
        internal bool ConfigPreventOutput { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the sequence pipeline builder.
        /// </summary>
        ISequencePipelineBaseBuilder IInternalSequencePipelineVGrainStageBaseBuilder.SequencePipelineBuilder
        {
            get { return (this._root?.SequencePipelineBuilder ?? this._sequencePipelineBuilder) ?? throw new ArgumentNullException("Root missing"); }
        }

        /// <inheritdoc />
        ISequencePipelineBuilder ISequencePipelineStageFinalizerBuilder<TVGrain>.Return
        {
            get
            {
                if (this._root == null)
                    ((IInternalSequencePipelineVGrainStageBaseBuilder)this).PreventOutput();
                else
                    this._root.PreventOutput();

                return ((IInternalSequencePipelineVGrainStageBaseBuilder)this).SequencePipelineBuilder.EnqueueStage(this);
            }
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineStageFinalizerBuilder<TInput, TVGrain>.Return
        {
            get { return ((IInternalSequencePipelineVGrainStageBaseBuilder)this).SequencePipelineBuilder.EnqueueStage<TInput>(this); }
        }

        /// <inheritdoc />
        ISequencePipelineBuilder ISequencePipelineStageFinalizerBuilder<TInput, TVGrain>.ReturnNoMessage
        {
            get
            {
                if (this._root == null)
                    ((IInternalSequencePipelineVGrainStageBaseBuilder)this).PreventOutput();
                else
                    this._root.PreventOutput();

                return ((IInternalSequencePipelineVGrainStageBaseBuilder)this).SequencePipelineBuilder.EnqueueStage(this);
            }
        }

        /// <exception cref="System.NotImplementedException"></exception>
        public ISequencePipelineStageFinalizerBuilder<TVGrain> SendCopyTo(Action<string> targetDefinition)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="System.NotImplementedException"></exception>
        ISequencePipelineStageFinalizerBuilder<TInput, TVGrain> ISequencePipelineStageFinalizerBuilder<TInput, TVGrain>.SendCopyTo(Action<string> targetDefinition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts to definition.
        /// </summary>
        public virtual SequenceStageBaseDefinition ToDefinition()
        {
            return this._root?.ToDefinition() ?? InternalToDefinition();
        }

        /// <summary>
        /// Save in <see cref="ISequenceStageDefinition"/> if element is root
        /// </summary>
        protected virtual SequenceStageBaseDefinition InternalToDefinition()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prevents the output.
        /// </summary>
        void IInternalSequencePipelineVGrainStageBaseBuilder.PreventOutput()
        {
            this.ConfigPreventOutput = true;
        }

        #region Tools

        /// <summary>
        /// Gets the debugger display.
        /// </summary>
        private string GetDebuggerDisplay()
        {
            return ToString() ?? GetType().Name;
        }

        /// <inheritdoc />
        protected override SequenceOptionStageDefinition? BuildConfigDefinition()
        {
            if (this._root != null)
                return this._root.ConfigOption();

            return base.BuildConfigDefinition();
        }

        /// <inheritdoc />
        SequenceOptionStageDefinition? IInternalSequencePipelineVGrainStageBaseBuilder.ConfigOption()
        {
            return BuildConfigDefinition();
        }

        #endregion

        #endregion
    }
}
