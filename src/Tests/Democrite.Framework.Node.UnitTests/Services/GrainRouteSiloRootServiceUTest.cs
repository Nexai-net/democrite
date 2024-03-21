// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Services
{
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Node.Abstractions.Administrations;
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Services;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="GrainRouteSiloRootService"/>
    /// </summary>
    public sealed class GrainRouteSiloRootServiceUTest
    {
        [Fact]
        public void No_Redirection()
        {
            var democriteSystemProviderMock = Substitute.For<IVGrainDemocriteSystemProvider>();

            var silo = new GrainRouteSiloRootService(democriteSystemProviderMock, NullLogger<IVGrainRouteService>.Instance);

            var solved = silo.GetRoute(typeof(GrainRouteFixedServiceUTest.IMainRedirectionTest), null, null, null);

            Check.That(solved.Cachable).IsTrue();
            Check.That(solved.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(solved.TargetGrain).IsNotNull().And.IsEqualTo(typeof(GrainRouteFixedServiceUTest.IMainRedirectionTest));
        }

        [Fact]
        public async Task Update_Redirection()
        {
            var democriteSystemProviderMock = Substitute.For<IVGrainDemocriteSystemProvider>();

            var silo = new GrainRouteSiloRootService(democriteSystemProviderMock, NullLogger<IVGrainRouteService>.Instance);

            var solved = silo.GetRoute(typeof(GrainRouteFixedServiceUTest.IMainRedirectionTest), null, null, null);

            Check.That(solved.Cachable).IsTrue();
            Check.That(solved.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(solved.TargetGrain).IsNotNull().And.IsEqualTo(typeof(GrainRouteFixedServiceUTest.IMainRedirectionTest));

            var clusterRouteRegistryVGrain = Substitute.For<IClusterRouteRegistryVGrain>();

            var redirection = VGrainInterfaceRedirectionDefinition.Create<GrainRouteFixedServiceUTest.IBaseRedirectionTest, GrainRouteFixedServiceUTest.ISecondRedirectionTest>();
            var etag = new EtagContainer<IReadOnlyCollection<VGrainRedirectionDefinition>>(Guid.NewGuid().ToString(), redirection.AsEnumerable().ToArray());

            democriteSystemProviderMock.GetVGrainAsync<IClusterRouteRegistryVGrain>(null, Arg.Any<ILogger>()).Returns(clusterRouteRegistryVGrain);
            clusterRouteRegistryVGrain.GetGlobalRedirection(string.Empty, Arg.Any<GrainCancellationToken>()).Returns(etag);
            
            await silo.UpdateGlobalRedirectionAsync(string.Empty, default);

            var redirectSolved = silo.GetRoute(typeof(GrainRouteFixedServiceUTest.IBaseRedirectionTest), null, null, null);

            Check.That(redirectSolved.Cachable).IsTrue();
            Check.That(redirectSolved.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(redirectSolved.TargetGrain).IsNotNull().And.IsEqualTo(typeof(GrainRouteFixedServiceUTest.ISecondRedirectionTest));
        }

        [Fact]
        public async Task Update_Redirection_Hierarchy_Cache()
        {
            var democriteSystemProviderMock = Substitute.For<IVGrainDemocriteSystemProvider>();

            // Setup Root
            var silo = new GrainRouteSiloRootService(democriteSystemProviderMock, NullLogger<IVGrainRouteService>.Instance);

            var solved = silo.GetRoute(typeof(GrainRouteFixedServiceUTest.IMainRedirectionTest), null, null, null);

            Check.That(solved.Cachable).IsTrue();
            Check.That(solved.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(solved.TargetGrain).IsNotNull().And.IsEqualTo(typeof(GrainRouteFixedServiceUTest.IMainRedirectionTest));

            // Chlid with no redirection
            var childService = new GrainRouteFixedService(null, silo);

            var childComputeRoute = childService.GetRoute(typeof(GrainRouteFixedServiceUTest.IBaseRedirectionTest), null, null, null);

            Check.That(childComputeRoute.Cachable).IsTrue();
            Check.That(childComputeRoute.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(childComputeRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(GrainRouteFixedServiceUTest.IBaseRedirectionTest));

            // Update root
            var clusterRouteRegistryVGrain = Substitute.For<IClusterRouteRegistryVGrain>();

            var redirection = VGrainInterfaceRedirectionDefinition.Create<GrainRouteFixedServiceUTest.IBaseRedirectionTest, GrainRouteFixedServiceUTest.ISecondRedirectionTest>();
            var etag = new EtagContainer<IReadOnlyCollection<VGrainRedirectionDefinition>>(Guid.NewGuid().ToString(), redirection.AsEnumerable().ToArray());

            democriteSystemProviderMock.GetVGrainAsync<IClusterRouteRegistryVGrain>(null, Arg.Any<ILogger>()).Returns(clusterRouteRegistryVGrain);
            clusterRouteRegistryVGrain.GetGlobalRedirection(string.Empty, Arg.Any<GrainCancellationToken>()).Returns(etag);

            await silo.UpdateGlobalRedirectionAsync(string.Empty, default);

            var redirectSolved = silo.GetRoute(typeof(GrainRouteFixedServiceUTest.IBaseRedirectionTest), null, null, null);

            Check.That(redirectSolved.Cachable).IsTrue();
            Check.That(redirectSolved.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(redirectSolved.TargetGrain).IsNotNull().And.IsEqualTo(typeof(GrainRouteFixedServiceUTest.ISecondRedirectionTest));

            // Try child
            var childOtherComputeRoute = childService.GetRoute(typeof(GrainRouteFixedServiceUTest.IBaseRedirectionTest), null, null, null);

            Check.That(childOtherComputeRoute.Cachable).IsTrue();
            Check.That(childOtherComputeRoute.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(childOtherComputeRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(GrainRouteFixedServiceUTest.ISecondRedirectionTest));
        }
    }
}
