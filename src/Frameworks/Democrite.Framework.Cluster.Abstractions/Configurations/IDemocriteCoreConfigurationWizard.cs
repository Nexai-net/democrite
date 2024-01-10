// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Wizard used to setup services and configuration
    /// </summary>
    public interface IDemocriteCoreConfigurationWizard<TSubWizard>
        where TSubWizard : IDemocriteCoreConfigurationWizard<TSubWizard>
    {
        /// <summary>
        /// Add dependency injection service
        /// </summary>
        TSubWizard Add(ServiceDescriptor serviceDescriptor);

        /// <summary>
        /// Add service <typeparamref name="TImplementation"/> instance with access key <typeparamref name="TService"/>
        /// </summary>
        TSubWizard AddService<TService>(TService instance)
            where TService : class;

        /// <summary>
        /// Add service <typeparamref name="TImplementation"/> instance with access key <typeparamref name="TService"/>
        /// </summary>
        TSubWizard AddService<TService>(Func<IServiceProvider, TService> factory)
            where TService : class;

        /// <summary>
        /// Add service <typeparamref name="TImplementation"/> instance with access key <typeparamref name="TService"/>
        /// </summary>
        TSubWizard AddService(Type service, Type instanceType);

        /// <summary>
        /// Add service <typeparamref name="TImplementation"/> instance with access key <typeparamref name="TService"/>
        /// </summary>
        TSubWizard AddService(Type service, object instance);

        /// <summary>
        /// Add service <typeparamref name="TImplementation"/> with access key <typeparamref name="TService"/>
        /// </summary>
        TSubWizard AddService<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        /// Map configuration section to a specific <typeparamref name="TOption"/> option
        /// </summary>
        TSubWizard AddOptionMapping<TOptions>(string section) where TOptions : class;
    }

    /// <summary>
    /// Wizard used to setup services and configuration
    /// </summary>
    public interface IDemocriteCoreConfigurationWizard : IDemocriteCoreConfigurationWizard<IDemocriteCoreConfigurationWizard>
    {

    }
}
