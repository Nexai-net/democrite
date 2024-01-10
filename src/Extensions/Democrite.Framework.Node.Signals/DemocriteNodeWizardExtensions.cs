﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Node.Signals;

    public static class DemocriteNodeWizardExtensions
    {
        /// <summary>
        /// Setup all services to enabled signal management on this node.
        /// </summary>
        public static IDemocriteNodeWizard UseSignals(this IDemocriteNodeWizard democriteNodeWizard)
        {
            democriteNodeWizard.ConfigureServices(s => s.AddGrainService<DoorVGrainService>());
            return democriteNodeWizard;
        }
    }
}
