// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy
{
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System.Linq.Expressions;

    /// <summary>
    /// Define a source of information consumed by <see cref="IProviderStrategy{T, TKey}"/>
    /// </summary>
    public interface IProviderStrategySource<T, TKey> : ISupportInitialization
        where T : class
        where TKey : notnull
    {
        #region Properties

        /// <summary>
        /// Gets all loaded keys.
        /// </summary>
        IReadOnlyCollection<TKey> Keys { get; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when intformation have been updated.
        /// </summary>
        event EventHandler<IReadOnlyCollection<TKey>> DataChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Gets all values
        /// </summary>
        ValueTask<IReadOnlyCollection<T>> GetAllValuesAsync(CancellationToken token);

        /// <summary>
        /// Tries get data <typeparamref name="T"/> from key <paramref name="key"/>
        /// </summary>
        ValueTask<(bool Success, T? Result)> TryGetDataAsync(TKey key, CancellationToken token = default);

        /// <summary>
        /// Gets the values based on filter
        /// </summary>
        ValueTask<IReadOnlyCollection<T>> GetValuesAsync(Expression<Func<T, bool>> filterExpression, Func<T, bool> filter, CancellationToken token = default);

        /// <summary>
        /// Gets the values keys
        /// </summary>
        ValueTask<IReadOnlyCollection<T>> GetValuesAsync(IEnumerable<TKey> keys, CancellationToken token = default);

        /// <summary>
        /// Gets the value based on filter
        /// </summary>
        ValueTask<T?> GetFirstValueAsync(Expression<Func<T, bool>> filterExpression, Func<T, bool> filter, CancellationToken token = default);

        /// <summary>
        /// Forces cache data to update
        /// </summary>
        ValueTask ForceUpdateAsync(CancellationToken token);

        #endregion
    }
}
