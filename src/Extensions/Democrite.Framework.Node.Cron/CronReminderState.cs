// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Cron
{
    using Democrite.Framework.Node.Abstractions.Triggers;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Cron Reminder state used to store information and cache to help <see cref="CronTriggerHandlerVGrain"/> to perform
    /// </summary>
    [GenerateSerializer]
    public sealed class CronReminderState : TriggerState
    {
        #region Fields

        private IGrainReminder? _grainReminder;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the grain reminder token.
        /// </summary>
        [Id(0)]
        public IGrainReminder? GrainReminderToken
        {
            get { return this._grainReminder; }
            set
            {
                if (object.ReferenceEquals(this._grainReminder, value))
                    return;

                var old = this._grainReminder;
                this._grainReminder = value;

                if (old is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        #endregion
    }
}
