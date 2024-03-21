// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Patterns.Strategy;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base InMemory defintion provider
    /// </summary>
    public abstract class InMemoryBaseDefinitionProvider<TDefinition> : ProviderStrategyBaseSource<TDefinition, Guid>, IDefinitionInMemoryFillSourceProvider
        where TDefinition : class, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryBaseDefinitionProvider{TDefinition}"/> class.
        /// </summary>
        protected InMemoryBaseDefinitionProvider(IServiceProvider serviceProvider, IEnumerable<(Guid key, TDefinition value)>? initValues = null) 
            : base(serviceProvider, initValues)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(TDefinition defintion)
        {
            base.SafeAddOrReplace(defintion.Uid, defintion);
        }

        /// <inheritdoc />
        public bool CanStore(IDefinition definition)
        {
            return definition is TDefinition;
        }

        /// <inheritdoc />
        public bool TryStore(IDefinition definition)
        {
            if (definition is TDefinition tr)
            {
                AddOrUpdate(tr);
                return true;
            }

            return false;
        }

        #endregion
    }
}
