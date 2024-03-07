// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Bag.Toolbox.Abstractions;
    using Democrite.Framework.Bag.Toolbox.Grains;

    /// <summary>
    /// Configure the toolbox vgrain, services ...
    /// </summary>
    public static class DemocriteNodeConfigurationToolboxExtensions
    {
        /// <summary>
        /// Adds the debug tools, (Sequence, vgrins ...).
        /// </summary>
        public static IDemocriteNodeWizard AddToolBoxTools(this IDemocriteNodeWizard democriteNodeWizard)
        {
#pragma warning disable IDE0053 // Use expression body for lambda expression
            democriteNodeWizard.SetupNodeVGrains(s =>
            {
                s.Add<IDelayVGrain, DelayVGrain>();
            });
#pragma warning restore IDE0053 // Use expression body for lambda expression

            return democriteNodeWizard;
        }
    }
}