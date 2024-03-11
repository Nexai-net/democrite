// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Thread stage executor managing simple input filter
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorFilterThreadStageProvider : SequenceExecutorBaseThreadStageProvider<SequenceStageFilterDefinition>, ISequenceExecutorThreadStageSourceProvider
    {
        #region Fields

        private static readonly Type s_handlerGenericTraits = typeof(SequenceExecutorThreadStageFilter<,>);

        private readonly Dictionary<AbstractType, ISequenceExecutorThreadStageHandler> _handlerCache;
        private readonly ReaderWriterLockSlim _locker;

        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorFilterThreadStageProvider"/> class.
        /// </summary>
        public SequenceExecutorFilterThreadStageProvider(IServiceProvider serviceProvider)
        {
            this._handlerCache = new Dictionary<AbstractType, ISequenceExecutorThreadStageHandler>();
            this._locker = new ReaderWriterLockSlim();
            this._serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override ISequenceExecutorThreadStageHandler OnProvide(SequenceStageFilterDefinition? stage)
        {
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
                    var handlerType = s_handlerGenericTraits.MakeGenericType(stage.Input.ToType(), stage.CollectionItemType.ToType());

                    newTypeHandler = (ISequenceExecutorThreadStageHandler?)ActivatorUtilities.CreateInstance(this._serviceProvider, handlerType);

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
