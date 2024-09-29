// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Extensions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Core.Attributes;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.Framework.Core.Services;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Core.Storages;

    using Elvex.Toolbox.Abstractions.Patterns.Workers;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Patterns.Workers;
    using Elvex.Toolbox.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using Orleans.Runtime;

    public static class DemocriteCoreServicesExtensions
    {
        /// <summary>
        /// Setups the core services if no previous implementation have been setups
        /// </summary>
        public static IServiceCollection SetupCoreServices(this IServiceCollection serviceCollection)
        {
            AddDefaultCoreService<ISequenceDefinitionProvider, SequenceDefinitionProvider>(serviceCollection);
            AddDefaultCoreService<IDefinitionProvider, DefinitionProvider>(serviceCollection);

            AddDefaultCoreService<IDiagnosticLogger, DiagnosticLogger>(serviceCollection);

            AddDefaultCoreService<IProcessSystemService, ProcessSystemService>(serviceCollection);

            serviceCollection.TryAddSingleton<IJsonSerializer>(NewtownJsonSerializer.Instance);
            AddDefaultCoreService<ITimeManager, TimeManager>(serviceCollection);

            serviceCollection.AddSingleton<IVGrainIdDedicatedFactory, VGrainIdFactoryDedicatedTemplates>();
            serviceCollection.AddSingleton<IVGrainIdDedicatedFactory, VGrainIdFactorySingleton>();
            AddDefaultCoreService<IVGrainIdDefaultFactory, VGrainIdFactoryDefault>(serviceCollection);

            AddDefaultCoreService<IVGrainIdFactory, VGrainIdFactory>(serviceCollection);
            AddDefaultCoreService<IVGrainProvider, VGrainProvider>(serviceCollection);

            AddDefaultCoreService<IDemocriteExecutionHandler, DemocriteExecutionHandler>(serviceCollection);

            AddDefaultCoreService<ISignalService, SignalService>(serviceCollection);
            AddDefaultCoreService<IWorkerTaskSchedulerProvider, WorkerTaskSchedulerProvider>(serviceCollection);

            AddDefaultCoreService<IRepositoryFactory, RepositoryFactory>(serviceCollection);
            AddDefaultCoreService<IAttributeToFactoryMapper<RepositoryAttribute>, RepositoryAttributeMapper>(serviceCollection);

            AddDefaultCoreService<IRemoteGrainServiceFactory, RemoteGrainServiceFactory>(serviceCollection);
            AddDefaultCoreService<IDynamicDefinitionHandler, DynamicDefinitionHandler>(serviceCollection);

            return serviceCollection;
        }

        /// <summary>
        /// Adds service <typeparamref name="TContract"/> using singleton implementation <typeparamref name="TImplementation"/>
        /// if it doesn't already exist
        /// </summary>
        private static void AddDefaultCoreService<TContract, TImplementation>(IServiceCollection serviceCollection, bool tryAdd = true)
            where TContract : class
            where TImplementation : class, TContract
        {
            //if (!serviceCollection.Any(s => s.ServiceType == typeof(TContract)))
            {
                if (tryAdd)
                    serviceCollection.TryAddSingleton<TContract, TImplementation>();
                else
                    serviceCollection.AddSingleton<TContract, TImplementation>();
            }
        }
    }
}
