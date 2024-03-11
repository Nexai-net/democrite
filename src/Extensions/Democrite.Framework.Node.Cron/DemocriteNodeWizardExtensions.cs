// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Configurations; to easy configuration use
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Node.Cron;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    public static class DemocriteNodeWizardExtensions
    {
        /// <summary>
        /// Setup cron triggers into the democrite node server.
        /// </summary>
        /// <param name="supportSecondCron">Democrite support cron under minute time, but need more configuration to override the default orlean limit to 1 minute.</param>
        /// <remarks>
        ///     If you want second handling and custom the <see cref="ReminderOptions"/> plz add the <see cref="ReminderOptions.MinimumReminderPeriod"/> minimal value to 1 sec
        /// </remarks>
        public static IDemocriteNodeWizard UseCronTriggers(this IDemocriteNodeWizard democriteNodeWizard, bool supportSecondCron = false)
        {
            democriteNodeWizard.ConfigureServices(s =>
            {
                s.AddGrainService<CronVGrainService>()
                 .AddReminders();

                    // To support second timed cron we need to override the default option orleans reminders
                if (supportSecondCron)
                {
                    var reminderOptions = new ReminderOptions()
                    {
                        InitializationTimeout = TimeSpan.FromSeconds(2),
                        RefreshReminderListPeriod = TimeSpan.FromSeconds(2),
                        MinimumReminderPeriod = TimeSpan.FromSeconds(1),
                    };

                    s.AddSingleton(reminderOptions.ToOption())
                     .AddSingleton(reminderOptions.ToMonitorOption());
                }
            });
            return democriteNodeWizard;
        }
    }
}
