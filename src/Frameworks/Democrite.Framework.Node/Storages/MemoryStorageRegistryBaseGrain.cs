// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Concurrency;
    using Orleans.Runtime;
    using Orleans.Storage;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
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
    internal abstract class MemoryStorageRegistryBaseGrain<TKey, TDataStored, TStorageGrain> : Grain, IMemoryStorageRegistryGrain
        where TKey : notnull, IEquatable<TKey>
        where TStorageGrain : IGrain
    {
        #region Fields

        private static readonly Type s_keyTraits;
        private static readonly bool s_isKeyString;

        private readonly Dictionary<object, MemoryStorageInfo> _memoryStorageInfoExtraKeyIndexation;
        private readonly Dictionary<TKey, MemoryStorageInfo> _memoryStorageInfo;

        private readonly IDedicatedObjectConverter _dedicatedObjectConverter;
        private readonly ReaderWriterLockSlim _registryLocker;
        private readonly ILogger<IMemoryStorageRegistryGrain> _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MemoryStorageRegistryBaseGrain{TKey, TDataStored}"/> class.
        /// </summary>
        static MemoryStorageRegistryBaseGrain()
        {
            s_keyTraits = typeof(TKey);
            s_isKeyString = s_keyTraits == typeof(string);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageRegistryBaseGrain"/> class.
        /// </summary>
        public MemoryStorageRegistryBaseGrain(IGrainFactory grainFactory,
                                              ILogger<IMemoryStorageRegistryGrain> logger,
                                              IDedicatedObjectConverter dedicatedObjectConverter)
        {
            this._dedicatedObjectConverter = dedicatedObjectConverter;

            // Locker to access registry in thread safe mode due to ReadOnly And OneWay attribute on the grain method
            // Those attribute are used to optimize the process and prevent deadlock

            this._registryLocker = new ReaderWriterLockSlim();

            this._memoryStorageInfoExtraKeyIndexation = new Dictionary<object, MemoryStorageInfo>();
            this._memoryStorageInfo = new Dictionary<TKey, MemoryStorageInfo>();

            this.RegisterGrainFactory = grainFactory;
            this._logger = logger ?? NullLogger<IMemoryStorageRegistryGrain>.Instance;
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

            public object? EntityKey { get; set; }

            public IReadOnlySet<AbstractType>? ParentType { get; set; }

            public GrainId? Source { get; init; }

            public GrainId Storage { get; set; }

            public TStorageGrain? StorageGrain { get; set; }
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
        [OneWay]
        public Task ReportActionAsync<TReportKey>(StoreActionEnum storeAction,
                                                  TReportKey fullkey,
                                                  object? entityKey,
                                                  AbstractType? type,
                                                  IReadOnlyCollection<AbstractType>? parentTypes,
                                                  GrainId? source,
                                                  GrainId? storageGrain)
            where TReportKey : notnull, IEquatable<TReportKey>
        {
            var key = ConvertKey(fullkey);

            if (key is null)
                return Task.CompletedTask;

            this._registryLocker.EnterWriteLock();
            try
            {
                switch (storeAction)
                {
                    case StoreActionEnum.Clear:

                        this._memoryStorageInfo.Remove(key);

                        if (entityKey is not null)
                            this._memoryStorageInfoExtraKeyIndexation.Remove(entityKey);

                        break;

                    case StoreActionEnum.Read:
                    case StoreActionEnum.Write:
                    default:
                        MemoryStorageInfo? memoryStorageInfo;

                        if (this._memoryStorageInfo.TryGetValue(key, out memoryStorageInfo) == false)
                        {
                            memoryStorageInfo = new MemoryStorageInfo()
                            {
                                Source = source,
                                FullKey = key
                            };
                            this._memoryStorageInfo.Add(key, memoryStorageInfo);
                        }

                        if (storageGrain is null)
                            throw new InvalidOperationException("Storage Grain must be reported for future request");

                        var storageGrainChange = memoryStorageInfo.Storage != storageGrain.Value;

                        memoryStorageInfo.EntityKey = entityKey;

                        if (storageGrainChange)
                            memoryStorageInfo.Storage = storageGrain.Value;

                        memoryStorageInfo.StoredType = type ?? throw new InvalidOperationException("Stored object type must be reported for future request");

                        if (memoryStorageInfo.StorageGrain is null || storageGrainChange)
                            memoryStorageInfo.StorageGrain = this.RegisterGrainFactory.GetGrain<TStorageGrain>(storageGrain.Value);

                        if (memoryStorageInfo.ParentType is null)
                            memoryStorageInfo.ParentType = parentTypes?.ToHashSet() ?? throw new InvalidOperationException("Stored object parent Types must be reported for future request");
                        break;
                }
            }
            finally
            {
                this._registryLocker.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        [ReadOnly]
        public virtual Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreDataAsync([NotNull] AbstractType entityAbstract, GrainCancellationToken token)
        {
            return GetAllStoreByFilderDataAsync(kv => kv.StoredType == entityAbstract || (kv.ParentType?.Contains(entityAbstract) ?? false), token);
        }

        /// <inheritdoc />
        [ReadOnly]
        public virtual Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByKeysDataAsync<TReportKey>(IReadOnlyCollection<TReportKey> fullkeys, GrainCancellationToken token)
            where TReportKey : notnull, IEquatable<TReportKey>
        {
            var hashSetResult = new HashSet<TReportKey>();

            IReadOnlyCollection<TKey>? sourceKeys;

            if (fullkeys is IReadOnlyCollection<TKey> castKeys)
            {
                sourceKeys = castKeys;
            }
            else
            {
                sourceKeys = fullkeys.Select(k => ConvertKey(k))
                                     .NotNull()
                                     .ToArray();
            }

            return GetAllStoreByFilderDataAsync(kv => sourceKeys.Contains(kv.FullKey) || (kv.EntityKey is not null && kv.EntityKey is TReportKey rpKey && fullkeys.Contains(rpKey)), token);
        }

        #region Tools

        /// <inheritdoc />
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this.GetPrimaryKeyLong(out var stateNameAndStorageConfig);

            MemoryStorageRegistryHelper.ExplodeRegistryExtKey(stateNameAndStorageConfig, out var stateName, out var storageConfig);

            // One master by storage config
            var masterGrain = this.GrainFactory.GetGrain<IMemoryStorageRegistryGrainMaster>(storageConfig);

            await masterGrain.RegisterRegistryAsync(this.GetDedicatedGrainId<IMemoryStorageRegistryGrain>(), stateName);
            await base.OnActivateAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all store by filder data.
        /// </summary>
        private async Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByFilderDataAsync(Func<MemoryStorageInfo, bool> filter,
                                                                                                   GrainCancellationToken token)
        {
            IReadOnlyCollection<MemoryStorageInfo> storageInfos;

            this._registryLocker.EnterReadLock();
            try
            {
                // OPTIM : Use immutable array to store data and prevent allocation if read is more often than write
                storageInfos = this._memoryStorageInfo.Values.ToArray();
            }
            finally
            {
                this._registryLocker.ExitReadLock();
            }

            var correspondingStoredDataTask = storageInfos.Where(filter)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TKey? ConvertKey<TReportKey>(TReportKey k) where TReportKey : notnull, IEquatable<TReportKey>
        {
            if (EqualityComparer<TReportKey>.Default.Equals(k, default))
                return default;

            if (k is TKey kkey)
                return kkey;

            if (s_isKeyString)
                return (TKey)(object)k.ToString()!;

            if (this._dedicatedObjectConverter.TryConvert(k, s_keyTraits, out var convertItem))
                return (TKey?)convertItem;

            throw new InvalidCastException("Invalid Key in memory storage : " + k + " instead of " + s_keyTraits);
        }

        #endregion

        #endregion
    }
}
