// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy
{
    using System.Linq.Expressions;

    /// <summary>
    /// Define a source of information consumed by <see cref="IProviderStrategy{T, TKey}"/>
    /// </summary>
    public interface IProviderStrategySource<T, TKey>
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
        event EventHandler DataChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Call to build data <see cref="T"/>
        /// </summary>
        ValueTask BuildAsync();

        /// <summary>
        /// Tries get data <typeparamref name="T"/> from key <paramref name="key"/>
        /// </summary>
        ValueTask<bool> TryGetDataAsync(TKey key, out T? value);

        /// <summary>
        /// Gets the values based on filter
        /// </summary>
        ValueTask<IReadOnlyCollection<T>> GetValuesAsync(Expression<Func<T, bool>> filterExpression, Func<T, bool> filter);

        /// <summary>
        /// Gets the value based on filter
        /// </summary>
        ValueTask<T?> GetFirstValueAsync(Expression<Func<T, bool>> filterExpression, Func<T, bool> filter);

        #endregion
    }
}
