// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Signals
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <inheritdoc />
    internal sealed class SignalService : ISignalService
    {
        #region Fields
        
        private static readonly MethodInfo s_fireImplGeneric;

        private readonly Dictionary<Type, Func<SignalId, object, IVGrainInformationProvider?, Task<Guid>>> _funcCache;
        private readonly ReaderWriterLockSlim _cacheLocker;

        private readonly ISignalDefinitionProvider _signalDefinitionProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SignalService"/> class.
        /// </summary>
        static SignalService()
        {
            var fireImplGeneric = typeof(SignalService).GetMethod(nameof(FireImpl), BindingFlags.NonPublic | BindingFlags.Instance);
            System.Diagnostics.Debug.Assert(fireImplGeneric != null);
            s_fireImplGeneric = fireImplGeneric;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalService"/> class.
        /// </summary>
        public SignalService(ISignalDefinitionProvider signalDefinitionProvider,
                            IGrainFactory grainFactory)
        {
            this._funcCache = new Dictionary<Type, Func<SignalId, object, IVGrainInformationProvider?, Task<Guid>>>();
            this._cacheLocker = new ReaderWriterLockSlim();

            this._signalDefinitionProvider = signalDefinitionProvider;
            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<Guid> Fire(string signalName, IVGrainInformationProvider? vgrainInformationProvider = null)
        {
            var definition = await ResolveSignalDefinitionByNameAsync(signalName);
            return await FireImpl<NoneTypeStruct>(definition.SignalId, NoneTypeStruct.Instance, vgrainInformationProvider);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Guid> Fire(in SignalId signalId, IVGrainInformationProvider? vgrainInformationProvider = null)
        {
            return FireImpl<NoneTypeStruct>(signalId, NoneTypeStruct.Instance, vgrainInformationProvider);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Guid> Fire(string signalName, object? data, IVGrainInformationProvider? vgrainInformationProvider = null)
        {
            var definition = await ResolveSignalDefinitionByNameAsync(signalName);
            return await Fire(signalName, data, vgrainInformationProvider);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Guid> Fire(in SignalId signalId, object? data, IVGrainInformationProvider? vgrainInformationProvider = null)
        {
            if (data is null)
                return FireImpl(signalId, NoneTypeStruct.Instance, vgrainInformationProvider);

            var type = data.GetType();

            if (!type.IsValueType)
                throw new Exception();

            Func<SignalId, object, IVGrainInformationProvider?, Task<Guid>>? builder = null;

            this._cacheLocker.EnterReadLock();
            try
            {
                this._funcCache.TryGetValue(type, out builder);
            }
            finally
            {
                this._cacheLocker.ExitReadLock();
            }

            if (builder == null)
            {
                this._cacheLocker.EnterWriteLock();
                try
                {
                    if (!this._funcCache.TryGetValue(type, out builder))
                    {
                        var paramSignalId = Expression.Parameter(typeof(SignalId), "id");
                        var paramData = Expression.Parameter(typeof(object), "data");
                        var paramProvider = Expression.Parameter(typeof(IVGrainInformationProvider), "provider");

                        var call = Expression.Call(Expression.Constant(this), s_fireImplGeneric.MakeGenericMethod(type), paramSignalId, Expression.Convert(paramData, type), paramProvider);

                        var lambda = Expression.Lambda<Func<SignalId, object, IVGrainInformationProvider?, Task<Guid>>>(call, paramSignalId, paramData, paramProvider);
                        
                        builder = lambda.Compile();
                        this._funcCache.Add(type, builder);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    this._cacheLocker.ExitWriteLock();
                }
            }

            return builder!(signalId, data, vgrainInformationProvider);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Guid> Fire<TData>(string signalName, TData data, IVGrainInformationProvider? vgrainInformationProvider = null)
            where TData : struct
        {
            var definition = await ResolveSignalDefinitionByNameAsync(signalName);
            return await FireImpl<TData>(definition.SignalId, data, vgrainInformationProvider);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Guid> Fire<TData>(in SignalId signalId, in TData data, IVGrainInformationProvider? vgrainInformationProvider = null)
            where TData : struct
        {
            return FireImpl<TData>(signalId, data, vgrainInformationProvider);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<Guid> FireImpl<TData>(in SignalId signalId, in TData data, IVGrainInformationProvider? vgrainInformationProvider = null)
            where TData : struct
        {
            var signalVGrain = this._grainFactory.GetGrain<ISignalVGrain>(signalId.Uid) ?? throw new SignalNotFoundException(signalId.Name);

            if (data is NoneTypeStruct)
                return signalVGrain.Fire(vgrainInformationProvider?.GetGrainId(), vgrainInformationProvider?.MetaData);

            return signalVGrain.Fire<TData>(vgrainInformationProvider?.GetGrainId(), data, vgrainInformationProvider?.MetaData);
        }

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(string signalName, ISignalReceiver receiver)
        {
            var definition = await ResolveSignalDefinitionByNameAsync(signalName);
            return await SubscribeAsync(definition.SignalId, receiver);
        }

        /// <inheritdoc />
        public Task<Guid> SubscribeAsync(in SignalId signalId, ISignalReceiver receiver)
        {
            var signalVGrain = this._grainFactory.GetGrain<ISignalVGrain>(signalId.Uid) ?? throw new SignalNotFoundException(signalId.Name);
            return signalVGrain.SubscribeAsync(receiver.GetGrainId());
        }

        /// <inheritdoc />
        public Task<Guid> SubscribeAsync(in DoorId doorId, ISignalReceiver receiver)
        {
            var signalVGrain = this._grainFactory.GetGrain<IDoorSignalVGrain>(doorId.Uid) ?? throw new DoorNotFoundException(doorId.Uid + "/" + doorId.Name);
            return signalVGrain.SubscribeAsync(receiver.GetGrainId());
        }

        #region Tools

        /// <summary>
        /// Resolves the signal definition by name.
        /// </summary>
        private async Task<SignalDefinition> ResolveSignalDefinitionByNameAsync(string signalName)
        {
            var definition = await this._signalDefinitionProvider.GetFirstValueAsync(p => string.Equals(p.Name, signalName, StringComparison.OrdinalIgnoreCase));

#pragma warning disable IDE0270 // Use coalesce expression
            if (definition == null)
                throw new SignalNotFoundException(signalName);
#pragma warning restore IDE0270 // Use coalesce expression

            return definition;
        }

        #endregion

        #endregion
    }
}
