// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions;

    using Elvex.Toolbox.Disposables;

    /// <summary>
    /// Base Thread stage executor
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal abstract class SequenceExecutorBaseThreadStageProvider<TStageDefinition> : SafeDisposable, ISequenceExecutorThreadStageSourceProvider
        where TStageDefinition : SequenceStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorBaseThreadStageProvider"/> class.
        /// </summary>
        public SequenceExecutorBaseThreadStageProvider()
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual bool CanHandler(SequenceStageDefinition? stage)
        {
            return stage is TStageDefinition;
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(SequenceStageDefinition? stageBase)
        {
            return OnProvide((TStageDefinition?)stageBase);
        }

        /// <inheritdoc cref="ISequenceExecutorThreadStageSourceProvider.Provide(SequenceStageDefinition)" />
        protected abstract ISequenceExecutorThreadStageHandler OnProvide(TStageDefinition? stageBase);

        #endregion
    }
}
