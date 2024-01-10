// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Auto configure - In AppDomain Memory - custom state memory
    /// </summary>
    /// <seealso cref="INodeCustomMemoryAutoConfigurator" />
    public sealed class AutoDefaultCustomMemoryConfigurator : INodeCustomMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            throw new NotSupportedException("Call AutoConfigureCustomStorage to pass key");
        }

        /// <inheritdoc />
        public void AutoConfigureCustomStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                               IConfiguration configuration,
                                               IServiceCollection serviceCollection,
                                               ILogger logger,
                                               string key)
        {
            ArgumentNullException.ThrowIfNull(democriteBuilderWizard);

            // Prever type case before instead on allocating inline variable in condition if it's use outside the scope

#pragma warning disable IDE0019 // Use pattern matching
            var siloBuilder = democriteBuilderWizard.SourceOrleanBuilder as ISiloBuilder;
#pragma warning restore IDE0019 // Use pattern matching

            if (democriteBuilderWizard.IsClient || siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");

            siloBuilder.AddMemoryGrainStorage(key);
        }
    }
}
