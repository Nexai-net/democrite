// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Runtime;
    using Orleans.Storage;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry that record all the actions in a <see cref="IMemoryStorageGrain"/>.
    /// The registry could provide read value on all the storage entity dedicated to a specific key configuration.
    /// 
    /// The resitry doesn't store data itself to prevent duplicate memory growth.
    /// 
    /// Attention: the grain could be deployed on any silo that doesn't have specifically the store type declared
    /// that why the value store and transmit are only in serialize binary format
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="IMemoryStorageRegistryGrain" />
    internal abstract class MemoryStorageRegistryBaseGrain<TKey, TDataStored> : Grain, IMemoryStorageRegistryGrain<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        #region Fields

        private readonly Dictionary<string, Dictionary<TKey, MemoryStorageInfo>> _memoryStorageInfo;

        private readonly ILogger<IMemoryStorageRegistryGrain<TKey>> _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageRegistryBaseGrain"/> class.
        /// </summary>
        public MemoryStorageRegistryBaseGrain(IGrainFactory grainFactory,
                                              ILogger<IMemoryStorageRegistryGrain<TKey>> logger)
        {
            this._memoryStorageInfo = new Dictionary<string, Dictionary<TKey, MemoryStorageInfo>>();
            this.RegisterGrainFactory = grainFactory;
            this._logger = logger ?? NullLogger<IMemoryStorageRegistryGrain<TKey>>.Instance;
        }

        #endregion

        #region Nested

        /// <summary>
        /// Internal info strorage
        /// </summary>
        protected sealed class MemoryStorageInfo
        {
            [NotNull]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public AbstractType StoredType { get; set; }

            [NotNull]
            public TKey FullKey { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

            public IReadOnlySet<AbstractType>? ParentType { get; set; }

            public GrainId? Source { get; init; }

            public GrainId Storage { get; set; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the register grain factory.
        /// </summary>
        public IGrainFactory RegisterGrainFactory { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task ReportActionAsync(StoreActionEnum storeAction,
                                      string stateName,
                                      TKey fullkey,
                                      AbstractType? type,
                                      IReadOnlyCollection<AbstractType>? parentTypes,
                                      GrainId? source,
                                      GrainId? storageGrain)
        {
            switch (storeAction)
            {
                case StoreActionEnum.Clear:

                    if (this._memoryStorageInfo.TryGetValue(stateName, out var kvDictionary))
                        kvDictionary.Remove(fullkey);

                    break;

                case StoreActionEnum.Read:
                case StoreActionEnum.Write:
                default:
                    MemoryStorageInfo? memoryStorageInfo = null;
                    Dictionary<TKey, MemoryStorageInfo>? dataStored = null;

                    if (!this._memoryStorageInfo.TryGetValue(stateName, out dataStored))
                    {
                        dataStored = new Dictionary<TKey, MemoryStorageInfo>();
                        this._memoryStorageInfo.Add(stateName, dataStored);
                    }

                    if (dataStored.TryGetValue(fullkey, out memoryStorageInfo) == false)
                    {
                        memoryStorageInfo = new MemoryStorageInfo()
                        {
                            Source = source,
                            FullKey = fullkey
                        };
                        dataStored.Add(fullkey, memoryStorageInfo);
                    }

                    memoryStorageInfo.Storage = storageGrain ?? throw new InvalidOperationException("Storage Grain must be reported for future request");
                    memoryStorageInfo.StoredType = type ?? throw new InvalidOperationException("Stored object type must be reported for future request");
                    memoryStorageInfo.ParentType = parentTypes?.ToHashSet() ?? throw new InvalidOperationException("Stored object parent Types must be reported for future request");
                    break;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreDataAsync(string? stateName, [NotNull] AbstractType entityAbstract, GrainCancellationToken token)
        {
            return GetAllStoreByFilderDataAsync(stateName, kv => kv.StoredType == entityAbstract || (kv.ParentType?.Contains(entityAbstract) ?? false), token);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByKeysDataAsync([AllowNull] string? stateName, IReadOnlyCollection<TKey> fullkeys, GrainCancellationToken token)
        {
            return GetAllStoreByFilderDataAsync(stateName, kv => fullkeys.Contains(kv.FullKey), token);
        }

        #region Tools

        /// <summary>
        /// Gets all store by filder data.
        /// </summary>
        private async Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByFilderDataAsync([AllowNull] string? stateName,
                                                                                                   Func<MemoryStorageInfo, bool> filter,
                                                                                                   GrainCancellationToken token)
        {
            var data = this._memoryStorageInfo.Values.SelectMany(v => v.Values);

            if (!string.IsNullOrEmpty(stateName))
            {
                if (this._memoryStorageInfo.TryGetValue(stateName, out var specializedValues))
                    data = specializedValues.Values;
                else // If state filter is not founded then no result
                    return EnumerableHelper<ReadOnlyMemory<byte>>.ReadOnly;
            }

            var correspondingStoredDataTask = data.Where(filter)
                                                  .Select(kv => RequestStoredDataAsync(kv.FullKey, kv))
                                                  .ToArray();

            try
            {
                await correspondingStoredDataTask.SafeWhenAllAsync(token.CancellationToken);
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Critical, "[MemoryRegistry:{storageName}] {exception}", this.GetPrimaryKeyString(), ex);
            }

            token.CancellationToken.ThrowIfCancellationRequested();

            return correspondingStoredDataTask.Where(t => t.IsCompletedSuccessfully && t.Result != null)
                                              .Select(t => GetEntityBytes(t.Result!))
                                              .ToArray();
        }

        /// <summary>
        /// Requests the stored data
        /// </summary>
        private async Task<TDataStored?> RequestStoredDataAsync(TKey key, MemoryStorageInfo info)
        {
            try
            {
                return await OnRequestStoredDataAsync(key, info);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Critical, "[MemoryRegistry:{storageName}] [Key:{key}] {exception}", this.GetPrimaryKeyString(), key, ex);
            }

            return default;
        }

        /// <summary>
        /// <typeparamref name="TDataStored"/> from storage info
        /// </summary>
        protected abstract Task<TDataStored?> OnRequestStoredDataAsync(TKey key, MemoryStorageInfo info);

        /// <summary>
        /// Gets the entity bytes from <paramref name="dataStored"/>
        /// </summary>
        protected abstract ReadOnlyMemory<byte> GetEntityBytes(TDataStored dataStored);

        #endregion

        #endregion
    }
}
