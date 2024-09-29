// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.References
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes.MetaData;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.References;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;

    using Elvex.Toolbox.Abstractions.Attributes;
    using Elvex.Toolbox.Abstractions.Comparers;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Runtime.Services;
    using Orleans.Services;

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to solve the <see cref="Uri"/> RefId
    /// </summary>
    /// <remarks>
    ///     Unit TEST are on the project "Democrite.Framework.Core.CodeGenerator.UnitTests" due to refId test environement already setups
    /// </remarks>
    internal sealed class DemocriteReferenceSolverService : SafeDisposable, IDemocriteReferenceSolverService
    {
        #region Fields

        private readonly IDemocriteTypeReferenceGrainServiceClient _typeReference;
        private readonly IDefinitionProvider _definitionProvider;

        private readonly Dictionary<Uri, ReferenceTarget> _indexedReferences;
        private readonly Dictionary<string, HashSet<ReferenceTarget>> _indexedReferenceBySNI;
        private readonly Dictionary<string, HashSet<ReferenceTarget>> _indexedReferenceByNamespace;
        private readonly Dictionary<string, HashSet<ReferenceTarget>> _indexedReferenceByMethod;
        private readonly Dictionary<RefTypeEnum, HashSet<ReferenceTarget>> _indexedReferenceByType;

        private readonly ILogger<IDemocriteReferenceSolverService> _logger;

        private readonly CancellationContext _syncLastContext;
        private readonly ReaderWriterLockSlim _locker;

        private string? _currentRegistryEtag;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteReferenceSolverService"/> class.
        /// </summary>
        public DemocriteReferenceSolverService(IDemocriteTypeReferenceGrainServiceClient typeReference,
                                               IDefinitionProvider definitionProvider,
                                               ILogger<IDemocriteReferenceSolverService> logger)
        {
            this._typeReference = typeReference;
            this._definitionProvider = definitionProvider;

            this._indexedReferences = new Dictionary<Uri, ReferenceTarget>(UriComparer.WithFragment);
            this._indexedReferenceBySNI = new Dictionary<string, HashSet<ReferenceTarget>>();
            this._indexedReferenceByNamespace = new Dictionary<string, HashSet<ReferenceTarget>>();
            this._indexedReferenceByMethod = new Dictionary<string, HashSet<ReferenceTarget>>();
            this._indexedReferenceByType = new Dictionary<RefTypeEnum, HashSet<ReferenceTarget>>();

            this._syncLastContext = new CancellationContext();
            this._locker = new ReaderWriterLockSlim();

            this._logger = logger;

            // TODO : Register to definition provider to clean to cached info dedicated to any changes
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<MethodInfo?> GetReferenceMethodAsync(Uri methodRefId, Type? sourceType = null, CancellationToken token = default)
        {
            var target = await GetReferenceTarget(methodRefId, token);

            if (target is ReferenceTypeMethodTarget mth)
            {
                var method = mth.Method.ToMethod(sourceType ?? mth.Type.ToType());
                if (method is null)
                    throw new InvalidDataException("Could not found method {0} on type {1}".WithArguments(mth.Method, sourceType));

                return method as MethodInfo;
            }

            if (target is not null)
                throw new InvalidDataException("{0} reference doesn't refer to a method kind".WithArguments(target.RefId));

            return null;
        }

        /// <inheritdoc />
        public async ValueTask<Tuple<Type, Uri>?> GetReferenceTypeAsync(Uri typeRefId, CancellationToken token = default)
        {
            var target = await GetReferenceTarget(typeRefId, token);

            if (target is not null)
            {
                if (target is ReferenceTypeTarget refTypeTarget)
                {
                    var type = refTypeTarget.Type.ToType();
                    if (type is null)
                        throw new InvalidDataException("Type {0} Referenced by ref {1} could not be loaded on this silo".WithArguments(refTypeTarget.Type, refTypeTarget.RefId));

                    return Tuple.Create(type, refTypeTarget.RefId);
                }

                throw new InvalidDataException("{0} reference doesn't refer to a type kind".WithArguments(target.RefId));
            }

            return null;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<IDefinition>> GetReferenceDefinitionsAsync(Uri definitionRefId, CancellationToken token = default)
        {
            var defFilter = GenerateFilterRequest(definitionRefId);

            var defs = await this._definitionProvider.GetValuesAsync(d => defFilter.IsMatch(((IRefDefinition)d).RefId.ToString()), token);
            return defs;
        }

        /// <inheritdoc />
        public async ValueTask<IDefinition?> TryGetReferenceDefinitionAsync(Uri definitionRefId, CancellationToken token = default)
        {
            var defs = await GetReferenceDefinitionsAsync(definitionRefId);

            if (!defs.Any())
                return null;

            if (defs.Count > 1)
                return null;

            return defs.Single();
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<Guid>> GetReferenceDefinitionUidAsync(Uri definitionRefId, CancellationToken token = default)
        {
            var defFilter = GenerateFilterRequest(definitionRefId);

            var defs = await this._definitionProvider.GetKeysAsync(d => defFilter.IsMatch(((IRefDefinition)d).RefId.ToString()), token);
            return defs;
        }

        /// <inheritdoc />
        public async ValueTask<Guid?> TryGetReferenceDefinitionUriAsync(Uri definitionRefId, CancellationToken token = default)
        {
            var defs = await GetReferenceDefinitionUidAsync(definitionRefId);

            if (!defs.Any())
                return null;

            if (defs.Count > 1)
                return null;

            return defs.Single();
        }

        #region Tools

        /// <summary>
        /// Generates the filter request.
        /// </summary>
        private Regex GenerateFilterRequest(Uri definitionRefId)
        {
            if (!RefIdHelper.IsRefId(definitionRefId))
                throw new ArgumentException(nameof(definitionRefId) + " must be a ref id format : " + definitionRefId);

            RefIdHelper.Explode(definitionRefId, out var _, out var ns, out var sni);

            if (!string.Equals(ns, RefIdHelper.DEFAULT_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                return new Regex(definitionRefId.OriginalString, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return new Regex("^" + RefIdHelper.REF_SCHEMA + "://" + definitionRefId.UserInfo + "@([a-zA-Z0-9.]+)/" + sni + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets the reference target.
        /// </summary>
        private async ValueTask<ReferenceTarget?> GetReferenceTarget(Uri refId, CancellationToken token)
        {
            if (!RefIdHelper.IsRefId(refId))
                return null;

            await EnsureRegistryIsUptoDate(token);
            ReferenceTarget? target = null;

            bool toAddToCache = false;

            this._locker.EnterReadLock();
            try
            {
                target = SafeGetReferenceTarget(refId, out toAddToCache);
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            if (toAddToCache && target is not null)
            {
                this._locker.EnterWriteLock();
                try
                {
                    SafeCacheReference(target);
                }
                finally
                {
                    this._locker.ExitWriteLock();
                }
            }

            return target;
        }

        /// <summary>
        /// [ThreadSafe] get <see cref="ReferenceTarget"/> based on local cache information
        /// </summary>
        [ThreadSafe]
        private ReferenceTarget? SafeGetReferenceTarget(Uri refId, out bool toCache)
        {
            toCache = false;

            if (this._indexedReferences.TryGetValue(refId, out var target))
                return target;

            toCache = true;

            RefIdHelper.Explode(refId, out var type, out var ns, out var sni);

            var result = SafeGetReferenceTargetDirect(refId, out var tooMany);

            if (result is null && type == RefTypeEnum.Method)
            {
                var methodSNI = RefIdHelper.GetMethodName(refId);

                var methodTargetType = SafeGetReferenceTargetDirect(RefIdHelper.Generate(RefTypeEnum.VGrain, sni, ns), out _);
                if (methodTargetType is null)
                    methodTargetType = SafeGetReferenceTargetDirect(RefIdHelper.Generate(RefTypeEnum.VGrainImplementation, sni, ns), out _);

                if (methodTargetType is ReferenceTypeTarget typeRef)
                {
                    var realType = typeRef.Type.ToType();
                    if (realType is not null)
                    {
                        // Look for parent declaration

                        var allType = realType.GetTypeInfoExtension().GetAllCompatibleTypes();

                        var methodRef = allType.Where(t => t != realType)
                                               .Select(t => TryGetRefByType(t))
                                               .NotNull()
                                               .Select(parentIdIf => SafeGetReferenceTargetDirect(RefIdHelper.WithMethod(parentIdIf, methodSNI), out _))
                                               .NotNull()
                                               .FirstOrDefault();

                        if (methodRef is not null)
                            return methodRef;
                    }
                }
            }

            if (result is null && tooMany is not null && tooMany.Any())
                throw new InvalidOperationException("Multiple target match the required ref " + refId + " : " + string.Join(", ", tooMany.Select(s => s.RefId)));

            return result;
        }

        /// <summary>
        /// Format refId based on type information
        /// </summary>
        private Uri? TryGetRefByType(Type solvedType)
        {
            var metaDataAttr = solvedType.GetCustomAttribute<VGrainMetaDataAttribute>();
            var simpleMetaDataAttr = solvedType.GetCustomAttribute<RefSimpleNameIdentifierAttribute>();

            var refType = RefTypeEnum.Type;
            if (solvedType.IsAssignableTo(typeof(IVGrain)))
                refType = solvedType.IsInterface ? RefTypeEnum.VGrain : RefTypeEnum.VGrainImplementation;

            var sni = metaDataAttr?.SimpleNameIdentifier ?? simpleMetaDataAttr?.SimpleNameIdentifier;
            var ns = metaDataAttr?.NamespaceIdentifier ?? simpleMetaDataAttr?.NamespaceIdentifier;

            if (string.IsNullOrEmpty(sni))
                return null;

            return RefIdHelper.Generate(refType, sni!, ns);
        }

        /// <summary>
        /// Get reference base on uri, null if not founded, <paramref name="tooManyMatches"/> is set if multiple ref matches
        /// </summary>
        private ReferenceTarget? SafeGetReferenceTargetDirect(Uri refId, out IReadOnlyCollection<ReferenceTarget>? tooManyMatches)
        {
            tooManyMatches = null;
            if (this._indexedReferences.TryGetValue(refId, out var target))
                return target;

            RefIdHelper.Explode(refId, out var type, out var ns, out var sni);

            var defaultEmpty = EnumerableHelper<ReferenceTarget>.ReadOnly;

            var byType = this._indexedReferenceByType.TryGetValueInline(type, out _) ?? defaultEmpty;

            if (byType.Any() == false)
                return null;

            var bySNI = this._indexedReferenceBySNI.TryGetValueInline(sni, out _) ?? defaultEmpty;

            if (bySNI.Any() == false)
                return null;

            // MATCH sni and type
            var matches = byType.Intersect(bySNI).ToArray();

            if (type == RefTypeEnum.Method)
            {
                var mthdName = RefIdHelper.GetMethodName(refId);
                matches = matches.Where(m => RefIdHelper.GetMethodName(m.RefId) == mthdName)
                                 .ToArray();
            }

            // If basic match only one then return 
            if (matches.Length == 1)
                return matches.First();
            else if (matches.Length == 0)
                return null;

            // Try filter more by namespace
            if (string.Equals(ns, RefIdHelper.DEFAULT_NAMESPACE, StringComparison.OrdinalIgnoreCase) == false)
            {
                var byNamespace = this._indexedReferenceByNamespace.TryGetValueInline(ns, out _) ?? defaultEmpty;
                matches = matches.Intersect(byNamespace).ToArray();

                if (matches.Length == 1)
                    return matches.First();
                else if (matches.Length == 0)
                    return null;
            }

            tooManyMatches = matches;
            return null;
        }

        /// <summary>
        /// Ensures the registry is upto date.
        /// </summary>
        private async ValueTask EnsureRegistryIsUptoDate(CancellationToken token)
        {
            bool needRefresh = false;
            this._locker.EnterReadLock();
            try
            {
                needRefresh = this._currentRegistryEtag == null || this._typeReference.IsToUpdate(this._currentRegistryEtag) == false;
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            if (needRefresh)
            {
                using (var grainCancellationToken = token.ToGrainCancellationTokenSource())
                {
                    var lastRegistry = await this._typeReference.GetLatestRegistryAsync(this._currentRegistryEtag ?? string.Empty, grainCancellationToken.Token);
                    if (lastRegistry != null)
                    {
                        using (var lastToken = this._syncLastContext.Lock())
                        {
                            try
                            {
                                this._locker.EnterWriteLock();
                                try
                                {
                                    // A most recent call have been made
                                    if (lastToken.Content.IsCancellationRequested)
                                        return;

                                    this._currentRegistryEtag = lastRegistry.Etag;
                                    SafeBuildIndexRegistry(lastRegistry);
                                }
                                finally
                                {
                                    this._locker.ExitWriteLock();
                                }
                            }
                            catch (Exception ex)
                            {
                                this._logger.OptiLog(LogLevel.Error, "Sync RefId local cache {exception}", ex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// build index registry.
        /// </summary>
        [ThreadSafe]
        private void SafeBuildIndexRegistry(ReferenceTargetRegistry? lastRegistry)
        {
            if (lastRegistry is null)
                return;

            this._indexedReferences.Clear();
            this._indexedReferenceBySNI.Clear();
            this._indexedReferenceByNamespace.Clear();
            this._indexedReferenceByMethod.Clear();
            this._indexedReferenceByType.Clear();

            foreach (var reference in lastRegistry.References)
                SafeCacheReference(reference);
        }

        [ThreadSafe]
        private void SafeCacheReference(ReferenceTarget reference)
        {
            if (this._indexedReferences.ContainsKey(reference.RefId))
                return;

            this._indexedReferences.Add(reference.RefId, reference);

            RefIdHelper.Explode(reference.RefId, out var type, out var ns, out var sni);

            var byType = this._indexedReferenceByType.TryGetOrAddContainer(type);
            byType.Add(reference);

            var bySNI = this._indexedReferenceBySNI.TryGetOrAddContainer(sni);
            bySNI.Add(reference);

            if (string.Equals(ns, RefIdHelper.DEFAULT_NAMESPACE, StringComparison.OrdinalIgnoreCase) == false)
            {
                var byNamespace = this._indexedReferenceByNamespace.TryGetOrAddContainer(ns);
                byNamespace.Add(reference);
            }

            if (type == RefTypeEnum.Method)
            {
                var methodSNI = RefIdHelper.GetMethodName(reference.RefId);

                if (!string.IsNullOrEmpty(methodSNI))
                {
                    var byMethodSNI = this._indexedReferenceByMethod.TryGetOrAddContainer(methodSNI);
                    byMethodSNI.Add(reference);
                }
            }
        }

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            this._locker.Dispose();
            base.DisposeBegin();
        }

        #endregion

        #endregion

    }
}
