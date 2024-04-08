// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Disposables;

    using Orleans;
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class GrainFactoryScoped : SafeDisposable, IGrainFactoryScoped
    {
        #region Fields

        private static readonly Type[] s_solverSignatureParameters;

        private readonly Func<Type, IdSpan, string?, IAddressable?> _finalSolver;
        private readonly IVGrainRouteService _grainRouteService;
        private readonly IGrainFactory _parentGrainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="GrainFactoryScoped"/> class.
        /// </summary>
        static GrainFactoryScoped()
        {
            s_solverSignatureParameters = new Type[]
            {
                typeof(Type),
                typeof(IdSpan),
                typeof(string),
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainFactoryScoped"/> class.
        /// </summary>
        public GrainFactoryScoped(IGrainFactory parentGrainFactory, IVGrainRouteService grainRouteService)
        {
            this._grainRouteService = grainRouteService;
            this._parentGrainFactory = parentGrainFactory;

            if (parentGrainFactory is GrainFactoryScoped)
            {
                this._finalSolver = (type, id, grainClassNamePrefix) => ((GrainFactoryScoped)this._parentGrainFactory).GetGrainImpl(type, id, grainClassNamePrefix);
            }
            else
            {
                var grainImplMth = parentGrainFactory.GetType()
                                                     .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                     .FirstOrDefault(mth => mth.ReturnType == typeof(IAddressable) &&
                                                                            mth.GetParameters().Length == 3 &&
                                                                            mth.GetParameters().Select(p => p.ParameterType).SequenceEqual(s_solverSignatureParameters));

#pragma warning disable IDE0270 // Use coalesce expression
                if (grainImplMth is null)
                    throw new InvalidOperationException("Root grain factory must have a method able to solve IAddressable element from Type, IdSpan and string");
#pragma warning restore IDE0270 // Use coalesce expression

                this._finalSolver = (type, id, grainClassNamePrefix) => (IAddressable?)grainImplMth!.Invoke(this._parentGrainFactory, new object?[] { type, id, grainClassNamePrefix });
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainFactoryScoped"/> class.
        /// </summary>
        public GrainFactoryScoped(IGrainFactory parentGrainFactory, IEnumerable<VGrainRedirectionDefinition>? redirectionDefinitions)
            : this(parentGrainFactory, new GrainRouteFixedService(redirectionDefinitions))
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IGrainFactoryScoped ApplyRedirections(params VGrainRedirectionDefinition[] redirections)
        {
            return new GrainFactoryScoped(this, redirections);
        }

        /// <inheritdoc />
        public TGrainObserverInterface CreateObjectReference<TGrainObserverInterface>(IGrainObserver obj)
            where TGrainObserverInterface : IGrainObserver
        {
            return this._parentGrainFactory.CreateObjectReference<TGrainObserverInterface>(obj);
        }

        /// <inheritdoc />
        public void DeleteObjectReference<TGrainObserverInterface>(IGrainObserver obj)
            where TGrainObserverInterface : IGrainObserver
        {
            this._parentGrainFactory.DeleteObjectReference<TGrainObserverInterface>(obj);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithGuidKey
        {
            var idSpan = GrainIdKeyExtensions.CreateGuidKey(primaryKey);
            return (TGrainInterface)GetGrainImpl(typeof(TGrainInterface), idSpan, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithIntegerKey
        {
            var idSpan = GrainIdKeyExtensions.CreateIntegerKey(primaryKey);
            return (TGrainInterface)GetGrainImpl(typeof(TGrainInterface), idSpan, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(string primaryKey, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithStringKey
        {
            var idSpan = IdSpan.Create(primaryKey);
            return (TGrainInterface)GetGrainImpl(typeof(TGrainInterface), idSpan, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string keyExtension, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithGuidCompoundKey
        {
            var idSpan = GrainIdKeyExtensions.CreateGuidKey(primaryKey, keyExtension);
            return (TGrainInterface)GetGrainImpl(typeof(TGrainInterface), idSpan, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string keyExtension, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithIntegerCompoundKey
        {
            var idSpan = GrainIdKeyExtensions.CreateIntegerKey(primaryKey, keyExtension);
            return (TGrainInterface)GetGrainImpl(typeof(TGrainInterface), idSpan, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey)
        {
            var idSpan = GrainIdKeyExtensions.CreateGuidKey(grainPrimaryKey);
            return (IGrain)GetGrainImpl(grainInterfaceType, idSpan, null);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey)
        {
            var idSpan = GrainIdKeyExtensions.CreateIntegerKey(grainPrimaryKey);
            return (IGrain)GetGrainImpl(grainInterfaceType, idSpan, null);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, string grainPrimaryKey)
        {
            var idSpan = IdSpan.Create(grainPrimaryKey);
            return (IGrain)GetGrainImpl(grainInterfaceType, idSpan, null);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey, string keyExtension)
        {
            var idSpan = GrainIdKeyExtensions.CreateGuidKey(grainPrimaryKey, keyExtension);
            return (IGrain)GetGrainImpl(grainInterfaceType, idSpan, null);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey, string keyExtension)
        {
            var idSpan = GrainIdKeyExtensions.CreateIntegerKey(grainPrimaryKey, keyExtension);
            return (IGrain)GetGrainImpl(grainInterfaceType, idSpan, null);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     No redirection apply du to direct link attemps
        /// </remarks>
        public TGrainInterface GetGrain<TGrainInterface>(GrainId grainId)
            where TGrainInterface : IAddressable
        {
            return (TGrainInterface)this._parentGrainFactory.GetGrain<TGrainInterface>(grainId);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     No redirection apply du to direct link attemps
        /// </remarks>
        public IAddressable GetGrain(GrainId grainId)
        {
            return this._parentGrainFactory.GetGrain(grainId);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     No redirection apply du to direct link attemps
        /// </remarks>
        public IAddressable GetGrain(GrainId grainId, GrainInterfaceType interfaceType)
        {
            return this._parentGrainFactory.GetGrain(grainId, interfaceType);
        }

        #region Tools

        /// <summary>
        /// Gets the grain final call.
        /// </summary>
        private IAddressable GetGrainImpl(Type interfaceType, IdSpan grainKey, string? grainClassNamePrefix)
        {
            ArgumentNullException.ThrowIfNull(interfaceType);

            var route = this._grainRouteService.GetRoute(interfaceType, null, null, grainClassNamePrefix);

            return this._finalSolver(route.TargetGrain, grainKey, route.GrainPrefixExtension) ?? throw new InvalidOperationException("Could not solve a grain associate to contract " + route.TargetGrain + " ext('" + route.GrainPrefixExtension);
        }

        #endregion

        #endregion
    }
}
