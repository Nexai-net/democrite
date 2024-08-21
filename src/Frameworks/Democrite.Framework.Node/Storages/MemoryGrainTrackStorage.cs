// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Configuration;
    using Orleans.Runtime;
    using Orleans.Storage;

    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Memory storage using base <see cref="MemoryGrainStorage"/> to add record to easy repository service (look through all data)
    /// </summary>
    /// <seealso cref="MemoryGrainStorage" />
    internal sealed class MemoryGrainTrackStorage : MemoryGrainStorage
    {
        #region Fields

        private readonly static Func<MemoryGrainStorage, string, IMemoryStorageGrain> s_getStorageGrain;
        private readonly static Func<string, GrainId, string> s_makeKey;

        private readonly ReaderWriterLockSlim _locker;

        private readonly Dictionary<string, object?> _keyNotifiedExtraKey;
        private readonly HashSet<string> _keyNotified;

        private readonly IGrainFactory _grainFactory;
        private readonly string _storageConfig;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize the class <see cref="MemoryGrainTrackStorage"/>
        /// </summary>
        static MemoryGrainTrackStorage()
        {
            var makeMthd = typeof(MemoryGrainStorage).GetMethod("MakeKey", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(makeMthd != null);

            s_makeKey = (type, id) => (string)makeMthd.Invoke(null, new object[] { type, id })!;

            var getStorageGrainMth = typeof(MemoryGrainStorage).GetMethod("GetStorageGrain", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(getStorageGrainMth != null);

            s_getStorageGrain = (inst, key) => (IMemoryStorageGrain)getStorageGrainMth.Invoke(inst, new[] { key })!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryGrainTrackStorage"/> class.
        /// </summary>
        public MemoryGrainTrackStorage(string storageConfig,
                                       MemoryGrainStorageOptions options,
                                       ILogger<MemoryGrainStorage> logger,
                                       IGrainFactory grainFactory,
                                       IGrainStorageSerializer defaultGrainStorageSerializer)
            : base(storageConfig, options, logger, grainFactory, defaultGrainStorageSerializer)
        {
            this._locker = new ReaderWriterLockSlim();
            this._keyNotifiedExtraKey = new Dictionary<string, object?>();
            this._keyNotified = new HashSet<string>();
            this._grainFactory = grainFactory;
            this._storageConfig = storageConfig;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override async Task ClearStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            await base.ClearStateAsync(grainType, grainId, grainState);
            await ReportToRegistryAsync(StoreActionEnum.Clear, grainType, typeof(T), grainId, grainState.State);
        }

        /// <inheritdoc />
        public override async Task ReadStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            await base.ReadStateAsync(grainType, grainId, grainState);
            await ReportToRegistryAsync(StoreActionEnum.Read, grainType, typeof(T), grainId, grainState.State);
        }

        /// <inheritdoc />
        public override async Task WriteStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            await base.WriteStateAsync(grainType, grainId, grainState);
            await ReportToRegistryAsync(StoreActionEnum.Write, grainType, typeof(T), grainId, grainState.State);
        }

        #region Tools

        /// <summary>
        /// Report to registry storage any activity
        /// </summary>
        private async Task ReportToRegistryAsync(StoreActionEnum storeAction,
                                                 string stateName,
                                                 Type? requestedEntityType,
                                                 GrainId sourceGrainId,
                                                 object? entity)
        {
            var key = s_makeKey(stateName, sourceGrainId);
            var targetStorageGrain = s_getStorageGrain(this, key);

            this._locker.EnterReadLock();
            try
            {
                if (storeAction != StoreActionEnum.Clear && this._keyNotified.Contains(key) && this._keyNotifiedExtraKey.TryGetValue(key, out var objEntityKey) && object.Equals(entity, objEntityKey))
                    return;
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            this._locker.EnterWriteLock();
            try
            {
                if (storeAction != StoreActionEnum.Clear && this._keyNotified.Contains(key) && this._keyNotifiedExtraKey.TryGetValue(key, out var objEntityKey) && object.Equals(entity, objEntityKey))
                    return;

                if (storeAction == StoreActionEnum.Clear)
                {
                    this._keyNotified.Remove(key);
                    this._keyNotifiedExtraKey.Remove(key);
                }
                else
                {
                    this._keyNotified.Add(key);
                    this._keyNotifiedExtraKey[key] = entity;
                }

            }
            finally
            {
                this._locker.ExitWriteLock();
            }
            
            var grainIndx = targetStorageGrain.GetPrimaryKeyLong();

            // IMemoryStorageStateRegistryGrain => GRAIN STATE Registry
            var registry = this._grainFactory.GetGrain<IMemoryStorageStateRegistryGrain<string>>(grainIndx / 10, MemoryStorageRegistryHelper.ComputeRegistryExtName(stateName, this._storageConfig));
            await registry.ReportActionAsync(storeAction,
                                             key,
                                             EntityHelper.GetEntityId(entity),
                                             requestedEntityType?.GetAbstractType(),
                                             requestedEntityType?.GetAllCompatibleAbstractTypes(),
                                             sourceGrainId,
                                             targetStorageGrain?.GetGrainId());
        }

        #endregion

        #endregion
    }
}

