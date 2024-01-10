// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Configurations
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Configurations;
    using Democrite.Framework.Node.UnitTests.Tools;
    using Democrite.UnitTests.ToolKit.VGrains.Transformers;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using Moq;

    using NFluent;

    using Orleans.Runtime;
    using Orleans.Serialization;
    using Orleans.Serialization.Configuration;

    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Unit test <see cref="ClusterNodeVGrainBuilder"/>
    /// </summary>
    public sealed class ClusterNodeVGrainBuilderUnitTest
    {
        #region Fields

        private static readonly IReadOnlyCollection<Regex> s_testToolKitAssemblyDetector;
        private static readonly IDemocriteNodeWizard s_mockRoot;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        static ClusterNodeVGrainBuilderUnitTest()
        {
            s_testToolKitAssemblyDetector = new[]
            {
                new Regex(",\\sDemocrite.UnitTests.ToolKit,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            };

            s_mockRoot = new Mock<IDemocriteNodeWizard>(MockBehavior.Strict).Object;
        }

        #endregion

        #region Nested
        //public interface IClusterNodeVGrainBuilderTestVGrain : IGrain
        //{
        //}

        //public sealed class ClusterNodeVGrainBuilderTestVGrain : IClusterNodeVGrainBuilderTestVGrain
        //{
        //}

        //public class CustomTypeManifestOptionProvider : ITypeManifestProvider, IConfigureOptions<TypeManifestOptions>
        //{
        //    #region Fields

        //    private readonly Dictionary<Type, Type> _vgrains;

        //    #endregion

        //    #region Ctor

        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="CustomTypeManifestOptionProvider"/> class.
        //    /// </summary>
        //    public CustomTypeManifestOptionProvider()
        //    {
        //        this._vgrains = new Dictionary<Type, Type>();
        //    }

        //    #endregion

        //    #region Methods

        //    public void Add<TInterface, TVGrain>()
        //        where TVGrain : TInterface
        //    {
        //        this._vgrains.Add(typeof(TInterface), typeof(TVGrain));
        //    }

        //    public void Configure(TypeManifestOptions options)
        //    {
        //        foreach (var agt in this._vgrains)
        //        {
        //            options.Interfaces.Add(agt.Key);
        //            options.InterfaceImplementations.Add(agt.Value);
        //        }
        //    }

        //    #endregion
        //}

        ///// <summary>
        /////  <see cref="IConfigureOptions{TypeManifestOptions}"/> is register on the DI (Dependency Injection) type to be instanciated 
        ///// </summary>
        //public class OutSideServiceCustomTypeManifestOptionProvider : CustomTypeManifestOptionProvider
        //{
        //    public OutSideServiceCustomTypeManifestOptionProvider()
        //    {
        //        Add<IServiceCollection, ServiceCollection>();
        //    }
        //}

        #endregion

        #region Methods

        /// <summary>
        /// Check if <see cref="TypeManifestOptionsComparer"/> work fine
        /// </summary>
        [Fact]
        public void Check_TypeManifestOptionsComparer()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSerializer();

            var builder = new ClusterNodeVGrainBuilder(s_mockRoot);

            var option = builder.Setup(serviceCollection);

            // Restore service collection because ClusterNodeVGrainBuilder.Setup remove other ITypeManifestProvider to ensure control
            var serviceCollectionOther = new ServiceCollection();
            serviceCollectionOther.AddSerializer();
            var option2 = builder.Setup(serviceCollectionOther);

            Check.That(option).IsNotNull();

            // Test Comparer
            Check.That(option).IsEqualTo(option, TypeManifestOptionsComparer.Default);
            Check.That(option).IsEqualTo(option2, TypeManifestOptionsComparer.Default);
            Check.That(option).Not.IsSameReferenceAs(option2);

            Check.That(option).IsEqualTo(option, TypeManifestOptionsComparer.Default);
            Check.That(option).IsEqualTo(option2, TypeManifestOptionsComparer.Default);

            option2.AllowAllTypes = !option2.AllowAllTypes;

            Check.That(option).IsEqualTo(option, TypeManifestOptionsComparer.Default);
            Check.That(option).Not.IsEqualTo(option2, TypeManifestOptionsComparer.Default);

            // Restore
            option2.AllowAllTypes = !option2.AllowAllTypes;

            Check.That(option).IsEqualTo(option, TypeManifestOptionsComparer.Default);
            Check.That(option).IsEqualTo(option2, TypeManifestOptionsComparer.Default);

            var collectionProps = typeof(TypeManifestOptions).GetProperties()
                                                             .Where(p => p.PropertyType == typeof(HashSet<Type>))
                                                             .ToArray();

            foreach (var prop in collectionProps)
            {
                Check.That(option).IsEqualTo(option, TypeManifestOptionsComparer.Default);
                Check.That(option).IsEqualTo(option2, TypeManifestOptionsComparer.Default);

                var contains = prop.GetValue(option2) as HashSet<Type>;

                Check.That(contains).IsNotNull();

                ArgumentNullException.ThrowIfNull(contains);

                if (contains.Any())
                {
                    var last = contains.Last();
                    contains.Remove(last);

                    Check.That(option).Not.IsSameReferenceAs(option2);
                    Check.That(option).Not.IsEqualTo(option2, TypeManifestOptionsComparer.Default);

                    contains.Add(last);

                    Check.That(option).Not.IsSameReferenceAs(option2);
                    Check.That(option).IsEqualTo(option2, TypeManifestOptionsComparer.Default);
                }
                else
                {
                    var type = typeof(ClusterNodeVGrainBuilderUnitTest);
                    contains.Add(type);

                    Check.That(option).Not.IsSameReferenceAs(option2);
                    Check.That(option).Not.IsEqualTo(option2, TypeManifestOptionsComparer.Default);

                    contains.Remove(type);

                    Check.That(option).Not.IsSameReferenceAs(option2);
                    Check.That(option).IsEqualTo(option2, TypeManifestOptionsComparer.Default);
                }
            }
        }

        /// <summary>
        /// Check that  orlean - democrite system/framework vgrain are protected from <see cref="ClusterNodeVGrainBuilder.RemoveAllAutoDiscoveryVGrain"/>.
        /// </summary>
        [Fact]
        public void Check_ThatSystemFrameworkVGrain_AreProtected_FromRemoveAll()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSerializer();
            RemoveTestToolKitServiceProviders(serviceCollection);

            var builder = new ClusterNodeVGrainBuilder(s_mockRoot);

            var reference = builder.Setup(serviceCollection);

            // Restore service collection because ClusterNodeVGrainBuilder.Setup remove other ITypeManifestProvider to ensure control
            var serviceCollectionOther = new ServiceCollection();
            serviceCollectionOther.AddSerializer();
            RemoveTestToolKitServiceProviders(serviceCollectionOther);

            var removeBuilder = new ClusterNodeVGrainBuilder(s_mockRoot);

            removeBuilder.RemoveAllAutoDiscoveryVGrain();

            var afterRemoveAll = removeBuilder.Setup(serviceCollectionOther);

            var removedImplementationThatIsNotTest = reference.InterfaceImplementations.Except(afterRemoveAll.InterfaceImplementations)
                                                              .Where(implType => implType != null &&
                                                                                 !Regex.IsMatch(implType.Assembly.GetName().Name!, "^Democrite\\.Test([a-zA-Z0-9.]+)$"))
                                                              .ToArray();

            Check.That(removedImplementationThatIsNotTest).IsEmpty();
        }

        /// <summary>
        /// Check <see cref="ClusterNodeVGrainBuilder.Remove(Type)"/> pnly the requested vgrain implementation.
        /// </summary>
        [Fact]
        public void Check_ClusterNodeVGrainBuilder_Remove()
        {
            var serviceCollection = new ServiceCollection();

            // Need to setup a type as parameters
            serviceCollection.AddSerializer();

            var builder = new ClusterNodeVGrainBuilder(s_mockRoot);

            var reference = builder.Setup(serviceCollection);

            // Check that test service have been correctly added
            Check.That(reference.Interfaces).Contains(typeof(ITestExtractEmailTransformer));
            Check.That(reference.InterfaceImplementations).Contains(typeof(TestExtractEmailTransformer));

            Check.That(reference.InterfaceImplementations).Contains(typeof(GrainReference));

            // Restore service collection because ClusterNodeVGrainBuilder.Setup remove other ITypeManifestProvider to ensure control
            var serviceCollectionOther = new ServiceCollection();
            // Need to setup a type as parameters
            serviceCollectionOther.AddSerializer();

            var removeBuilder = new ClusterNodeVGrainBuilder(s_mockRoot);

            Check.ThatCode(() => removeBuilder.Remove(null)).Throws<ArgumentNullException>().WithProperty(p => p.ParamName, "vgrain");

            removeBuilder.Remove<TestExtractEmailTransformer>();

            // Try remove system/framework
            removeBuilder.Remove(typeof(GrainReference));

            var afterRemoveAll = removeBuilder.Setup(serviceCollectionOther);

            Check.That(reference).Not.IsEqualTo(afterRemoveAll, TypeManifestOptionsComparer.Default);

            Check.That(afterRemoveAll.Interfaces).Contains(typeof(ITestExtractEmailTransformer));
            Check.That(afterRemoveAll.InterfaceImplementations).Not.Contains(typeof(TestExtractEmailTransformer));

            // Check that system/framework are not removed
            Check.That(afterRemoveAll.InterfaceImplementations).Contains(typeof(GrainReference));
        }

        /// <summary>
        /// Check <see cref="ClusterNodeVGrainBuilder.Add{TVGrain}()"/> pnly the requested vgrain implementation.
        /// </summary>
        [Fact]
        public void Check_ClusterNodeVGrainBuilder_Add()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSerializer();
            RemoveTestToolKitServiceProviders(serviceCollection);

            var builder = new ClusterNodeVGrainBuilder(s_mockRoot);

            var reference = builder.Setup(serviceCollection);

            // Check that test service have been correctly added
            Check.That(reference.InterfaceImplementations).Contains(typeof(GrainReference));

            Check.That(reference.Interfaces).Not.Contains(typeof(ITestExtractEmailTransformer));
            Check.That(reference.InterfaceImplementations).Not.Contains(typeof(TestExtractEmailTransformer));

            // Restore service collection because ClusterNodeVGrainBuilder.Setup remove other ITypeManifestProvider to ensure control
            var serviceCollectionOther = new ServiceCollection();
            serviceCollectionOther.AddSerializer();
            RemoveTestToolKitServiceProviders(serviceCollection);

            var removeBuilder = new ClusterNodeVGrainBuilder(s_mockRoot);

            Check.ThatCode(() => removeBuilder.Add(null, typeof(ServiceCollection))).Throws<ArgumentNullException>().WithProperty(p => p.ParamName, "vgrainInterface");
            Check.ThatCode(() => removeBuilder.Add(typeof(IServiceCollection), null)).Throws<ArgumentNullException>().WithProperty(p => p.ParamName, "vgrainImplementation");

            removeBuilder.Add<ITestExtractEmailTransformer, TestExtractEmailTransformer>();

            var afterAdd = removeBuilder.Setup(serviceCollectionOther);

            Check.That(reference).Not.IsEqualTo(afterAdd, TypeManifestOptionsComparer.Default);

            Check.That(afterAdd.Interfaces).Contains(typeof(ITestExtractEmailTransformer));
            Check.That(afterAdd.InterfaceImplementations).Contains(typeof(TestExtractEmailTransformer));
        }

        /// <summary>
        /// Check that <see cref="ClusterNodeVGrainBuilder.RemoveAllAutoDiscoveryVGrain"/> remove unprotected vgrain.
        /// </summary>
        [Fact]
        public void Check_FromRemoveAll_ExceptProtected()
        {
            var serviceCollection = new ServiceCollection();
            // Need to setup a type as parameters
            serviceCollection.AddSerializer();

            var builder = new ClusterNodeVGrainBuilder(s_mockRoot);

            var reference = builder.Setup(serviceCollection);

            // Check that test service have been correctly added
            Check.That(reference.Interfaces).Contains(typeof(ITestExtractEmailTransformer));
            Check.That(reference.InterfaceImplementations).Contains(typeof(TestExtractEmailTransformer));

            Check.That(reference.InterfaceImplementations).Contains(typeof(GrainReference));

            // Restore service collection because ClusterNodeVGrainBuilder.Setup remove other ITypeManifestProvider to ensure control
            var serviceCollectionOther = new ServiceCollection();
            serviceCollectionOther.AddSerializer();

            var removeBuilder = new ClusterNodeVGrainBuilder(s_mockRoot);

            removeBuilder.RemoveAllAutoDiscoveryVGrain();

            var afterRemoveAll = removeBuilder.Setup(serviceCollectionOther);

            // Still contains the interface declaration
            Check.That(afterRemoveAll.Interfaces).Contains(typeof(ITestExtractEmailTransformer));

            // The dedicated implementation for this cluster have been removed
            Check.That(afterRemoveAll.InterfaceImplementations).Not.Contains(typeof(TestExtractEmailTransformer));
            Check.That(afterRemoveAll.InterfaceImplementations).Contains(typeof(GrainReference));

            Check.That(reference).Not.IsEqualTo(afterRemoveAll, TypeManifestOptionsComparer.Default);
        }

        /// <summary>
        /// Removes the test tool kit from service providers.
        /// </summary>
        private static void RemoveTestToolKitServiceProviders(ServiceCollection serviceCollectionOther)
        {
            var testProviders = serviceCollectionOther.Where(s => s.ServiceType == typeof(IConfigureOptions<TypeManifestOptions>) &&
                                                                  s.ImplementationType != null &&
                                                                  s_testToolKitAssemblyDetector.Any(r => r.IsMatch(s.ImplementationType.AssemblyQualifiedName ?? "NoneType")))
                                                      .ToArray();

            if (testProviders.Any())
            {
                foreach (var prov in testProviders)
                    serviceCollectionOther.Remove(prov);
            }
        }

        #endregion
    }
}
