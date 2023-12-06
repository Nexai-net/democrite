// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Triggers;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    [DemocriteSystemVGrain]
    public abstract class TriggerBaseHandlerVGrain<TVGrainState, TTriggerDefinition, TVGrainInterface> : VGrainBase<TVGrainState, TVGrainInterface>, ITriggerHandlerVGrain
        where TVGrainState : TriggerState
        where TVGrainInterface : ITriggerHandlerVGrain
        where TTriggerDefinition : TriggerDefinition
    {
        #region Fields

        private static readonly TimeSpan s_inputBuildTimeout = TimeSpan.FromMinutes(1);

        private readonly ISequenceDefinitionProvider _sequenceDefinitionProvider;
        private readonly IInputSourceProviderFactory _inputSourceProviderFactory;
        private readonly ITriggerDefinitionProvider _triggerDefinitionProvider;
        private readonly IDemocriteExecutionHandler _democriteExecutionHandler;
        private readonly ISignalDefinitionProvider _signalDefinitionProvider;
        private readonly ISignalService _signalService;

        private TTriggerDefinition? _triggerDefinition;

        private IInputProvider? _inputProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerBaseHandlerVGrain{TStates, TVGrainInterface}"/> class.
        /// </summary>
        protected TriggerBaseHandlerVGrain(ILogger<TVGrainInterface> logger,
                                          IPersistentState<TVGrainState> persistentState,
                                          ITriggerDefinitionProvider triggerDefinitionProvider,
                                          ISequenceDefinitionProvider sequenceDefinitionProvider,
                                          ISignalDefinitionProvider signalDefinitionProvider,
                                          IDemocriteExecutionHandler democriteExecutionHandler,
                                          IInputSourceProviderFactory inputSourceProviderFactory,
                                          ISignalService signalService)
            : base(logger, persistentState)
        {
            this._triggerDefinitionProvider = triggerDefinitionProvider;
            this._sequenceDefinitionProvider = sequenceDefinitionProvider;
            this._signalDefinitionProvider = signalDefinitionProvider;
            this._democriteExecutionHandler = democriteExecutionHandler;
            this._inputSourceProviderFactory = inputSourceProviderFactory;
            this._signalService = signalService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the trigger definition.
        /// </summary>
        public TTriggerDefinition TriggerDefinition
        {
            get { return this._triggerDefinition ?? throw new NullReferenceException("Consume the definition is not allow before activation or update"); }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual async Task UpdateAsync()
        {
            await EnsureTriggerDefinitionAsync();
        }

        /// <inheritdoc />
        public sealed override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await EnsureTriggerDefinitionAsync();
        }

        /// <summary>
        /// Fires target sequence and signal
        /// </summary>
        protected async Task FireTriggerAsync(object? defaultInput = null)
        {
            await EnsureTriggerDefinitionAsync();

            var timeoutToken = CancellationHelper.Timeout(s_inputBuildTimeout);
            var definition = this.TriggerDefinition;

            var input = await GetNextInputAsync(definition, timeoutToken, defaultInput);

            // Fire signals
            if (definition.TargetSignalIds != null && definition.TargetSignalIds.Any())
            {
                var targetSignals = await this._signalDefinitionProvider.GetValuesAsync(definition.TargetSignalIds.Select(t => t.Uid).ToArray());

                if (input is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                        await FireSignalsTargets(targetSignals, item);
                }
                else
                {
                    await FireSignalsTargets(targetSignals, input);
                }
            }

            // Fire sequences
            if (definition.TargetSequenceIds != null && definition.TargetSequenceIds.Any())
            {
                var targetSequences = await this._sequenceDefinitionProvider.GetValuesAsync(definition.TargetSequenceIds);

                if (input is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                        await FireSequencesTargets(targetSequences, item);
                }
                else
                {
                    await FireSequencesTargets(targetSequences, input);
                }
            }
        }

        /// <summary>
        /// Fetch the next input bases on setup if needed
        /// </summary>
        private async Task<object?> GetNextInputAsync(TTriggerDefinition definition,
                                                      CancellationToken timeoutToken,
                                                      object? defaultInput = null)
        {
            if (definition.InputSourceDefinition != null)
            {
                object? input = null;

                bool needBuildInputProvider = this._inputProvider == null ||
                                              await this._inputSourceProviderFactory.IsStillValidAsync(this._inputProvider,
                                                                                                       definition.InputSourceDefinition,
                                                                                                       timeoutToken);

                var old = this._inputProvider;

                if (needBuildInputProvider)
                {
                    this._inputProvider = this._inputSourceProviderFactory.GetProvider(definition.InputSourceDefinition);

                    await this._inputProvider.RestoreStateAsync(this.State!.InputProviderState);

                    if (old is IAsyncDisposable asyncDisposable)
                        await asyncDisposable.DisposeAsync();
                    else if (old is IDisposable disposable)
                        disposable.Dispose();
                }

                input = await this._inputProvider!.GetNextAsync(timeoutToken);
                this.State!.InputProviderState = this._inputProvider.GetState();

                return input;
            }

            return defaultInput;

        }

        /// <summary>
        /// Fires the sequences targets.
        /// </summary>
        private async Task FireSequencesTargets(IReadOnlyCollection<SequenceDefinition> targetSequences, object? input)
        {
            foreach (var target in targetSequences)
            {
                // Fire will not wait the end of the execution
                await this._democriteExecutionHandler.SequenceWithInput(target.Uid)
                                                     .SetInput(input)
                                                     .Fire();
            }
        }

        /// <summary>
        /// Fires the signals targets.
        /// </summary>
        /// <remarks>
        ///     Get the definition to ensure the register signal still exists
        /// </remarks>
        private async Task FireSignalsTargets(IReadOnlyCollection<SignalDefinition> targetSignals,
                                              object? input)
        {
            foreach (var target in targetSignals)
            {
                await this._signalService.Fire(target.SignalId, input, null);
            }
        }

        #region Tools

        /// <summary>
        /// Use <see cref="ISignalDefinitionProvider"/> to get <see cref="ICronTriggerHandlerVGrain"/>
        /// </summary>
        protected async Task EnsureTriggerDefinitionAsync()
        {
            var triggerDefinitionId = this.GetPrimaryKey();

            if (this._triggerDefinition != null && this._triggerDefinition.Uid == triggerDefinitionId)
                return;

            if (triggerDefinitionId == Guid.Empty)
                throw new InvalidVGrainIdException(GetGrainId(), "Trigger definition id");

            var definition = (await this._triggerDefinitionProvider.GetFirstValueByIdAsync(triggerDefinitionId)) as TTriggerDefinition;

#pragma warning disable IDE0270 // Use coalesce expression
            if (definition == null)
                throw new MissingDefinitionException(typeof(TTriggerDefinition), triggerDefinitionId);
#pragma warning restore IDE0270 // Use coalesce expression

            this._triggerDefinition = definition;
        }

        #endregion

        #endregion
    }
}
