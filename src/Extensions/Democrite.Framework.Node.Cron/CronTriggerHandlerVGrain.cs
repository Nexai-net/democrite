// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Cron
{
    using Cronos;

    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Triggers;
    using Democrite.Framework.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Virtual Grain handler incharge to a specific <see cref="CronTriggerDefinition"/>. <br />
    /// Handled the cron expression and calculate the next trigger date and fire.
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="ICronTriggerHandlerVGrain" />
    /// <seealso cref="IGrainWithGuidKey" />
    /// <remarks>
    ///     Helper : https://crontab.guru/
    /// </remarks>
    [DemocriteSystemVGrain]
    internal sealed class CronTriggerHandlerVGrain : TriggerBaseHandlerVGrain<CronReminderState, CronTriggerDefinition, ICronTriggerHandlerVGrain>, IRemindable, ICronTriggerHandlerVGrain, IGrainWithGuidKey
    {
        #region Fields

        private static readonly TimeSpan s_maxAllowPeriodTime = TimeSpan.FromMicroseconds(uint.MaxValue - 10);

        private readonly IInputSourceProviderFactory _inputSourceProviderFactory;
        private readonly ISequenceDefinitionProvider _sequenceDefinitionProvider;
        private readonly IDemocriteExecutionHandler _democriteExecutionHandler;
        private readonly ITriggerDefinitionProvider _triggerDefinitionProvider;

        private readonly ITimeManager _timeHandler;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronTriggerHandlerVGrain"/> class.
        /// </summary>
        public CronTriggerHandlerVGrain(ILogger<ICronTriggerHandlerVGrain> logger,
                                        [PersistentState(nameof(Triggers), nameof(Democrite))] IPersistentState<CronReminderState> persistentState,
                                        ITriggerDefinitionProvider triggerDefinitionProvider,
                                        ISequenceDefinitionProvider sequenceDefinitionProvider,
                                        ISignalDefinitionProvider signalDefinitionProvider,
                                        IDemocriteExecutionHandler democriteExecutionHandler,
                                        IInputSourceProviderFactory inputSourceProviderFactory,
                                        ISignalService signalService,
                                        ITimeManager timeHandler)

            : base(logger,
                   persistentState,
                   triggerDefinitionProvider,
                   sequenceDefinitionProvider,
                   signalDefinitionProvider,
                   democriteExecutionHandler,
                   inputSourceProviderFactory,
                   signalService)
        {
            this._inputSourceProviderFactory = inputSourceProviderFactory;

            this._sequenceDefinitionProvider = sequenceDefinitionProvider;
            this._triggerDefinitionProvider = triggerDefinitionProvider;
            this._democriteExecutionHandler = democriteExecutionHandler;

            this._timeHandler = timeHandler;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            await EnsureTriggerDefinitionAsync();

            if (!await UpdateNextTriggerAndCheckCanExecuteAsync(this.TriggerDefinition))
                return;

            await base.FireTriggerAsync();
        }

        /// <inheritdoc />
        public override async Task UpdateAsync()
        {
            await base.UpdateAsync();
            await UpdateNextTriggerAndCheckCanExecuteAsync(this.TriggerDefinition!);
        }

        /// <inheritdoc />
        private async Task<bool> UpdateNextTriggerAndCheckCanExecuteAsync(ICronTriggerDefinition definition)
        {
            if (definition.Enabled == false)
            {
                var oldReminderToken = this.State!.GrainReminderToken;
                if (oldReminderToken is not null)
                    await GrainReminderExtensions.UnregisterReminder(this, oldReminderToken);

                this.State.GrainReminderToken = null;
                return false;
            }

            if (string.IsNullOrWhiteSpace(definition.CronExpression))
            {
                throw new InvalidDefinitionPropertyValueException(typeof(ICronTriggerDefinition),
                                                                  nameof(ICronTriggerDefinition.CronExpression),
                                                                  definition.CronExpression,
                                                                  "not null");
            }

            CronExpression cronExpression;

            try
            {
                cronExpression = CronExpression.Parse(definition.CronExpression);

            }
            catch (Exception ex)
            {
                throw new InvalidDefinitionPropertyValueException(typeof(ICronTriggerDefinition),
                                                                  nameof(ICronTriggerDefinition.CronExpression),
                                                                  definition.CronExpression,
                                                                  ex.Message,
                                                                  ex);
            }

            var utcNow = this._timeHandler.UtcNow;

            var current = cronExpression.GetNextOccurrence(utcNow.AddMinutes(-1), TimeZoneInfo.Utc);

            var next = cronExpression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);

            if (next is null || current == null)
                return false;

            var after = cronExpression.GetNextOccurrence(next.Value.AddSeconds(1), TimeZoneInfo.Utc) ?? next.Value.AddMinutes(10);

            var dueTime = next.Value - utcNow;
            var period = after - next.Value;

            if (period > s_maxAllowPeriodTime)
                period = s_maxAllowPeriodTime;

            var reminderToken = await GrainReminderExtensions.RegisterOrUpdateReminder(this, definition.Uid.ToString(), dueTime, period);
            this.State!.GrainReminderToken = reminderToken;

            return Math.Abs((current.Value - utcNow).TotalMinutes) < 1.0;
        }

        #endregion
    }
}
