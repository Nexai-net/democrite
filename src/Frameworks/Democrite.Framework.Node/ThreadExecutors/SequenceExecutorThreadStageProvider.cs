// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions;
    using Elvex.Toolbox.Helpers;

    using System.Collections.Generic;
    using System.Linq;

    internal sealed class SequenceExecutorThreadStageProvider : ISequenceExecutorThreadStageProvider
    {
        #region Fields
        
        private readonly IReadOnlyCollection<ISequenceExecutorThreadStageSourceProvider> _sourceProviders;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStageProvider"/> class.
        /// </summary>
        public SequenceExecutorThreadStageProvider(IEnumerable<ISequenceExecutorThreadStageSourceProvider> sourceProviders)
        {
            this._sourceProviders = sourceProviders?.ToArray() ?? EnumerableHelper<ISequenceExecutorThreadStageSourceProvider>.ReadOnlyArray;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandler(SequenceStageDefinition stage)
        {
            return this._sourceProviders.Any(s => s.CanHandler(stage));
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(SequenceStageDefinition stage)
        {
            var sourceProvider = this._sourceProviders.FirstOrDefault(s => s.CanHandler(stage));

            if (sourceProvider is null)
                throw new StageExecutorNotFoundException(stage);

            return sourceProvider.Provide(stage);
        }

        #endregion
    }
}
