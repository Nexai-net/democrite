// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy
{
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Define a provider through strategy pattern to give access to <typeparamref name="T"/> using <see cref="IProviderStrategySource{T, TKey}"/>
    /// </summary>
    public interface IProviderStrategy<T, TKey>
        where T : class
    {
        #region Events

        /// <summary>
        /// Occurs when source have been update.
        /// </summary>
        event EventHandler<IReadOnlyCollection<TKey>>? DataChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Gets all values corresponding to <paramref name="keys"/>
        /// </summary>
        ValueTask<IReadOnlyCollection<T>> GetValuesAsync(params TKey[] keys);

        /// <summary>
        /// Gets all values corresponding to <paramref name="keys"/>
        /// </summary>
        ValueTask<IReadOnlyCollection<T>> GetValuesAsync(IReadOnlyCollection<TKey> keys);

        /// <summary>
        /// Gets all values corresponding match conditions
        /// </summary>
        ValueTask<IReadOnlyCollection<T>> GetValuesAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Gets first value to correspond match conditions
        /// </summary>
        ValueTask<T?> GetFirstValueAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Gets first value to correspond to <paramref name="key"/>
        /// </summary>
        ValueTask<T?> GetFirstValueByIdAsync(TKey key);

        /// <summary>
        /// Try get first value to correspond to <paramref name="key"/>
        /// </summary>
        ValueTask<(bool Result, T? value)> TryGetFirstValueAsync(TKey key);

        /// <summary>
        /// Forces cache data to update
        /// </summary>
        ValueTask ForceUpdateAsync();

        #endregion
    }
}
