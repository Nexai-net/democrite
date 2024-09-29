// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// Virtual grain used to handled signal fire and subscription
    /// </summary>
    /// <remarks>
    ///     Grain id match the definition id
    /// </remarks>
    /// <seealso cref="ISignalHandlerVGrain" />
    [DemocriteSystemVGrain]
    internal sealed class SignalHandlerVGrain : BaseSignalVGrain<ISignalHandlerVGrain>, ISignalHandlerVGrain
    {
        #region Fields

        private readonly ISignalDefinitionProvider _signalDefinitionProvider;
        private readonly ITimeManager _timeManager;

        private SignalDefinition? _signalDefinition;
        private ISignalHandlerVGrain? _parentSignalHandler;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalHandlerVGrain"/> class.
        /// </summary>
        public SignalHandlerVGrain(ILogger<SignalHandlerVGrain> logger,
                                   [PersistentState("Signals", DemocriteConstants.DefaultDemocriteStateConfigurationKey)] IPersistentState<SignalHandlerStateSurrogate> persistentState,
                                   ISignalDefinitionProvider signalDefinitionProvider,
                                   IGrainOrleanFactory grainFactory,
                                   IRemoteGrainServiceFactory remoteGrainServiceFactory,
                                   ITimeManager timeManager)
            : base(logger, persistentState, grainFactory, remoteGrainServiceFactory)
        {
            this._timeManager = timeManager;
            this._signalDefinitionProvider = signalDefinitionProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public async Task<Guid> Fire(Guid fireId, GrainId? sourceId, VGrainMetaData? sourceMetaData, GrainCancellationToken token)
        {
            if (this.State is not null && this.State.HasSubscriptions == false)
                return fireId;

            await EnsureInitializedAsync(token.CancellationToken);

            var now = this._timeManager.UtcNow;
            var signalSource = new SignalSource(fireId,
                                                this._signalDefinition!.SignalId.Uid,
                                                this._signalDefinition!.SignalId.Name ?? string.Empty,
                                                false,
                                                now,
                                                sourceId,
                                                sourceMetaData);

            await Fire(signalSource);
            await RelayToParent<NoneTypeStruct>(fireId, sourceId, NoneTypeStruct.Instance, sourceMetaData, token);
            return fireId;
        }

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public async Task<Guid> Fire<TData>(Guid fireId, GrainId? sourceId, TData data, VGrainMetaData? sourceMetaData, GrainCancellationToken token)
            where TData : struct
        {
            if (this.State is not null && this.State.HasSubscriptions == false)
                return fireId;

            await EnsureInitializedAsync(token.CancellationToken);

            var now = this._timeManager.UtcNow;

            var signalSource = new SignalSource<TData>(fireId,
                                                       this._signalDefinition!.SignalId.Uid,
                                                       this._signalDefinition!.SignalId.Name ?? string.Empty,
                                                       false,
                                                       now,
                                                       sourceId,
                                                       sourceMetaData,
                                                       data);

            await Fire(signalSource);
            await RelayToParent<TData>(fireId, sourceId, data, sourceMetaData, token);
            return fireId;
        }

        #region Tools

        /// <summary>
        /// Ensures this instance is initialized.
        /// </summary>
        protected override async ValueTask OnEnsureInitializedAsync(CancellationToken token)
        {
            if (this._signalDefinition is not null)
                return;

            var signalDefinitionId = this.GetPrimaryKey();

            if (signalDefinitionId == Guid.Empty)
                throw new InvalidVGrainIdException(GetGrainId(), "signal definition id");

            var signalDefinitions = (await this._signalDefinitionProvider.GetByKeyAsync(token, signalDefinitionId)).Distinct().ToArray();

            if (signalDefinitions.Length > 1)
            {
                this.Logger.OptiLog(LogLevel.Warning,
                                    "Multiple signal with different name have the same UID '{Uid}' : {defintions}",
                                    signalDefinitionId,
                                    signalDefinitions);
            }
            else if (signalDefinitions.Length <= 0)
            {
                throw new SignalNotFoundException(signalDefinitionId.ToString());
            }

            this._signalDefinition = signalDefinitions.First();

            if (this._signalDefinition.ParentSignalId is not null && this._signalDefinition.ParentSignalId.Value.Uid != Guid.Empty)
                this._parentSignalHandler = this.GrainFactory.GetGrain<ISignalHandlerVGrain>(this._signalDefinition.ParentSignalId.Value.Uid);
        }

        /// <inheritdoc cref="ISignalVGrain.Fire"/>
        private async Task<Guid> Fire(SignalSource signalSource)
        {
            var signal = new SignalMessage(signalSource.SignalUid,
                                           signalSource.SendUtcTime,
                                           signalSource);

            return await FireSignal(signal);
        }

        /// <summary>
        /// Relays to parent if needed
        /// </summary>
        private async Task RelayToParent<TData>(Guid fireId, GrainId? sourceId, TData data, VGrainMetaData? sourceMetaData, GrainCancellationToken token)
            where TData : struct
        {
            if (this._parentSignalHandler is not null)
            {
                if (NoneTypeStruct.IsEqualTo<TData>())
                    await this._parentSignalHandler.Fire(fireId, sourceId, sourceMetaData, token);
                else
                    await this._parentSignalHandler.Fire(fireId, sourceId, data, sourceMetaData, token);
            }
        }

        #endregion

        #endregion
    }
}
