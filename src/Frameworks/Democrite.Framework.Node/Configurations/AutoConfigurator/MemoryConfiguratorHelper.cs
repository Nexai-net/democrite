// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Node.Models;

    using Microsoft.Extensions.Options;

    using Orleans.Configuration;

    using System.Linq;

    internal static class MemoryConfiguratorHelper
    {
        /// <summary>
        /// Options the configurator.
        /// </summary>
        internal static void OptionConfigurator(OptionsBuilder<MemoryGrainStorageOptions> options)
        {
            var defaultMemoryOptionServiceDescriptor = options.Services.Where(s => s.IsKeyedService == false && 
                                                                              s.ImplementationInstance is DefaultMemoryGrainStorageOptions)
                                                                       .LastOrDefault();              

            if (defaultMemoryOptionServiceDescriptor is not null && defaultMemoryOptionServiceDescriptor.ImplementationInstance is DefaultMemoryGrainStorageOptions defaultOption)
            {
                options.PostConfigure(m =>
                {
                    m.NumStorageGrains = (int)defaultOption.NumStorageGrains;
                    m.InitStage = (int)defaultOption.InitStage;
                });
            }
        }
    }
}
