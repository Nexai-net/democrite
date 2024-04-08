// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Statements;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Door virtual grain that use boolean logic to determin if or not the door activate
    /// </summary>
    /// <seealso cref="DoorBaseVGrain{ILogicalDoorVGrain, LogicalAggregatorDoorDefinition}" />
    /// <seealso cref="ILogicalDoorVGrain" />
    [DemocriteSystemVGrain]
    internal sealed class LogicalDoorVGrain : DoorBaseVGrain<ILogicalDoorVGrain, BooleanLogicalDoorDefinition>, ILogicalDoorVGrain
    {
        #region Fields

        private static readonly TimeSpan s_noRepetitionDelay = TimeSpan.FromDays(1);

        private Dictionary<Guid, int>? _indexedVariables;
        private IBoolLogicStatement? _conditionExpression;
        private IDisposable? _simulateThisTimerToken;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalDoorVGrain"/> class.
        /// </summary>
        public LogicalDoorVGrain(ILogger<ILogicalDoorVGrain> logger,
                                 [PersistentState("Doors", nameof(Democrite))] IPersistentState<DoorHandlerStateSurrogate> persistentState,
                                 ISignalService signalService,
                                 IDoorDefinitionProvider doorDefinitionProvider,
                                 ITimeManager timeHandler,
                                 IGrainOrleanFactory grainOrleanFactory)

            : base(logger, persistentState, signalService, doorDefinitionProvider, timeHandler, grainOrleanFactory)
        {
        }

        #endregion

        #region Methods

        /// </<inheritdoc />
        protected override ValueTask OnInitializeAsync(BooleanLogicalDoorDefinition doorDefinition, CancellationToken token)
        {
            var variables = doorDefinition.VariableNames.Keys.ToList();

            token.ThrowIfCancellationRequested();

            this._indexedVariables = doorDefinition.DoorSourceIds.Select(s => (Uid: s.Uid, Key: doorDefinition.VariableNames.FirstOrDefault(kv => kv.Value == s.Uid).Key))
                                                   .Concat(doorDefinition.SignalSourceIds.Select(s => (Uid: s.Uid, Key: doorDefinition.VariableNames.FirstOrDefault(kv => kv.Value == s.Uid).Key)))
                                                   .Where(kv => kv.Key != null)
                                                   .ToDictionary(kv => kv.Uid, v => variables.IndexOf(v.Key));

            if (doorDefinition.UseCurrentDoorStatus)
            {
                var thisVariable = doorDefinition.VariableNames.FirstOrDefault(vn => vn.Value == doorDefinition.Uid);
                if (thisVariable.Key != null)
                    this._indexedVariables.Add(thisVariable.Value, variables.IndexOf(thisVariable.Key));
            }

            if (variables.Count != this._indexedVariables.Count)
                this.Logger.OptiLog(LogLevel.Critical, "[Door:{doorId}-{name}] doesn't founed all the variable requested in the formula", doorDefinition.Uid, doorDefinition.ToDebugDisplayName());

            this._conditionExpression = ExpressionBuilder.BuildBoolLogicStatement(doorDefinition.LogicalFormula, variables);
            token.ThrowIfCancellationRequested();

            return base.OnInitializeAsync(doorDefinition, token);
        }

        /// </<inheritdoc />
        protected override ValueTask<StimulationReponse> OnDoorStimulateAsync(BooleanLogicalDoorDefinition doordDefinition, CancellationToken token)
        {
            if (doordDefinition == null || doordDefinition.VariableNames.Count == 0)
                return ValueTask.FromResult(StimulationReponse.Default);

            Span<bool> arguments = stackalloc bool[doordDefinition!.VariableNames.Count];

            var activeSignals = base.GetLastActiveSignalNotConsumed();

            var result = false;
            var responsibleSignals = EnumerableHelper<SignalMessage>.ReadOnly;

            token.ThrowIfCancellationRequested();
            foreach (var activeSignal in activeSignals)
            {
                if (this._indexedVariables!.TryGetValue(activeSignal.From.SourceDefinitionId, out var indx))
                    arguments[indx] = true;
            }

            result = this._conditionExpression!.Ask(arguments);

            token.ThrowIfCancellationRequested();
            if (result)
            {
                responsibleSignals = activeSignals;
            }

            if (doordDefinition.UseCurrentDoorStatus && doordDefinition.ActiveWindowInterval != null)
            {
                var lastDoorSignal = base.GetLastSignalReceived(doordDefinition.DoorId.Uid);

                var utcNow = this.TimeHandler.UtcNow;

                var nextTick = (lastDoorSignal?.SendUtcTime ?? utcNow) + doordDefinition.ActiveWindowInterval;

                if (utcNow < nextTick.Value)
                {
                    this._simulateThisTimerToken?.Dispose();

                    this._simulateThisTimerToken = RegisterTimer(SimulateThisFireAsync,
                                                                 string.Empty,
                                                                 nextTick.Value - utcNow,
                                                                 s_noRepetitionDelay);
                }
            }

            token.ThrowIfCancellationRequested();

            var stimulationResult = new StimulationReponse(result, responsibleSignals);
            return ValueTask.FromResult(stimulationResult);
        }

        /// <summary>
        /// Simulates the current door trigger.
        /// </summary>
        /// <remarks>
        ///     Use full when the argument "this" is used in the logical equation
        /// </remarks>
        private Task SimulateThisFireAsync(object _)
        {
            this._simulateThisTimerToken?.Dispose();
            this._simulateThisTimerToken = null;

            return base.ManuallyTriggerStimulation();
        }

        #endregion
    }
}
