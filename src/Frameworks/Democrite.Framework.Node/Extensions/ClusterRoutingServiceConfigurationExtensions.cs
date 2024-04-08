// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Extensions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Core.Services;
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Services;

    using Microsoft.Extensions.DependencyInjection;

    using System.Diagnostics;
    using System.Linq;

    public static class ClusterRoutingServiceConfigurationExtensions
    {
        /// <summary>
        /// Setups the grain routing services.
        /// </summary>
        public static IServiceCollection SetupGrainRoutingServices(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddSingleton<GrainOrleanFactory>(p =>
            {
                var orleanGrainFactory = typeof(MembershipEntry).Assembly.GetTypes().FirstOrDefault(d => d.Name == "GrainFactory" &&
                                                                                                        (d.Attributes & System.Reflection.TypeAttributes.NotPublic) == System.Reflection.TypeAttributes.NotPublic &&
                                                                                                         d.IsAssignableTo(typeof(IGrainFactory)));
                Debug.Assert(orleanGrainFactory != null);
                return new GrainOrleanFactory((IGrainFactory)p.GetRequiredService(orleanGrainFactory));
            });

            serviceDescriptors.AddSingleton<IGrainOrleanFactory>(p => p.GetRequiredService<GrainOrleanFactory>());
            serviceDescriptors.AddSingleton<IVGrainDemocriteSystemProvider, VGrainDemocriteSystemProvider>();

            serviceDescriptors.AddSingleton<ISequenceVGrainProviderFactory, SequenceVGrainProviderFactory>();

            serviceDescriptors.AddSingleton<GrainRouteSiloRootService>()
                                            .AddSingleton<IVGrainRouteService>(p => p.GetRequiredService<GrainRouteSiloRootService>())
                                            .AddSingleton<IGrainFactory>(p => new GrainFactoryScoped(p.GetRequiredService<GrainOrleanFactory>().GrainFactory, p.GetRequiredService<IVGrainRouteService>()));

            return serviceDescriptors;
        }
    }
}
