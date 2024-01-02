﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Patterns.Strategy
{
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;
    using Democrite.Framework.Toolbox.Abstractions.Supports;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Supports;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of <see cref="IProviderStrategySource{T, TKey}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="IProviderStrategySource{T, TKey}" />
    public abstract class ProviderStrategyBaseSource<TValue, TKey> : SupportBaseInitialization, IProviderStrategySource<TValue, TKey>
        where TValue : class
        where TKey : notnull
    {
        #region Fields

        private readonly Dictionary<TKey, TValue> _cachedData;
        private readonly ReaderWriterLockSlim _dataCacheLock;

        private readonly HashSet<TKey> _keys;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderStrategyBaseSource{TValue, TKey}"/> class.
        /// </summary>
        protected ProviderStrategyBaseSource(IEnumerable<(TKey key, TValue value)>? initValues = null)
        {
            var readOnlyInitValues = initValues?.Select(kv => new KeyValuePair<TKey, TValue>(kv.key, kv.value))
                                               ?? EnumerableHelper<KeyValuePair<TKey, TValue>>.ReadOnly;

            this._cachedData = new Dictionary<TKey, TValue>(readOnlyInitValues);
            this._dataCacheLock = new ReaderWriterLockSlim();

            this._keys = new HashSet<TKey>();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IReadOnlyCollection<TKey> Keys
        {
            get
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    return this._keys;
                }
                finally
                {
                    this._dataCacheLock?.ExitReadLock();
                }
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<IReadOnlyCollection<TKey>>? DataChanged;

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual ValueTask<(bool Success, TValue? Result)> TryGetDataAsync(TKey key, CancellationToken token)
        {
            this._dataCacheLock.EnterReadLock();
            try
            {
                if (this._cachedData.TryGetValue(key, out var cachedValue))
                    return ValueTask.FromResult<(bool Success, TValue? Result)>((Success: true, Result: cachedValue));
            }
            finally
            {
                this._dataCacheLock.ExitReadLock();
            }

            return ValueTask.FromResult<(bool Success, TValue? Result)>((false, default(TValue)));
        }

        /// <inheritdoc />
        public virtual ValueTask<IReadOnlyCollection<TValue>> GetValuesAsync(Expression<Func<TValue, bool>> filterExpression, Func<TValue, bool> filter, CancellationToken token)
        {
            this._dataCacheLock.EnterReadLock();
            try
            {
                var result = this._cachedData.Where(kv => filter(kv.Value))
                                             .Select(kv => kv.Value)
                                             .ToArray();

                return ValueTask.FromResult<IReadOnlyCollection<TValue>>(result);
            }
            finally
            {
                this._dataCacheLock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public virtual ValueTask<TValue?> GetFirstValueAsync(Expression<Func<TValue, bool>> filterExpression, Func<TValue, bool> filter, CancellationToken token)
        {
            this._dataCacheLock.EnterReadLock();
            try
            {
                var result = this._cachedData.Where(kv => filter(kv.Value))
                                             .Select(kv => kv.Value)
                                             .FirstOrDefault();

                return ValueTask.FromResult(result);
            }
            finally
            {
                this._dataCacheLock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public virtual ValueTask<IReadOnlyCollection<TValue>> GetValuesAsync(IEnumerable<TKey> keys, CancellationToken token)
        {
            this._dataCacheLock.EnterReadLock();
            try
            {
                var results = keys.Distinct()
                                  .Where(k => this._cachedData.ContainsKey(k))
                                  .Select(k => this._cachedData[k])
                                  .ToReadOnly();

                return ValueTask.FromResult(results);
            }
            finally
            {
                this._dataCacheLock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public virtual ValueTask ForceUpdateAsync(CancellationToken token)
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        protected sealed override async ValueTask OnInitializedAsync<TState>(TState? _, CancellationToken token) 
            where TState : default
        {
            await OnInitializedAsync(token);
            await ForceUpdateAsync(token);
        }

        #region Tools

        /// <inheritdoc />
        protected virtual ValueTask OnInitializedAsync(CancellationToken token)
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Thread safes add a key, value
        /// </summary>
        protected void SafeAddOrReplace(TKey key, TValue value)
        {
            SafeAddOrReplace((key, value).AsEnumerable());
        }

        /// <summary>
        /// Thread safes add a key, values
        /// </summary>
        protected void SafeAddOrReplace(IEnumerable<(TKey key, TValue value)> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            this._dataCacheLock.EnterWriteLock();
            try
            {
                foreach (var kv in values)
                {
                    if (this._cachedData.TryAdd(kv.key, kv.value))
                    {
                        this._keys.Add(kv.key);
                    }
                    else
                    {
                        this._cachedData[kv.key] = kv.value;
                    }
                }
            }
            finally
            {
                this._dataCacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Safes the clear all recorded data.
        /// </summary>
        protected void SafeClear()
        {
            ArgumentNullException.ThrowIfNull(this._keys);

            this._dataCacheLock.EnterWriteLock();
            try
            {
                this._cachedData.Clear();
                this._keys.Clear();
            }
            finally
            {
                this._dataCacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Thread safes add a key, value
        /// </summary>
        protected void SafeRemoves(IEnumerable<TKey> keys)
        {
            ArgumentNullException.ThrowIfNull(keys);

            this._dataCacheLock.EnterWriteLock();
            try
            {
                foreach (var key in keys)
                {
                    if (this._cachedData.Remove(key))
                        this._keys.Remove(key);
                }
            }
            finally
            {
                this._dataCacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Raiseds <see cref="DataChanged"/>.
        /// </summary>
        protected void RaisedDataChanged(IReadOnlyCollection<TKey> definitionThatChanged)
        {
            DataChanged?.Invoke(this, definitionThatChanged);
        }

        #endregion

        #endregion
    }
}
