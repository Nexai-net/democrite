// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Extensions
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Services;
    using Democrite.UnitTests.ToolKit.Helpers;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;

    using NSubstitute;

    using Orleans.Runtime;

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extesions method that help auto fixture to generate grain
    /// </summary>
    public static class AutoFixtureExtensions
    {
        #region Fields

        private static readonly MethodInfo s_runtimeContextResetStaticMthd;
        private static readonly MethodInfo s_runtimeContextSetStaticMthd;
        private static readonly MethodInfo s_grainReferenceFromGrainId;

        #endregion

        /// <summary>
        /// Initializes the <see cref="AutoFixtureExtensions"/> class.
        /// </summary>
        static AutoFixtureExtensions()
        {
            try
            {
                var runtimeCtxType = Assembly.GetAssembly(typeof(Orleans.Runtime.IGrainContext))!.GetType("Orleans.Runtime.RuntimeContext", true, true);
                Debug.Assert(runtimeCtxType != null);

                var runtimeContextSetStaticMthd = runtimeCtxType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                                                .First(m => m.Name == "SetExecutionContext" && m.GetParameters().Length == 1);
                Debug.Assert(runtimeContextSetStaticMthd != null);

                s_runtimeContextSetStaticMthd = runtimeContextSetStaticMthd;

                var runtimeContextResetStaticMthd = runtimeCtxType.GetMethod("ResetExecutionContext", BindingFlags.NonPublic | BindingFlags.Static);
                Debug.Assert(runtimeContextResetStaticMthd != null);

                s_runtimeContextResetStaticMthd = runtimeContextResetStaticMthd;

                var grainReferenceFromGrainId = typeof(GrainReference).GetMethod("FromGrainId", BindingFlags.Static | BindingFlags.NonPublic);
                Debug.Assert(grainReferenceFromGrainId != null);

                s_grainReferenceFromGrainId = grainReferenceFromGrainId;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                throw;
            }
        }

        /// <summary>
        /// Creates the grain
        /// </summary>
        public static TVGrain CreateVGrain<TVGrain>(this IFixture fixture,
                                                    IPersistentStateFactory? persistentStateFactory = null,
                                                    Action<IServiceCollection>? setupServiceCollection = null,
                                                    GrainId? forcedGrainId = null,
                                                    IGrainFactory? factory = null)
            where TVGrain : IVGrain
        {
            var serviceCollection = new ServiceCollection();

            setupServiceCollection?.Invoke(serviceCollection);

            var runtime = Substitute.For<IGrainRuntime>();

            factory ??= Substitute.For<IGrainFactory>();
            runtime.GrainFactory.Returns(factory);

            var orleanFactory = new GrainOrleanFactory(factory);
            serviceCollection.AddSingleton(factory);
            serviceCollection.AddSingleton<IGrainOrleanFactory>(orleanFactory);
            serviceCollection.AddSingleton(runtime);

            fixture.Register<IGrainFactory>(() => factory);
            fixture.Register<IGrainOrleanFactory>(() => orleanFactory);

            var provider = serviceCollection.BuildServiceProvider();

            runtime.ServiceProvider.Returns(provider);

            //RuntimeContext
            var context = Substitute.For<IGrainContext>();
            context.ActivationServices.Returns(provider);

            forcedGrainId ??= fixture.Create<GrainId>();
            context.GrainId.Returns<GrainId>(forcedGrainId.Value);

            var grainRefShared = new GrainReferenceShared(forcedGrainId.Value.Type,
                                                          new GrainInterfaceType(typeof(TVGrain).Name),
                                                          (ushort)1,
                                                          Substitute.For<IGrainReferenceRuntime>(),
                                                          Orleans.CodeGeneration.InvokeMethodOptions.OneWay,
                                                          null,
                                                          null,
                                                          provider);

            var grainRef = (GrainReference)s_grainReferenceFromGrainId.Invoke(null, new object[] { grainRefShared, forcedGrainId })!;

            context.GrainReference.Returns(grainRef);

            TVGrain instance;
            s_runtimeContextSetStaticMthd.Invoke(null, new object[] { context });
            try
            {
                instance = fixture.Create<TVGrain>();
            }
            finally
            {
                s_runtimeContextResetStaticMthd.Invoke(null, EnumerableHelper<object?>.ReadOnlyArray);
            }

            return instance;
        }

        /// <summary>
        /// Creates and activate the grain
        /// </summary>
        public static async ValueTask<TVGrain> CreateAndInitVGrain<TVGrain>(this IFixture fixture,
                                                                            IPersistentStateFactory? persistentStateFactory = null,
                                                                            Action<IServiceCollection>? setupServiceCollection = null,
                                                                            GrainId? forcedGrainId = null,
                                                                            IGrainFactory? factory = null)
            where TVGrain : IVGrain, IGrainBase
        {
            var instance = CreateVGrain<TVGrain>(fixture,
                                                 persistentStateFactory,
                                                 setupServiceCollection,
                                                 forcedGrainId,
                                                 factory);

            await InitVGrain(fixture, instance);

            return instance;
        }

        /// <summary>
        /// Creates and activate the grain
        /// </summary>
        public static async ValueTask<TVGrain> InitVGrain<TVGrain>(this IFixture fixture, TVGrain instance)
            where TVGrain : IVGrain, IGrainBase
        {
            if (instance is ILifecycleParticipant<IGrainLifecycle> grainLifestyle)
            {
                var lifeStyleObserver = new TestGrainLifecycle(NullLogger.Instance);
                grainLifestyle.Participate(lifeStyleObserver);

                await lifeStyleObserver.OnStart();
            }
            else
            {
                await instance.OnActivateAsync(default);
            }

            return instance;
        }
    }
}
