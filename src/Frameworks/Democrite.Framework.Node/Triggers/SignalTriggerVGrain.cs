// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="ISignalTriggerVGrain" />
    [DemocriteSystemVGrain]
    internal sealed class SignalTriggerVGrain : TriggerBaseHandlerVGrain<TriggerState, SignalTriggerDefinition, ISignalTriggerVGrain>, ISignalTriggerVGrain, ISignalReceiver, ISignalReceiverReadOnly
    {
        #region Fields

        private readonly ISignalService _localSignalService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerVGrain"/> class.
        /// </summary>
        public SignalTriggerVGrain(ILogger<ISignalTriggerVGrain> logger,
                                   [PersistentState(nameof(Triggers), DemocriteConstants.DefaultDemocriteStateConfigurationKey)] IPersistentState<TriggerState> persistentState,
                                   ITriggerDefinitionProvider triggerDefinitionProvider,
                                   ISequenceDefinitionProvider sequenceDefinitionProvider,
                                   ISignalDefinitionProvider signalDefinitionProvider,
                                   IDemocriteExecutionHandler democriteExecutionHandler,
                                   IDataSourceProviderFactory inputSourceProviderFactory,
                                   IStreamQueueDefinitionProvider streamQueueDefinitionProvider,
                                   ISignalService signalService)
            : base(logger,
                   persistentState,
                   triggerDefinitionProvider,
                   sequenceDefinitionProvider,
                   signalDefinitionProvider,
                   democriteExecutionHandler,
                   inputSourceProviderFactory,
                   streamQueueDefinitionProvider,
                   signalService)
        {
            this._localSignalService = signalService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task OnEnsureTriggerDefinitionAsync(CancellationToken token)
        {
            var definition = this.TriggerDefinition;

            if (definition.ListenSignal is not null)
                await this._localSignalService.SubscribeAsync(definition.ListenSignal.Value, this, token);
            
            if (definition.ListenDoor is not null)
                await this._localSignalService.SubscribeAsync(definition.ListenDoor.Value, this, token);
        }

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public Task ReceiveSignalAsync(SignalMessage signal)
        {
            if (this.Enabled == false)
                return Task.CompletedTask;

            return base.FireTriggerAsync(signal);
        }

        #endregion
    }
}
