// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Signals;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Statements;

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
    internal sealed class LogicalDoorVGrain : DoorBaseVGrain<ILogicalDoorVGrain, LogicalAggregatorDoorDefinition>, ILogicalDoorVGrain
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
                                   IGrainFactory grainFactory)

            : base(logger, persistentState, signalService, doorDefinitionProvider, timeHandler, grainFactory)
        {
        }

        #endregion

        #region Methods

        /// </<inheritdoc />
        protected override ValueTask OnInitializeAsync(LogicalAggregatorDoorDefinition doordDefinition)
        {
            var variables = doordDefinition.VariableNames.Keys.ToList();

            this._indexedVariables = doordDefinition.DoorSourceIds
                                                       .Select(s => (s.Uid, doordDefinition.VariableNames.First(kv => kv.Value == s.Uid).Key))
                                                       .Concat(doordDefinition.SignalSourceIds
                                                                                 .Select(s => (s.Uid, doordDefinition.VariableNames.First(kv => kv.Value == s.Uid).Key)))
                                                       .ToDictionary(kv => kv.Uid, v => variables.IndexOf(v.Key));

            this._conditionExpression = ExpressionBuilder.BuildBoolLogicStatement(doordDefinition.LogicalFormula, variables);

            return base.OnInitializeAsync(doordDefinition);
        }

        /// </<inheritdoc />
        protected override ValueTask<(bool result, IReadOnlyCollection<SignalMessage> responsibleSignals)> OnDoorStimulateAsync(LogicalAggregatorDoorDefinition doordDefinition,
                                                                                                                                CancellationToken token)
        {
            if (doordDefinition == null || doordDefinition.VariableNames.Count == 0)
                return ValueTask.FromResult((false, EnumerableHelper<SignalMessage>.ReadOnly));

            Span<bool> arguments = stackalloc bool[doordDefinition!.VariableNames.Count];

            var activeSignals = base.GetLastActiveSignalSince();

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

            if (doordDefinition.UseCurrentDoorStatus)
            {
                var lastDoorSignal = base.GetLastSignalReceived(doordDefinition.DoorId.Uid);

                var utcNow = this.TimeHandler.UtcNow;

                var nextTick = (lastDoorSignal?.SendUtcTime ?? utcNow) + doordDefinition.Interval;

                if (utcNow < nextTick)
                {
                    this._simulateThisTimerToken?.Dispose();

                    this._simulateThisTimerToken = RegisterTimer(SimulateThisFireAsync,
                                                                      string.Empty,
                                                                      nextTick - utcNow,
                                                                      s_noRepetitionDelay);
                }
            }

            token.ThrowIfCancellationRequested();
            return ValueTask.FromResult((result, responsibleSignals));
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
