// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Abstractions.Patterns.Strategy;
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc cref="ISequenceDefinitionProvider"/>
    /// <seealso cref="ISequenceDefinitionProvider" />
    public abstract class DefinitionBaseProvider<TDefinition, TSpecificProvider> : ProviderStrategyBase<TDefinition, Guid, IDefinitionSourceProvider<TDefinition>>, ISpecificDefinitionSourceProvider
        where TDefinition : class, IDefinition
        where TSpecificProvider : IDefinitionSourceProvider<TDefinition>
    {
        #region Fields
        
        private readonly ILogger<TSpecificProvider> _localLogger;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinitionProvider"/> class.
        /// </summary>
        public DefinitionBaseProvider(IEnumerable<TSpecificProvider> specificDefinitionProviderSources,
                                      IEnumerable<IDefinitionSourceProvider<TDefinition>> genericDefinitionSourceProviders,
                                      ILogger<TSpecificProvider> logger)

            : base(specificDefinitionProviderSources.Cast<IDefinitionSourceProvider<TDefinition>>()
                                                    .Concat(genericDefinitionSourceProviders).Distinct(), logger)
        {
            foreach (var provider in specificDefinitionProviderSources)
                provider.DataChanged += Provider_DataChanged;

            this._localLogger = logger;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        IReadOnlyCollection<Guid> IProviderStrategySource<IDefinition, Guid>.Keys
        {
            get { return this.ProviderSource.SelectMany(p => p.Keys).Distinct().ToArray(); }
        }

        /// <inheritdoc />
        bool ISupportInitialization.IsInitializing
        {
            get { return false; }
        }

        /// <inheritdoc />
        bool ISupportInitialization.IsInitialized
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected override Expression<Func<TDefinition, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return (t => keys.Contains(t.Uid));
        }

        /// <inheritdoc />
        async ValueTask IProviderStrategySource<IDefinition, Guid>.ForceUpdateAsync(CancellationToken token)
        {
            foreach (var provider in this.ProviderSource)
                await provider.ForceUpdateAsync(token);
        }

        /// <inheritdoc />
        async ValueTask<IReadOnlyCollection<IDefinition>> IProviderStrategySource<IDefinition, Guid>.GetAllValuesAsync(CancellationToken token)
        {
            var allTasks = this.ProviderSource.Select(p => p.GetAllValuesAsync(token)).ToArray();
            await allTasks.SafeWhenAllAsync(token);
            var results = await allTasks.AggregateResults(this._localLogger);
            return results;
        }

        /// <inheritdoc />
        async ValueTask<IDefinition?> IProviderStrategySource<IDefinition, Guid>.GetFirstValueAsync(Expression<Func<IDefinition, bool>> filterExpression, Func<IDefinition, bool> filter, CancellationToken token)
        {
            var transmute = filterExpression.ReplaceParameter<IDefinition, TDefinition, bool>(p => p);

            var allTasks = this.ProviderSource.Select(p => p.GetFirstValueAsync(transmute, filter, token)).ToArray();
            await allTasks.SafeWhenAllAsync(token);
            var results = await allTasks.AggregateResults(this._localLogger);
            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        async ValueTask<IReadOnlyCollection<IDefinition>> IProviderStrategySource<IDefinition, Guid>.GetValuesAsync(Expression<Func<IDefinition, bool>> filterExpression, Func<IDefinition, bool> filter, CancellationToken token)
        {
            var transmute = filterExpression.ReplaceParameter<IDefinition, TDefinition, bool>(p => p);

            var allTasks = this.ProviderSource.Select(p => p.GetValuesAsync(transmute, filter, token)).ToArray();
            await allTasks.SafeWhenAllAsync(token);
            var results = await allTasks.AggregateResults(this._localLogger);
            return results;
        }

        /// <inheritdoc />
        async ValueTask<IReadOnlyCollection<IDefinition>> IProviderStrategySource<IDefinition, Guid>.GetValuesAsync(IEnumerable<Guid> keys, CancellationToken token)
        {
            var allTasks = this.ProviderSource.Select(p => p.GetValuesAsync(keys, token)).ToArray();
            await allTasks.SafeWhenAllAsync(token);
            var results = await allTasks.AggregateResults(this._localLogger);
            return results;
        }

        async ValueTask<IReadOnlyCollection<Guid>> IProviderStrategySource<IDefinition, Guid>.GetKeysAsync(Expression<Func<IDefinition, bool>> filterExpression, Func<IDefinition, bool> filter, CancellationToken token)
        {
            var transmute = filterExpression.ReplaceParameter<IDefinition, TDefinition, bool>(p => p);

            var allTasks = this.ProviderSource.Select(p => p.GetKeysAsync(transmute, filter, token)).ToArray();
            await allTasks.SafeWhenAllAsync(token);
            var results = await allTasks.AggregateResults(this._localLogger);
            return results;
        }

        /// <inheritdoc />
        ValueTask ISupportInitialization.InitializationAsync(CancellationToken token)
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        async ValueTask<(bool Success, IDefinition? Result)> IProviderStrategySource<IDefinition, Guid>.TryGetDataAsync(Guid key, CancellationToken token)
        {
            var allTasks = this.ProviderSource.Select(p => p.TryGetDataAsync(key, token)).ToArray();
            await allTasks.SafeWhenAllAsync(token);
            var results = await allTasks.AggregateResults(this._localLogger);
            return results.First();
        }

        #region Tools

        /// <summary>
        /// Bases the inst.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ProviderStrategyBase<TDefinition, Guid, IDefinitionSourceProvider<TDefinition>> BaseInst()
        {
            return this;
        }

        /// <summary>
        /// Providers the data changed.
        /// </summary>
        private void Provider_DataChanged(object? sender, IReadOnlyCollection<Guid> e)
        {
            RaiseDataChanged(e);
        }

        #endregion

        #endregion
    }
}
