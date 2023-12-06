// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Extensions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.Framework.Core.Services;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Services;

    using Microsoft.Extensions.DependencyInjection;

    using System.Linq;

    public static class DemocriteCoreServicesExtensions
    {
        /// <summary>
        /// Setups the core services if no previous implementation have been setups
        /// </summary>
        public static IServiceCollection SetupCoreServices(this IServiceCollection serviceCollection)
        {
            AddDefaultCoreService<ISequenceDefinitionProvider, SequenceDefinitionProvider>(serviceCollection);
            AddDefaultCoreService<IDiagnosticLogger, DiagnosticLogger>(serviceCollection);

            AddDefaultCoreService<IProcessSystemService, ProcessSystemService>(serviceCollection);
            AddDefaultCoreService<IJsonSerializer, SystemJsonSerializer>(serviceCollection);
            AddDefaultCoreService<ITimeManager, TimeManager>(serviceCollection);

            AddDefaultCoreService<IVGrainIdFactory, VGrainIdFactory>(serviceCollection);
            AddDefaultCoreService<IVGrainProvider, VGrainProvider>(serviceCollection);

            AddDefaultCoreService<IDemocriteExecutionHandler, DemocriteExecutionHandler>(serviceCollection);

            AddDefaultCoreService<ISignalService, SignalService>(serviceCollection);

            return serviceCollection;
        }

        /// <summary>
        /// Adds service <typeparamref name="TContract"/> using singleton implementation <typeparamref name="TImplementation"/>
        /// if it doesn't already exist
        /// </summary>
        private static void AddDefaultCoreService<TContract, TImplementation>(IServiceCollection serviceCollection)
            where TContract : class
            where TImplementation : class, TContract
        {
            if (!serviceCollection.Any(s => s.ServiceType == typeof(TContract)))
                serviceCollection.AddSingleton<TContract, TImplementation>();
        }
    }
}
