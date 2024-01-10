// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Orleans.Serialization.Cloning;
    using Orleans.Serialization.Serializers;
    using Orleans.Serialization;

    using System;

    /// <summary>
    ///  Cluster configuration wizard tools
    /// </summary>
    public interface IDemocriteWizard<TWizard, TWizardConfig> : IDemocriteCoreConfigurationWizard<TWizardConfig>
        where TWizard : IDemocriteWizard<TWizard, TWizardConfig>
        where TWizardConfig : IDemocriteCoreConfigurationWizard<TWizardConfig>
    {
        /// <summary>
        /// Setups dependency injection relations
        /// </summary>
        TWizard Configure(Action<TWizardConfig> configureDelegate);

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="ILoggingBuilder"/>. This may be called multiple times.
        /// </summary>
        /// <param name="configureLogging">The delegate that configures the <see cref="ILoggingBuilder"/>.</param>
        TWizard ConfigureLogging(Action<ILoggingBuilder> configureLogging);

        /// <summary>
        /// Manual configuration of services.
        /// </summary>
        TWizard ConfigureServices(Action<IServiceCollection> config);

        /// <summary>
        /// Manual configuration of services.
        /// </summary>
        TWizard ConfigureServices(Action<IServiceCollection, IConfiguration> config);

        /// <summary>
        /// Adds the custom serializer must inherite from orleans interfaces <see cref="IGeneralizedCodec"/>, <see cref="IGeneralizedCopier"/>, <see cref="ITypeFilter"/>
        /// </summary>
        TWizard AddCustomSerializer<TInterfaceSerializer, TSerializerImplementation>()
            where TInterfaceSerializer : class, IGeneralizedCodec, IGeneralizedCopier, ITypeFilter
            where TSerializerImplementation : class, TInterfaceSerializer;
    }
}
