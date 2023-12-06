// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="ISignalTriggerVGrain" />
    [DemocriteSystemVGrain]
    internal sealed class SignalTriggerVGrain : TriggerBaseHandlerVGrain<TriggerState, SignalTriggerDefinition, ISignalTriggerVGrain>, ISignalTriggerVGrain, ISignalReceiver
    {
        #region Fields

        private readonly ISignalService _localSignalService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerVGrain"/> class.
        /// </summary>
        public SignalTriggerVGrain(ILogger<ISignalTriggerVGrain> logger,
                                   [PersistentState(nameof(Triggers), nameof(Democrite))] IPersistentState<TriggerState> persistentState,
                                   ITriggerDefinitionProvider triggerDefinitionProvider,
                                   ISequenceDefinitionProvider sequenceDefinitionProvider,
                                   ISignalDefinitionProvider signalDefinitionProvider,
                                   IDemocriteExecutionHandler democriteExecutionHandler,
                                   IInputSourceProviderFactory inputSourceProviderFactory,
                                   ISignalService signalService)
            : base(logger,
                   persistentState,
                   triggerDefinitionProvider,
                   sequenceDefinitionProvider,
                   signalDefinitionProvider,
                   democriteExecutionHandler,
                   inputSourceProviderFactory,
                   signalService)
        {
            this._localSignalService = signalService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override async Task UpdateAsync()
        {
            await base.UpdateAsync();

            var definition = this.TriggerDefinition;

            if (definition.ListenSignal != null)
                await this._localSignalService.SubscribeAsync(definition.ListenSignal.Value, this);
            else if (definition.ListenDoor != null)
                await this._localSignalService.SubscribeAsync(definition.ListenDoor.Value, this);
        }

        /// <inheritdoc />
        public Task ReceiveSignalAsync(SignalMessage signal)
        {
            return base.FireTriggerAsync(signal);
        }

        #endregion
    }
}
