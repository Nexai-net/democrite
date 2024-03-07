// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Extensions;

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

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalHandlerVGrain"/> class.
        /// </summary>
        public SignalHandlerVGrain(ILogger<SignalHandlerVGrain> logger,
                                   [PersistentState("Signals", nameof(Democrite))] IPersistentState<SignalHandlerStateSurrogate> persistentState,
                                   ISignalDefinitionProvider signalDefinitionProvider,
                                   IGrainFactory grainFactory,
                                   ITimeManager timeManager)
            : base(logger, persistentState, grainFactory)
        {
            this._timeManager = timeManager;
            this._signalDefinitionProvider = signalDefinitionProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public async Task<Guid> Fire(GrainId? sourceId, VGrainMetaData? sourceMetaData, GrainCancellationToken token)
        {
            await EnsureInitializedAsync(token.CancellationToken);

            var fireId = Guid.NewGuid();

            var now = this._timeManager.UtcNow;
            var signalSource = new SignalSource(fireId,
                                                this._signalDefinition!.SignalId.Uid,
                                                this._signalDefinition!.SignalId.Name,
                                                false,
                                                now,
                                                sourceId,
                                                sourceMetaData);

            return await Fire(signalSource);
        }

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public async Task<Guid> Fire<TData>(GrainId? sourceId, TData data, VGrainMetaData? sourceMetaData, GrainCancellationToken token) 
            where TData : struct
        {
            await EnsureInitializedAsync(token.CancellationToken);

            var fireId = Guid.NewGuid();
            var now = this._timeManager.UtcNow;

            var signalSource = new SignalSource<TData>(fireId,
                                                       this._signalDefinition!.SignalId.Uid,
                                                       this._signalDefinition!.SignalId.Name,
                                                       false,
                                                       now,
                                                       sourceId,
                                                       sourceMetaData,
                                                       data);

            return await Fire(signalSource);
        }

        #region Tools

        /// <summary>
        /// Ensures this instance is initialized.
        /// </summary>
        protected override async ValueTask OnEnsureInitializedAsync(CancellationToken token)
        {
            if (this._signalDefinition != null)
                return;

            var signalDefinitionId = this.GetPrimaryKey();

            if (signalDefinitionId == Guid.Empty)
                throw new InvalidVGrainIdException(GetGrainId(), "signal definition id");

            var signalDefinitions = (await this._signalDefinitionProvider.GetValuesAsync(token, signalDefinitionId)).Distinct().ToArray();

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
        }

        /// <inheritdoc cref="ISignalVGrain.Fire"/>
        private async Task<Guid> Fire(SignalSource signalSource)
        {
            var signal = new SignalMessage(signalSource.SignalUid,
                                           signalSource.SendUtcTime,
                                           signalSource);

            return await FireSignal(signal);
        }

        #endregion

        #endregion
    }
}
