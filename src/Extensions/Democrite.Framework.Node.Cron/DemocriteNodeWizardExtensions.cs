// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Node.Abstractions.Configurations;
    using Democrite.Framework.Node.Cron;

    public static class DemocriteNodeWizardExtensions
    {
        /// <summary>
        /// Setup cron triggers into the democrite node server.
        /// </summary>
        public static IDemocriteNodeWizard UseCronTriggers(this IDemocriteNodeWizard democriteNodeWizard)
        {
            democriteNodeWizard.ConfigureServices(s => s.AddGrainService<CronVGrainService>()
                                                        .AddReminders());
            return democriteNodeWizard;
        }
    }
}
