// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Abstractions.Disposables;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    internal abstract class GrainRouteBaseService : SafeDisposable, IVGrainRouteService
    {
        #region Fields

        private static readonly IReadOnlyDictionary<VGrainRedirectionTypeEnum, Func<VGrainRedirectionDefinition, Type, object?, IExecutionContext?, string?, (Type? GrainInterface, string? GrainClassNamePrefix)>> s_redirectionSolver;

        private readonly Dictionary<RouteKey, Tuple<Type, string>> _callCache;
        private readonly ReaderWriterLockSlim _cacheLockSlim;

        private IVGrainRouteService? _parentRouteService;
        private string? _localParentCacheEtag;
        private string _cacheEtag;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="GrainRouteBaseService"/> class.
        /// </summary>
        static GrainRouteBaseService()
        {
            s_redirectionSolver = new Dictionary<VGrainRedirectionTypeEnum, Func<VGrainRedirectionDefinition, Type, object?, IExecutionContext?, string?, (Type? GrainInterface, string? GrainClassNamePrefix)>>()
            {
                { VGrainRedirectionTypeEnum.OtherInterface, (redirection, sourceType, input, ctx, grainClassNamePrefix) => (((VGrainInterfaceRedirectionDefinition)redirection).Redirect.ToType(), string.Empty) },
                { VGrainRedirectionTypeEnum.ClassPrefixName, (redirection, sourceType, input, ctx, grainClassNamePrefix) => (sourceType, ((VGrainClassNameRedirectionDefinition)redirection).RedirectClassName) },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainRouteFixedService"/> class.
        /// </summary>
        public GrainRouteBaseService(IVGrainRouteService? parentRouteService)
        {
            this._parentRouteService = parentRouteService;

            this._callCache = new Dictionary<RouteKey, Tuple<Type, string>>();
            this._cacheLockSlim = new ReaderWriterLockSlim();

            this._cacheEtag = Guid.NewGuid().ToString();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string CacheEtag
        {
            get
            {
                this._cacheLockSlim.EnterReadLock();
                try
                {
                    return this._cacheEtag;
                }
                finally
                {
                    this._cacheLockSlim.ExitReadLock();
                }
            }
        }

        #endregion

        #region Nested

        protected record struct RouteKey(Type OriginTarget, string? GrainClassNamePrefix);

        #endregion

        #region Methods

        /// <inheritdoc />
        public IVGrainRouteScopedService CreateScope(IReadOnlyCollection<VGrainRedirectionDefinition> localRedirectionRules)
        {
            return new GrainRouteFixedService(localRedirectionRules, this);
        }

        /// <inheritdoc />
        public (Type TargetGrain, string? GrainPrefixExtension, bool Cachable) GetRoute(Type originTarget, object? input, IExecutionContext? context, string? grainClassNamePrefix)
        {
            var result = (originTarget, grainClassNamePrefix, true);

            // Copy in local parent variable to ensure no null exeception between the moment it is check and the usage
            var parent = this._parentRouteService;
            var parentEtag = parent?.CacheEtag;
            if (parent is not null && this._localParentCacheEtag != parentEtag)
            {
                ClearLocalRouteCache();
                this._localParentCacheEtag = parentEtag;
            }
            else if (TryGetRouteFromCache(originTarget, grainClassNamePrefix, out var cacheResult) && cacheResult is not null)
            {
                return (cacheResult.Item1, cacheResult.Item2, true);
            }

            var redirectionsIndex = GetRedirections();

            if (redirectionsIndex is not null && redirectionsIndex.Any())
            {
                var finalInterfaceType = originTarget;

                var abstrInterfaceType = originTarget.GetAbstractType();

                if (abstrInterfaceType is ConcretType concret && redirectionsIndex.TryGetValue(concret, out var redirections))
                {
                    foreach (var redirection in redirections)
                    {
                        // Item1 = Redirection definition
                        // Item2 = Condition

                        // Apply redirection comparaison
                        if (redirection.Item2 != null && redirection.Item2(originTarget, input, context, grainClassNamePrefix) == false)
                            continue;

                        if (s_redirectionSolver.TryGetValue(redirection.Item1.Type, out var resolver))
                        {
                            // Tolerate redirection to return default to notify no valid redirection
                            var redirectionSolved = resolver(redirection.Item1, originTarget, input, context, grainClassNamePrefix);

                            if (redirectionSolved.GrainInterface is not null)
                            {
                                result = (redirectionSolved!.GrainInterface, redirectionSolved.GrainClassNamePrefix, redirection.Item2 is null);
                                break;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Redirection type is not managed " + redirection.Item1);
                        }
                    }
                }
            }

            var finalResult = parent?.GetRoute(result.originTarget, input, context, result.grainClassNamePrefix) ?? result;

            if (finalResult.Cachable)
                SetToCache(originTarget, grainClassNamePrefix, finalResult);

            return finalResult;
        }

        #region Tools

        /// <summary>
        /// Builds the redirections.
        /// </summary>
        protected static IReadOnlyDictionary<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>> BuildRedirections(IEnumerable<VGrainRedirectionDefinition>? redirectionDefinitions)
        {
            var localRedirectionDefinitions = redirectionDefinitions?.GroupBy(k => k.Source)
                                                         .ToDictionary(k => k.Key,
                                                                       kv => kv.Select(def => Tuple.Create(def,
                                                                                                           (Func<Type, object?, IExecutionContext?, string?, bool>?)def.RedirectionCondition?.ToExpressionDelegateWithResult()?.Compile()))
                                                                               .ToReadOnly());

            return localRedirectionDefinitions ?? DictionaryHelper<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>>.ReadOnly;
        }

        /// <summary>
        /// Gets the redirectionsIndex information
        /// </summary>
        protected abstract IReadOnlyDictionary<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>> GetRedirections();

        /// <summary>
        /// Tries the get route from cache.
        /// </summary>
        protected virtual bool TryGetRouteFromCache(Type originTarget, string? grainClassNamePrefix, out Tuple<Type, string?>? result)
        {
            result = null;

            this._cacheLockSlim.EnterReadLock();
            try
            {
                if (this._callCache.TryGetValue(new RouteKey(originTarget, grainClassNamePrefix), out var cacheResult))
                {
                    result = cacheResult!;
                    return true;
                }
            }
            finally
            {
                this._cacheLockSlim.ExitReadLock();
            }

            return false;
        }

        /// <summary>
        /// Sets result to cache.
        /// </summary>
        protected virtual void SetToCache(Type originTarget, string? grainClassNamePrefix, (Type Target, string? GrainClassNamePrefix, bool Cachable) result)
        {
            this._cacheLockSlim.EnterWriteLock();
            try
            {
                this._callCache[new RouteKey(originTarget, grainClassNamePrefix)] = Tuple.Create(result.Target, result.GrainClassNamePrefix)!;
            }
            finally
            {
                this._cacheLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears the local route cache.
        /// </summary>
        protected virtual void ClearLocalRouteCache()
        {
            this._cacheLockSlim.EnterWriteLock();
            try
            {
                this._callCache.Clear();
                this._cacheEtag = Guid.NewGuid().ToString();
            }
            finally
            {
                this._cacheLockSlim.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            this._parentRouteService = null;
            ClearLocalRouteCache();
            base.DisposeBegin();
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            this._cacheLockSlim.Dispose();
            base.DisposeEnd();
        }

        #endregion

        #endregion

    }
}
