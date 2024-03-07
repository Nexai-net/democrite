// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Enums;

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

        private readonly IMemoryStorageRegistryGrain<string> _registryGrain;

        private readonly static Func<MemoryGrainStorage, string, IMemoryStorageGrain> s_getStorageGrain;
        private readonly static Func<string, GrainId, string> s_makeKey;

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
        public MemoryGrainTrackStorage(string name,
                                       MemoryGrainStorageOptions options,
                                       ILogger<MemoryGrainStorage> logger,
                                       IGrainFactory grainFactory,
                                       IGrainStorageSerializer defaultGrainStorageSerializer)
            : base(name, options, logger, grainFactory, defaultGrainStorageSerializer)
        {
            this._registryGrain = grainFactory.GetGrain<IMemoryStorageRegistryGrain<string>>(name);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override async Task ClearStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            await base.ClearStateAsync(grainType, grainId, grainState);
            await ReportToRegistryAsync(StoreActionEnum.Clear, grainType, typeof(T), grainId);
        }

        /// <inheritdoc />
        public override async Task ReadStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            await base.ReadStateAsync(grainType, grainId, grainState);
            await ReportToRegistryAsync(StoreActionEnum.Read, grainType, typeof(T), grainId);
        }

        /// <inheritdoc />
        public override async Task WriteStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            await base.WriteStateAsync(grainType, grainId, grainState);
            await ReportToRegistryAsync(StoreActionEnum.Write, grainType, typeof(T), grainId);
        }

        #region Tools

        /// <summary>
        /// Report to registry storage any activity
        /// </summary>
        private async Task ReportToRegistryAsync(StoreActionEnum storeAction, string grainType, Type? requestedEntityType, GrainId sourceGrainId)
        {
            var key = s_makeKey(grainType, sourceGrainId);
            var targetStorageGrain = s_getStorageGrain(this, key);

            await this._registryGrain.ReportActionAsync(storeAction,
                                                        grainType,
                                                        key,
                                                        requestedEntityType?.GetAbstractType(),
                                                        requestedEntityType?.GetAllCompatibleAbstractTypes(),
                                                        sourceGrainId,
                                                        targetStorageGrain?.GetGrainId());
        }

        #endregion

        #endregion
    }
}
