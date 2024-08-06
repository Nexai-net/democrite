// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.Patterns.Strategy;
    using Elvex.Toolbox.Supports;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Definition provider dedicated to <typeparamref name="TDefinition"/> using <see cref="IDynamicDefinitionHandler"/> as sources
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    internal sealed class DynamicDefinitionSourceProvider<TDefinition> : ProviderStrategyBaseSource<TDefinition, Guid>, IProviderStrategySource<TDefinition, Guid>, IDefinitionSourceProvider<TDefinition>, IInitService
        where TDefinition : class, IDefinition
    {
        #region Fields

        private static readonly ConcretType s_definitionConcretType;

        private readonly Dictionary<Guid, DateTime> _indexDefinitionIdUpdates;

        private readonly SupportInitializationImplementation<NoneType> _lazyInitializer;

        private readonly IVGrainDemocriteSystemProvider _vgrainDemocriteSystemProvider;
        private readonly ILogger<DynamicDefinitionSourceProvider<TDefinition>> _logger;
        private readonly ISignalLocalServiceRelay _signalLocalServiceRelay;

        private IDynamicDefinitionHandlerVGrain? _dynamicProviderGrain;
        private IDisposable? _subscritionToken;

        private readonly SemaphoreSlim _updateRegistryLocker;
        private string _registryEtag;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDefinitionSourceProvider{TDefinition}"/> class.
        /// </summary>
        static DynamicDefinitionSourceProvider()
        {
            s_definitionConcretType = (ConcretType)typeof(TDefinition).GetAbstractType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDefinitionSourceProvider{TDefinition}"/> class.
        /// </summary>
        public DynamicDefinitionSourceProvider(IServiceProvider serviceProvider,
                                               IVGrainDemocriteSystemProvider vgrainDemocriteSystemProvider,
                                               ILogger<DynamicDefinitionSourceProvider<TDefinition>> logger,
                                               ISignalLocalServiceRelay signalLocalServiceRelay)
            : base(serviceProvider, supportFallback: true)
        {
            this._indexDefinitionIdUpdates = new Dictionary<Guid, DateTime>();
            this._vgrainDemocriteSystemProvider = vgrainDemocriteSystemProvider;
            this._logger = logger;

            this._updateRegistryLocker = new SemaphoreSlim(1);
            this._registryEtag = string.Empty;

            this._signalLocalServiceRelay = signalLocalServiceRelay;

            this._lazyInitializer = new SupportInitializationImplementation<NoneType>(LayInitMethodAsync);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool ExpectOrleanStarted
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async ValueTask OnProviderInitializedAsync(IServiceProvider? serviceProvider, CancellationToken token)
        {
            this._dynamicProviderGrain = await this._vgrainDemocriteSystemProvider.GetVGrainAsync<IDynamicDefinitionHandlerVGrain>(null, this._logger);

            ArgumentNullException.ThrowIfNull(this._dynamicProviderGrain);

            await base.OnProviderInitializedAsync(serviceProvider, token);
        }

        /// <inheritdoc />
        protected override async ValueTask ForceUpdateAfterInitAsync(CancellationToken token)
        {
            var changes = await AlignFromDynamicDefinitionServiceAsync(token);
            await base.ForceUpdateAfterInitAsync(token);

            if (changes is not null && changes.Any())
                RaisedDataChanged(changes);
        }

        #region Tools

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDefinition>> GetAllValuesAsync(CancellationToken token)
        {
            await this._lazyInitializer.InitializationAsync(token);
            return await base.GetAllValuesAsync(token);
        }

        /// <inheritdoc />
        public override async ValueTask<TDefinition?> GetFirstValueAsync(Expression<Func<TDefinition, bool>> filterExpression, Func<TDefinition, bool> filter, CancellationToken token)
        {
            await this._lazyInitializer.InitializationAsync(token);
            return await base.GetFirstValueAsync(filterExpression, filter, token);
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDefinition>> GetValuesAsync(Expression<Func<TDefinition, bool>> filterExpression, Func<TDefinition, bool> filter, CancellationToken token)
        {
            await this._lazyInitializer.InitializationAsync(token);
            return await base.GetValuesAsync(filterExpression, filter, token);
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDefinition>> GetValuesAsync(IEnumerable<Guid> keys, CancellationToken token)
        {
            await this._lazyInitializer.InitializationAsync(token);
            return await base.GetValuesAsync(keys, token);
        }

        /// <inheritdoc />
        public override async ValueTask<(bool Success, TDefinition? Result)> TryGetDataAsync(Guid key, CancellationToken token)
        {
            await this._lazyInitializer.InitializationAsync(token);
            return await base.TryGetDataAsync(key, token);
        }

        /// <inheritdoc />
        private async ValueTask LayInitMethodAsync(NoneType? type, CancellationToken token)
        {
            // Opti : Use sub signal to refresh more precesily
            this._subscritionToken = await this._signalLocalServiceRelay.SubscribeAsync(OnDynamicDefinitionDataUpdate, DemocriteSystemDefinitions.Signals.DynamicDefinitionChanged);
        }

        /// <summary>
        /// Aligns from dynamic definition service asynchronous.
        /// </summary>
        private async Task<IReadOnlyCollection<Guid>?> AlignFromDynamicDefinitionServiceAsync(CancellationToken token)
        {
            Debug.Assert(this._dynamicProviderGrain is not null);

            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                await this._updateRegistryLocker.WaitAsync(token);
                try
                {
                    var etag = await this._dynamicProviderGrain.GetHandlerEtagAsync();
                    if (string.Equals(etag, this._registryEtag))
                        return EnumerableHelper<Guid>.ReadOnly;

                    var metadata = await this._dynamicProviderGrain!.GetDynamicDefinitionMetaDatasAsync(typeFilter: s_definitionConcretType, null, onlyEnabled: true, grainCancelToken.Token);

                    var existing = this.Keys.ToHashSet();
                    var toAdd = new HashSet<Guid>();
                    var toRemove = new HashSet<Guid>(this.Keys);

                    lock (this._indexDefinitionIdUpdates)
                    {
                        foreach (var meta in metadata.Info)
                        {
                            toRemove.Remove(meta.Uid);

                            if (existing.Contains(meta.Uid))
                            {
                                if (!this._indexDefinitionIdUpdates.TryGetValue(meta.Uid, out var lastUpdate) && lastUpdate >= meta.UTCLastUpdate)
                                {
                                    // Force an update
                                    toAdd.Add(meta.Uid);
                                }
                                continue;
                            }
                            else
                            {
                                toAdd.Add(meta.Uid);
                            }
                        }
                    }

                    if (toRemove.Any())
                    {
                        lock (this._indexDefinitionIdUpdates)
                        {
                            foreach (var rm in toRemove)
                                this._indexDefinitionIdUpdates.Remove(rm);
                        }

                        base.SafeRemoves(toRemove);
                    }

                    if (toAdd.Any())
                    {
                        var definitions = await this._dynamicProviderGrain.GetDefinitionAsync<TDefinition>(grainCancelToken.Token, toAdd);

                        if (definitions.Info.Any())
                        {
                            base.SafeAddOrReplace(definitions.Info.Select(d => (d.Uid, d)));

                            lock (this._indexDefinitionIdUpdates)
                            {
                                foreach (var meta in metadata.Info)
                                    this._indexDefinitionIdUpdates[meta.Uid] = meta.UTCLastUpdate;
                            }
                        }
                    }

                    this._registryEtag = metadata.Etag;
                    return (toRemove ?? EnumerableHelper<Guid>.ReadOnly).Concat(toAdd ?? EnumerableHelper<Guid>.ReadOnly).Distinct().ToArray();
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Error, "[{definition}] Align definition from dynamic {exception}", typeof(TDefinition), ex);
                    throw;
                }
                finally
                {
                    this._updateRegistryLocker.Release();
                }
            }
        }

        /// <summary>
        /// Called when [dynamic definition data update].
        /// </summary>
        private async ValueTask OnDynamicDefinitionDataUpdate(SignalMessage _)
        {
            // Optim: If delete then only check if the current provider have the value instead of by alignments
            var changes = await AlignFromDynamicDefinitionServiceAsync(default);

            if (changes is not null && changes.Any())
                RaisedDataChanged(changes);
        }

        /// <inheritdoc />
        protected override async Task FallbackOdRetryFailedAsync(CancellationToken token)
        {
            await OnDynamicDefinitionDataUpdate(null!);
            await base.FallbackOdRetryFailedAsync(token);
        }

        /// <summary>
        /// Disposes local data
        /// </summary>
        protected override void DisposeBegin()
        {
            this._subscritionToken?.Dispose();
            base.DisposeBegin();
        }

        #endregion

        #endregion
    }
}
