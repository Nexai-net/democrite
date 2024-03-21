// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Triggers;
    using Democrite.Framework.Node.Triggers;

    using Elvex.Toolbox.Abstractions.Patterns.Workers;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Patterns.Workers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Streams;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handler of trigger based on stream information
    /// </summary>
    /// <seealso cref="ITriggerHandlerVGrain" />
    /// <seealso cref="IGrainWithGuidCompoundKey" />
    [VGrainIdTypeValidator<IGrainWithGuidKey>()]
    internal sealed class StreamTriggerHandlerVGrain : TriggerBaseHandlerVGrain<StreamTriggerState, StreamTriggerDefinition, IStreamTriggerHandlerVGrain>, IStreamTriggerHandlerVGrain, IGrainWithGuidCompoundKey
    {
        #region Fields

        private readonly IStreamQueueDefinitionProvider _streamQueueDefinitionProvider;

        private readonly IWorkerTaskSchedulerProvider _taskSchedulerProvider;
        //private IWorkerTaskScheduler? _taskScheduler;

        //private readonly Queue<ProcessTicket> s_ticketBank;
        //private readonly SemaphoreSlim _tickerLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTriggerHandlerVGrain"/> class.
        /// </summary>
        public StreamTriggerHandlerVGrain(ILogger<IStreamTriggerHandlerVGrain> logger,
                                          [PersistentState(nameof(Triggers), DemocriteConstants.DefaultDemocriteStateConfigurationKey)] IPersistentState<StreamTriggerState> persistentState,
                                          ITriggerDefinitionProvider triggerDefinitionProvider,
                                          ISequenceDefinitionProvider sequenceDefinitionProvider,
                                          ISignalDefinitionProvider signalDefinitionProvider,
                                          IDemocriteExecutionHandler democriteExecutionHandler,
                                          IDataSourceProviderFactory inputSourceProviderFactory,
                                          IStreamQueueDefinitionProvider streamQueueDefinitionProvider,
                                          ISignalService signalService,
                                          IWorkerTaskSchedulerProvider taskSchedulerProvider)
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
            this._streamQueueDefinitionProvider = streamQueueDefinitionProvider;
            streamQueueDefinitionProvider.DataChanged += StreamQueueDefinitionProvider_DataChanged;
            this._taskSchedulerProvider = taskSchedulerProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ensures the stream subscription asynchronous.
        /// </summary>
        protected override async Task OnEnsureTriggerDefinitionAsync(CancellationToken token)
        {
            var grainId = GetGrainId();

            var streamInfo = await this._streamQueueDefinitionProvider.GetFirstValueByIdAsync(this.TriggerDefinition.StreamSourceDefinitionUid, token);

#pragma warning disable IDE0270 // Use coalesce expression
            if (streamInfo is null)
                throw new MissingDefinitionException(typeof(StreamQueueDefinition), this.TriggerDefinition.StreamSourceDefinitionUid.ToString());
#pragma warning restore IDE0270 // Use coalesce expression

            var streamId = streamInfo.ToStreamId();

            if (this.State?.StreamSubscription is not null && this.State?.StreamSubscription.StreamId == streamId)
            {
                this.State!.StreamSubscription = await this.State!.StreamSubscription.ResumeAsync(onNextAsync: ReceiveStreamData, onErrorAsync: ReceiveStreamError);
                await PushStateAsync(default);
                return;
            }

            // Some value change then unsubscrible previous one and connect to new one
            await (this.State?.StreamSubscription?.UnsubscribeAsync() ?? Task.CompletedTask);

            if (this.Enabled == false)
                return;

            var streamProvider = GrainStreamingExtensions.GetStreamProvider(this, streamInfo.StreamConfiguration);
            var stream = streamProvider.GetStream<object>(streamInfo.ToStreamId());

            this.State!.StreamSubscription = await stream.SubscribeAsync(onNextAsync: ReceiveStreamData, onErrorAsync: ReceiveStreamError);
            await PushStateAsync(default);
        }

        private readonly TicketProcessPipeline _pipelineProcess = new TicketProcessPipeline();

        /// <summary>
        /// Receives the stream data.
        /// </summary>
        private async Task ReceiveStreamData(object item, StreamSequenceToken streamSequenceToken)
        {
            if (this.Enabled == false)
            {
                if (this.State?.StreamSubscription is not null)
                {
                    await this.State.StreamSubscription.UnsubscribeAsync();
                    this.State.StreamSubscription = null;

                    await PushStateAsync(default);
                }
                return;
            }

            await _pipelineProcess.ProcessAsync(async () => await FireTriggerAsync(item,
                                                                                   true,
                                                                                   this.VGrainLifecycleToken),
                                                () => item,
                                                this.TriggerDefinition.MaxConcurrentProcess, this.Logger);
        }

        /// <summary>
        /// Receives the stream error.
        /// </summary>
        private Task ReceiveStreamError(Exception exception)
        {
            this.Logger.OptiLog(LogLevel.Error, "[StreamTrigger: {streamTriggerId}] - stream error {exception}", this.TriggerDefinition?.Uid, exception);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Raised when stream queue definition changed
        /// </summary>
        private void StreamQueueDefinitionProvider_DataChanged(object? sender, IReadOnlyCollection<Guid> e)
        {
            Task.Run(UpdateAsync).ConfigureAwait(false);
        }

        #endregion
    }
}
