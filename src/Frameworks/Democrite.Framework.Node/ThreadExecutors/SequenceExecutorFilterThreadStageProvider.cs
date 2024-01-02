// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Thread stage executor managing simple input filter
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorFilterThreadStageProvider : SafeDisposable, ISequenceExecutorThreadStageProvider
    {
        #region Fields

        private static readonly Type s_handlerGenericTraits = typeof(SequenceExecutorFilterThreadStageHandler<,>);

        private readonly Dictionary<AbstractType, ISequenceExecutorThreadStageHandler> _handlerCache;
        private readonly ReaderWriterLockSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorFilterThreadStageProvider"/> class.
        /// </summary>
        public SequenceExecutorFilterThreadStageProvider()
        {
            this._handlerCache = new Dictionary<AbstractType, ISequenceExecutorThreadStageHandler>();
            this._locker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandler(ISequenceStageDefinition? stage)
        {
            return stage is SequenceStageFilterDefinition;
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(ISequenceStageDefinition? stageBase)
        {
            var stage = stageBase as SequenceStageFilterDefinition;

            ArgumentNullException.ThrowIfNull(stage?.Input);

            this._locker.EnterReadLock();
            try
            {
                if (this._handlerCache.TryGetValue(stage.Input, out var typeHandler))
                    return typeHandler;
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            this._locker.EnterWriteLock();
            try
            {

                ISequenceExecutorThreadStageHandler? newTypeHandler;

                if (!this._handlerCache.TryGetValue(stage.Input, out newTypeHandler))
                {
                    newTypeHandler = (ISequenceExecutorThreadStageHandler?)Activator.CreateInstance(s_handlerGenericTraits.MakeGenericType(stage.Input.ToType(), stage.CollectionItemType.ToType()));

                    Debug.Assert(newTypeHandler != null);
                    this._handlerCache.Add(stage.Input, newTypeHandler);
                }

                return newTypeHandler;
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        #endregion
    }
}
