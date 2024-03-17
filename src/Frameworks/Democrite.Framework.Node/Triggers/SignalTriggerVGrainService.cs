// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <seealso cref="GrainService" />
    /// <seealso cref="ISignalTriggerVGrainService" />
    internal sealed class SignalTriggerVGrainService : GrainService, ISignalTriggerVGrainService
    {
        #region Fields

        private readonly ITriggerDefinitionProvider _triggerDefinitionProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerVGrainService"/> class.
        /// </summary>
        public SignalTriggerVGrainService(GrainId grainId,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          ITriggerDefinitionProvider triggerDefinitionProvider,
                                          IGrainFactory grainFactory)
            : base(grainId, silo, loggerFactory)
        {
            this._triggerDefinitionProvider = triggerDefinitionProvider;
            this._grainFactory = grainFactory;

            this._triggerDefinitionProvider.DataChanged -= TriggerDefinitionProvider_DataChanged;
            this._triggerDefinitionProvider.DataChanged += TriggerDefinitionProvider_DataChanged;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override async Task Start()
        {
            await EnsureSignalTriggerAreSetups();
            await base.Start();
        }

        #region Tools

        /// <summary>
        /// Triggers the definition provider data changed.
        /// </summary>
        private async void TriggerDefinitionProvider_DataChanged(object? sender, IReadOnlyCollection<Guid> dataThatChanged)
        {
            await EnsureSignalTriggerAreSetups();
        }

        /// <summary>
        /// Ensures the signal trigger are setups.
        /// </summary>
        private async Task EnsureSignalTriggerAreSetups()
        {
            using (var timeoutToken = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(10)))
            {
                var signalTriggers = await this._triggerDefinitionProvider.GetValuesAsync(t => t.TriggerType == TriggerTypeEnum.Signal, timeoutToken.Content);

                var initTask = signalTriggers.Select(signalTrigger =>
                {
                    var triggerHandler = this._grainFactory.GetGrain<ISignalTriggerVGrain>(signalTrigger.Uid);
                    return triggerHandler.UpdateAsync();
                }).ToArray();

                await initTask.SafeWhenAllAsync(timeoutToken.Content);
            }
        }

        #endregion

        #endregion
    }
}
