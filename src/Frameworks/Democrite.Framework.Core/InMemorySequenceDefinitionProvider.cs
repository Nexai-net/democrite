// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// <see cref="SequenceDefinition"/> provider register in memory
    /// </summary>
    /// <seealso cref="ISequenceDefinitionSourceProvider" />
    public sealed class InMemorySequenceDefinitionProvider : ProviderStrategyBaseSource<SequenceDefinition, Guid>, ISequenceDefinitionSourceProvider
    {
        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(SequenceDefinition triggerDefinition)
        {
            base.SafeAddOrReplace(triggerDefinition.Uid, triggerDefinition);
        }

        #endregion
    }
}
