// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Node.Services;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="GrainRouteFixedService"/>
    /// </summary>
    public sealed class GrainRouteFixedServiceUTest
    {
        #region Nested

        public interface IBaseRedirectionTest : IVGrain { }

        public interface IMainRedirectionTest : IBaseRedirectionTest { }
        public interface ISecondRedirectionTest : IBaseRedirectionTest { }
        public interface IThirdRedirectionTest : ISecondRedirectionTest { }

        #endregion

        #region Methods

        [Fact]
        public void Compute_Route()
        {
            var service = new GrainRouteFixedService(null, null);

            var route = service.GetRoute(typeof(IBaseRedirectionTest), null, null, null);

            Check.That(route.Cachable).IsTrue();
            Check.That(route.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(route.TargetGrain).IsNotNull().And.IsEqualTo(typeof(IBaseRedirectionTest));

            // Apply redirection

            var redirection = VGrainInterfaceRedirectionDefinition.Create<IBaseRedirectionTest, ISecondRedirectionTest>();

            var redirecService = new GrainRouteFixedService(redirection.AsEnumerable(), null);

            var redirectRoute = redirecService.GetRoute(typeof(IBaseRedirectionTest), null, null, null);

            Check.That(redirectRoute.Cachable).IsTrue();
            Check.That(redirectRoute.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(redirectRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(ISecondRedirectionTest));
        }

        [Fact]
        public void Compute_Route_From_Parent()
        {
            // Apply redirection
            var redirection = VGrainInterfaceRedirectionDefinition.Create<IBaseRedirectionTest, ISecondRedirectionTest>();

            var parentRedirectionService = new GrainRouteFixedService(redirection.AsEnumerable(), null);

            var redirectRoute = parentRedirectionService.GetRoute(typeof(IBaseRedirectionTest), null, null, null);

            Check.That(redirectRoute.Cachable).IsTrue();
            Check.That(redirectRoute.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(redirectRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(ISecondRedirectionTest));

            // Chlid with no redirection
            var childService = new GrainRouteFixedService(null, parentRedirectionService);
            
            var childComputeRoute = childService.GetRoute(typeof(IBaseRedirectionTest), null, null, null);

            Check.That(childComputeRoute.Cachable).IsTrue();
            Check.That(childComputeRoute.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(childComputeRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(ISecondRedirectionTest));
        }

        [Fact]
        public void Compute_Route_From_Parent_And_Child()
        {
            // Apply redirection
            var redirection = VGrainInterfaceRedirectionDefinition.Create<IBaseRedirectionTest, ISecondRedirectionTest>();

            var parentRedirectionService = new GrainRouteFixedService(redirection.AsEnumerable(), null);

            var redirectRoute = parentRedirectionService.GetRoute(typeof(IBaseRedirectionTest), null, null, null);

            Check.That(redirectRoute.Cachable).IsTrue();
            Check.That(redirectRoute.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(redirectRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(ISecondRedirectionTest));

            // Chlid with other redirection
            var otherRedirection = VGrainInterfaceRedirectionDefinition.Create<IBaseRedirectionTest, IMainRedirectionTest>();

            var childService = new GrainRouteFixedService(otherRedirection.AsEnumerable(), parentRedirectionService);

            var childComputeRoute = childService.GetRoute(typeof(IBaseRedirectionTest), null, null, null);

            Check.That(childComputeRoute.Cachable).IsTrue();
            Check.That(childComputeRoute.GrainPrefixExtension).IsNullOrEmpty();

            // Expect child redirection
            Check.That(childComputeRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(IMainRedirectionTest));
        }

        /// <summary>
        /// Redirect first IBaseRedirectionTest to ISecondRedirectionTest and parent redirect ISecondRedirectionTest to IThirdRedirectionTest
        /// </summary>
        [Fact]
        public void Compute_Route_From_Hierarchy_Chain()
        {
            // Apply redirection
            var redirection = VGrainInterfaceRedirectionDefinition.Create<ISecondRedirectionTest, IThirdRedirectionTest>();

            var parentRedirectionService = new GrainRouteFixedService(redirection.AsEnumerable(), null);

            var redirectRoute = parentRedirectionService.GetRoute(typeof(ISecondRedirectionTest), null, null, null);

            Check.That(redirectRoute.Cachable).IsTrue();
            Check.That(redirectRoute.GrainPrefixExtension).IsNullOrEmpty();
            Check.That(redirectRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(IThirdRedirectionTest));

            // Chlid with other redirection
            var otherRedirection = VGrainInterfaceRedirectionDefinition.Create<IBaseRedirectionTest, ISecondRedirectionTest>();

            var childService = new GrainRouteFixedService(otherRedirection.AsEnumerable(), parentRedirectionService);

            var childComputeRoute = childService.GetRoute(typeof(IBaseRedirectionTest), null, null, null);

            Check.That(childComputeRoute.Cachable).IsTrue();
            Check.That(childComputeRoute.GrainPrefixExtension).IsNullOrEmpty();

            // Expect parent redirection after child
            Check.That(childComputeRoute.TargetGrain).IsNotNull().And.IsEqualTo(typeof(IThirdRedirectionTest));
        }

        #endregion
    }
}
