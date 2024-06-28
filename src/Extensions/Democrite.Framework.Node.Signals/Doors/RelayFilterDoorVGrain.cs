// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Door that will relay a signal only if it match the condition setup on the <see cref="RelayFilterDoorDefinition"/>
    /// </summary>
    /// <seealso cref="DoorBaseVGrain{IRelayFilterVGrain, RelayFilterDoorDefinition}" />
    /// <seealso cref="IRelayFilterVGrain" />
    [DemocriteSystemVGrain]
    internal sealed class RelayFilterDoorVGrain : DoorBaseVGrain<IRelayFilterVGrain, RelayFilterDoorDefinition>, IRelayFilterVGrain, ISignalReceiverReadOnly
    {
        #region Fields

        private static readonly Type s_signalMessageTrait = typeof(SignalMessage);
        private int _signalMessageParameterIndex;
        private Type? _extraContentFilterType;
        private int _extraContentFilterIndex;
        private int _filterParameterCount;
        private Delegate? _condition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayFilterDoorVGrain"/> class.
        /// </summary>
        public RelayFilterDoorVGrain(ILogger<IRelayFilterVGrain> logger,
                                     [PersistentState("Doors", nameof(Democrite))] IPersistentState<DoorHandlerStateSurrogate> persistentState,
                                     ISignalService signalService,
                                     IDoorDefinitionProvider doorDefinitionProvider,
                                     ITimeManager timeHandler,
                                     IGrainOrleanFactory grainFactory,
                                     TimeSpan? stimulationTimeout = null)
            : base(logger, persistentState, signalService, doorDefinitionProvider, timeHandler, grainFactory, stimulationTimeout)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected sealed override async ValueTask OnInitializeAsync(RelayFilterDoorDefinition doordDefinition, CancellationToken token)
        {
            await base.OnInitializeAsync(doordDefinition, token);

            var conditionExpression = doordDefinition.FilterCondition.ToExpressionDelegateWithResult();
            this._condition = conditionExpression.Compile();

            token.ThrowIfCancellationRequested();

            var parameterIndexed = conditionExpression.Parameters
                                                      .Select((v, indx) => (Parameter: v, Index: indx))
                                                      .ToDictionary(k => k.Parameter.Type);

            this._filterParameterCount = parameterIndexed.Count;

            this._signalMessageParameterIndex = parameterIndexed[s_signalMessageTrait].Index;
            parameterIndexed.Remove(s_signalMessageTrait);

            System.Diagnostics.Debug.Assert(parameterIndexed.Count < 2);
            token.ThrowIfCancellationRequested();

            if (parameterIndexed.Count == 1)
            {
                var info = parameterIndexed.First();
                this._extraContentFilterType = info.Value.Parameter.Type;
                this._extraContentFilterIndex = info.Value.Index;
            }
        }

        /// <inheritdoc />
        protected sealed override ValueTask<StimulationReponse> OnDoorStimulateAsync(RelayFilterDoorDefinition doordDef, CancellationToken token)
        {
            var signals = base.GetLastActiveSignalNotConsumed();

            if (signals == null || signals.Count == 0)
                return ValueTask.FromResult(StimulationReponse.Default);

            SignalMessage? signalRelay = null;
            var signalUse = new List<SignalMessage>();
            var args = new object?[this._filterParameterCount];

            foreach (var signal in signals)
            {
                signalUse.Add(signal);
                if (_extraContentFilterType != null)
                {
                    var content = signal.From.GetContent();

                    if (content == null || content.GetType().IsAssignableTo(this._extraContentFilterType) == false)
                        break;

                    args[this._extraContentFilterIndex] = content;
                }

                args[this._signalMessageParameterIndex] = signal;

                token.ThrowIfCancellationRequested();
                if ((bool)this._condition!.DynamicInvoke(args)!)
                {
                    signalRelay = signal;
                    break;
                }
            }

            if (signalUse.Any())
                MarkAsUsed(signalUse);

            var result = new StimulationReponse(signalRelay != null,
                                                signalRelay != null ? new[] { signalRelay } : EnumerableHelper<SignalMessage>.ReadOnlyArray,
                                                signals.Except(signalUse).Any(),
                                                !doordDef.DontRelaySignalContent);

            return ValueTask.FromResult(result);
        }

        #endregion
    }
}
