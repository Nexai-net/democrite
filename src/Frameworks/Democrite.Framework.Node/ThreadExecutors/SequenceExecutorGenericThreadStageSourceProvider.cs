// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions;

    using Microsoft.Extensions.DependencyInjection;

    using System;

    /// <summary>
    /// Define a generic provider of thread state provider
    /// </summary>
    /// <typeparam name="TStageDefinition">The type of the stage definition.</typeparam>
    /// <typeparam name="IHandler">The type of the handler.</typeparam>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    internal class SequenceExecutorGenericThreadStageSourceProvider<TStageDefinition, IHandler> : ISequenceExecutorThreadStageSourceProvider
        where TStageDefinition : SequenceStageDefinition
        where IHandler : ISequenceExecutorThreadStageHandler
    {
        #region Fields
        
        private readonly IServiceProvider _serviceProvider;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorGenericThreadStageSourceProvider{TStageDefinition, IHandler}"/> class.
        /// </summary>
        public SequenceExecutorGenericThreadStageSourceProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandler(SequenceStageDefinition stage)
        {
            return stage is TStageDefinition;
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(SequenceStageDefinition stage)
        {
            return this._serviceProvider.GetRequiredService<IHandler>();
        }

        #endregion
    }
}
