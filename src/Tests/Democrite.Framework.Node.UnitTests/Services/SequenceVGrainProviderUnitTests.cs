// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Services
{
    using AutoFixture;

    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Node.UnitTests.Tools;
    using Democrite.UnitTests.ToolKit.Services;

    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="SequenceVGrainProvider"/>
    /// </summary>
    public sealed class SequenceVGrainProviderUnitTests
    {
        #region Fields
        
        private readonly Fixture _fixture;
        
        #endregion

        #region ReflectionAccess

        //   _grainRouteService = {Democrite.Framework.Node.Services.GrainRouteFixedService
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_grainRouteService")]
        private static extern ref IVGrainRouteService GetRouteRedirectionField(GrainFactoryScoped factoryScope);

        //_redirectionDefinitions
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_redirectionDefinitions")] 
        private static extern ref IReadOnlyDictionary<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>> GetIndexedRouteRedirectionField(GrainRouteFixedService routeFixedService);

        //  private readonly IGrainFactory _grainFactory;
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_grainFactory")]
        private static extern ref IGrainFactory GetGrainFactory(VGrainProvider grainProvder);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceVGrainProviderUnitTests"/> class.
        /// </summary>
        public SequenceVGrainProviderUnitTests()
        {
            this._fixture = ObjectTestHelper.PrepareFixture();
        }

        #endregion

        #region NestedType

        internal interface IBaseVGrain : IVGrain
        {

        }

        internal interface IInitVGrain : IBaseVGrain
        {

        }

        internal interface IInitBisVGrain : IBaseVGrain
        {

        }

        internal interface IInitTierVGrain : IBaseVGrain
        {

        }

        internal interface IInitLastVGrain : IBaseVGrain
        {

        }

        internal interface IRedirectAVGrain : IInitVGrain, IInitBisVGrain, IInitTierVGrain, IInitLastVGrain
        {

        }

        #endregion

        #region Method

        [Fact]
        public void Ctor_Dispose()
        {
            var defaultGrainProviderMock = Substitute.For<IVGrainProvider>();
            var defaultVGrainIdFactory = Substitute.For<IVGrainIdFactory>();
            var defaultGrainFactory = Substitute.For<IGrainFactory>();
            var grainRouteService = Substitute.For<IVGrainRouteService>();

            using (var prov = new SequenceVGrainProvider(defaultGrainProviderMock, defaultGrainFactory, defaultVGrainIdFactory, grainRouteService))
            {

            }
        }

        [Fact]
        public void No_Redirection()
        {
            var defaultGrainProviderMock = Substitute.For<IVGrainProvider>();
            var defaultVGrainIdFactory = Substitute.For<IVGrainIdFactory>();
            var defaultGrainFactory = Substitute.For<IGrainFactory>();
            var grainRouteService = Substitute.For<IVGrainRouteService>();

            using (var prov = new SequenceVGrainProvider(defaultGrainProviderMock, defaultGrainFactory, defaultVGrainIdFactory, grainRouteService))
            {
                SequenceStageDefinition stepDefinition = this._fixture.Create<TestSequenceStageDefinition>();

                // Exec
                var grainProvider = prov.GetGrainProvider(ref stepDefinition);

                Check.That(grainProvider).IsNotNull().And.IsSameReferenceAs(defaultGrainProviderMock);
            }
        }

        [Fact]
        public async Task One_Redirection()
        {
            await Redirection_Tester(

                    // Step specific
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitVGrain, IRedirectAVGrain>())

                    );
        }

        [Fact]
        public async Task Only_Global_Redirection()
        {
            await Redirection_Tester(

                    // Step specific
                    new StageVGrainRedirectionDescription(null, VGrainInterfaceRedirectionDefinition.Create<IInitVGrain, IRedirectAVGrain>())

                    );
        }

        [Fact]
        public async Task One_Redirection_With_Global()
        {
            await Redirection_Tester(

                    // Step specific
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitVGrain, IRedirectAVGrain>()),

                    // Global
                    new StageVGrainRedirectionDescription(null, VGrainInterfaceRedirectionDefinition.Create<IInitBisVGrain, IRedirectAVGrain>())
                    );
        }

        [Fact]
        public async Task Multi_Stage_Redirection()
        {
            await Redirection_Tester(

                    // Step specific
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitVGrain, IRedirectAVGrain>()),

                    // other step Global
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitBisVGrain, IRedirectAVGrain>())
                    );
        }

        [Fact]
        public async Task Multi_Stage_Redirection_With_Global()
        {
            await Redirection_Tester(

                    // Step specific
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitVGrain, IRedirectAVGrain>()),

                    // other step Global
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitBisVGrain, IRedirectAVGrain>()),

                    // Global
                    new StageVGrainRedirectionDescription(null, VGrainInterfaceRedirectionDefinition.Create<IInitTierVGrain, IRedirectAVGrain>())

                    );
        }

        [Fact]
        public async Task Multi_Stage_Redirection_With_Global_Partial()
        {
            await Redirection_Tester(

                    // Step specific
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitVGrain, IRedirectAVGrain>()),

                    // other step Global
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitBisVGrain, IRedirectAVGrain>()),

                    // Global to be include only on the first step
                    new StageVGrainRedirectionDescription(null, VGrainInterfaceRedirectionDefinition.Create<IInitBisVGrain, IRedirectAVGrain>())

                    );
        }

        [Fact]
        public async Task Multi_Stage_Redirection_With_Global_Partial_And_Mutiple_Same_Stage()
        {
            var commonStage = Guid.NewGuid();

            await Redirection_Tester(

                    // Step specific
                    new StageVGrainRedirectionDescription(commonStage, VGrainInterfaceRedirectionDefinition.Create<IInitVGrain, IRedirectAVGrain>()),
                    new StageVGrainRedirectionDescription(commonStage, VGrainInterfaceRedirectionDefinition.Create<IInitTierVGrain, IRedirectAVGrain>()),

                    // other step Global
                    new StageVGrainRedirectionDescription(Guid.NewGuid(), VGrainInterfaceRedirectionDefinition.Create<IInitBisVGrain, IRedirectAVGrain>()),

                    // Global to be include only on the first step
                    new StageVGrainRedirectionDescription(null, VGrainInterfaceRedirectionDefinition.Create<IInitBisVGrain, IRedirectAVGrain>()),

                    // Global to be include only on the second step
                    new StageVGrainRedirectionDescription(null, VGrainInterfaceRedirectionDefinition.Create<IInitTierVGrain, IRedirectAVGrain>()),

                    // Global
                    new StageVGrainRedirectionDescription(null, VGrainInterfaceRedirectionDefinition.Create<IInitLastVGrain, IRedirectAVGrain>())

                    );
        }

        #region Tools

        /// <summary>
        ///  Main tester used to validate redirection generated correspond to description inserted
        /// </summary>
        private async Task Redirection_Tester(params StageVGrainRedirectionDescription[] redirections)
        {
            var defaultGrainProviderMock = Substitute.For<IVGrainProvider>();
            var defaultVGrainIdFactory = Substitute.For<IVGrainIdFactory>();
            var grainRouteService = Substitute.For<IVGrainRouteService>();

            var defaultGrainFactory = new UnitTestGrainFactory();

            using (var prov = new SequenceVGrainProvider(defaultGrainProviderMock, defaultGrainFactory, defaultVGrainIdFactory, grainRouteService))
            {
                var executionContext = Substitute.For<IExecutionContext>();
                var diagnosticLogger = Substitute.For<IDiagnosticLogger>();

                // Prepare redirection
                Check.That(prov.IsInitialized).IsFalse();

                var customization = new ExecutionCustomizationDescriptions()
                {
                    VGrainRedirection = redirections
                };

                await prov.InitializationAsync(customization);

                Check.That(prov.IsInitialized).IsTrue();

                // Exec

                var uniqueStageRedirectionToTest = redirections.GroupBy(k => k.StageUid ?? Guid.Empty)
                                                               .ToDictionary(k => k.Key, v => v.Select(vv => vv.RedirectionDefinition).ToReadOnly());

                // Add global test if missing
                if (!uniqueStageRedirectionToTest.ContainsKey(Guid.Empty))
                    uniqueStageRedirectionToTest.Add(Guid.Empty, EnumerableHelper<VGrainRedirectionDefinition>.ReadOnly);

                var globalRedirections = uniqueStageRedirectionToTest[Guid.Empty];
                var indexedGlobalRedirections = globalRedirections.ToDictionary(k => k.Source);

                foreach (var stage in uniqueStageRedirectionToTest)
                {
                    var stageId = (stage.Key == Guid.Empty) ? Guid.NewGuid() : stage.Key;
                    SequenceStageDefinition toRedirectStepDefinition = TestSequenceStageDefinition.Create(this._fixture, stageId);

                    var grainProvider = prov.GetGrainProvider(ref toRedirectStepDefinition);

                    bool expectDefault = stage.Value.Count == 0;
                    if (expectDefault)
                    {
                        Check.That(grainProvider).IsNotNull().And.IsSameReferenceAs(defaultGrainProviderMock);
                        continue;
                    }

                    Check.That(grainProvider).IsNotNull()
                                             .And.Not.IsSameReferenceAs(defaultGrainProviderMock)
                                             .And.IsInstanceOf<VGrainProvider>();

                    var grainProviderTyped = (VGrainProvider)grainProvider;

                    // Check that factory used is correctly a redirection type
                    var factoryInProvider = GetGrainFactory(grainProviderTyped!);
                    Check.That(factoryInProvider).IsNotNull().And.IsInstanceOf<GrainFactoryScoped>();

                    // Use all redirection
                    var redirectionToTest = stage.Value.ToList();

                    var globalReditionsToInjectKeys = indexedGlobalRedirections.Keys.Except(stage.Value.Select(v => v.Source).Distinct());

                    foreach (var key in globalReditionsToInjectKeys)
                    {
                        redirectionToTest.Add(indexedGlobalRedirections[key]);
                    }

                    CheckRedirectionApplyed((GrainFactoryScoped)factoryInProvider, redirectionToTest);
                }
            }
        }

        /// <summary>
        /// Check if expected redirection are repected
        /// </summary>
        private static void CheckRedirectionApplyed(GrainFactoryScoped redirectionFactory, IReadOnlyCollection<VGrainRedirectionDefinition> expectedRedirections)
        {
            var interfaceRoute = GetRouteRedirectionField(redirectionFactory);

            Check.That(interfaceRoute).IsNotNull().And.IsInstanceOf<GrainRouteFixedService>();

            var index = GetIndexedRouteRedirectionField((GrainRouteFixedService)interfaceRoute);
            Check.That(index).IsNotNull().And.CountIs(expectedRedirections.Count);

            var indexExpected = expectedRedirections.ToDictionary(k => k.Source);

            foreach (var kv in indexExpected)
            {
                Check.That(index!.ContainsKey(kv.Key)).IsTrue();
                var result = index[kv.Key];

                Check.That(result).IsNotNull().And.CountIs(1);
                var first = result.First();

                Check.That(first.Item1).IsNotNull().And.IsSameReferenceAs(kv.Value);
            }
        }

        #endregion

        #endregion
    }
}
