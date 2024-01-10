// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Configurator in charge to setup the default services
    /// </summary>
    /// <seealso cref="IMembershipsAutoConfigurator" />
    public sealed class AutoDefaultClientConfigurator : IMembershipsAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteClusterBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceDescriptors,
                                  ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(democriteBuilderWizard);

            // Prever type case before instead on allocating inline variable in condition if it's use outside the scope

#pragma warning disable IDE0019 // Use pattern matching
            var clientBuilder = democriteBuilderWizard.SourceOrleanBuilder as IClientBuilder;
#pragma warning restore IDE0019 // Use pattern matching

            if (democriteBuilderWizard.IsServerNode || clientBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by client side");

            clientBuilder.UseLocalhostClustering();
        }
    }
}
