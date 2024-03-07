// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Patterns.Strategy
{
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Base provider strategy
    /// </summary>
    /// <seealso cref="IProviderStrategy{T, TKey}" />
    public abstract class ProviderStrategyBase<T, TKey, TSource> : IProviderStrategy<T, TKey>
        where T : class
        where TKey : notnull
        where TSource : IProviderStrategySource<T, TKey>
    {
        #region Fields

        private readonly IReadOnlyCollection<TSource> _providerSource;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactResourceProvider"/> class.
        /// </summary>
        public ProviderStrategyBase(IEnumerable<TSource> providerSource,
                                    ILogger? logger)
        {
            this._logger = logger ?? NullLogger.Instance;
            this._providerSource = providerSource?.ToArray() ?? EnumerableHelper<TSource>.ReadOnlyArray;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when source have been update.
        /// </summary>
        /// <remarks>
        /// 
        ///     Not connect yet
        ///     
        /// </remarks>
        public event EventHandler<IReadOnlyCollection<TKey>>? DataChanged;

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<T>> GetAllValuesAsync(CancellationToken token)
        {
            var tasks = this._providerSource.Select(source => source.GetAllValuesAsync(token).AsTask())
                                            .ToList();

            return await GetResults(tasks);
        }

        /// <inheritdoc />
        public virtual async ValueTask<T?> GetFirstValueByIdAsync(TKey key, CancellationToken token)
        {
            foreach (var source in this._providerSource)
            {
                var result = await source.TryGetDataAsync(key);
                if (result.Success)
                    return result.Result;
            }

            return null;
        }

        /// <inheritdoc />
        public virtual ValueTask<IReadOnlyCollection<T>> GetValuesAsync(CancellationToken token, params TKey[] keys)
        {
            if (keys.Length == 0)
                return ValueTask.FromResult(EnumerableHelper<T>.ReadOnly);
            return GetValuesAsync((IReadOnlyCollection<TKey>)keys, token);
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<T>> GetValuesAsync(IReadOnlyCollection<TKey> keys, CancellationToken token)
        {
            if (keys.Count == 0)
                return EnumerableHelper<T>.ReadOnly;

            var tasks = this._providerSource.Select(source => source.GetValuesAsync(keys, token).AsTask())
                                            .ToList();

            return await GetResults(tasks);
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<T>> GetValuesAsync(Expression<Func<T, bool>> filter, CancellationToken token)
        {
            var predicate = filter.Compile();
            var tasks = this._providerSource.Select(source => source.GetValuesAsync(filter, predicate, token).AsTask())
                                            .ToList();

            return await GetResults(tasks);
        }

        /// <inheritdoc />
        public async ValueTask<T?> GetFirstValueAsync(Expression<Func<T, bool>> filter, CancellationToken token)
        {
            var predicate = filter.Compile();

            foreach (var source in this._providerSource)
            {
                var result = await source.GetFirstValueAsync(filter, predicate, token);
                if (!EqualityComparer<T>.Default.Equals(result, default))
                    return result;
            }

            return default;
        }

        /// <inheritdoc />
        public virtual async ValueTask<(bool Result, T? value)> TryGetFirstValueAsync(TKey key, CancellationToken token)
        {
            var value = await GetFirstValueByIdAsync(key, token);
            return (value is not null, value);
        }

        /// <inheritdoc />
        public ValueTask ForceUpdateAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #region Tools

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected abstract Expression<Func<T, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<TKey> keys);

        /// <summary>
        /// Raises the data changed.
        /// </summary>
        protected void RaiseDataChanged(IReadOnlyCollection<TKey> dataUpdated)
        {
            DataChanged?.Invoke(this, dataUpdated);
        }

        /// <summary>
        /// Gets the results from tasks.
        /// </summary>
        private async ValueTask<IReadOnlyCollection<T>> GetResults(IReadOnlyList<Task<IReadOnlyCollection<T>>> tasks)
        {
            var tasksCollection = tasks.Where(t => t.IsCompleted == false);

            while (tasksCollection.Any())
            {
                try
                {
                    await Task.WhenAll(tasksCollection);
                }
                catch
                {
                }
                finally
                {
                }
            }

            var results = new HashSet<T>(tasks.Count * 2);
            foreach (var task in tasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    foreach (var taskResult in task.Result)
                        results.Add(taskResult);
                }
                else if (task.IsCanceled)
                {
                    continue;
                }
                else if (task.Exception != null)
                {
                    this._logger.OptiLog(LogLevel.Error,
                                         "{source} [StrategyProvider - exception : {exception}]",
                                         GetType(),
                                         task.Exception);
                }
            }

            return results;
        }

        #endregion

        #endregion
    }
}
