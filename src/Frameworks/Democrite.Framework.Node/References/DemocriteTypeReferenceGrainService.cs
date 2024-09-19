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
    using Democrite.Framework.Node.Services;

    using Elvex.Toolbox.Abstractions.Comparers;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Services;

    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Grain service in charge to be used as reference solver
    /// </summary>
    internal interface IDemocriteTypeReferenceGrainService : IGrainService
    {
        /// <inheritdoc cref="IDemocriteReferenceSolverService.GetReferenceType(Uri)" />
        ValueTask<ReferenceTarget?> GetReferenceTarget(Uri refId, bool askOtherNodes = true);
    }

    /// <inheritdoc cref="IDemocriteTypeReferenceGrainService" />
    internal sealed class DemocriteTypeReferenceGrainService : DemocriteVGrainService, IDemocriteTypeReferenceGrainService
    {
        #region Fields

        private static readonly Type s_serviceTraits = typeof(IDemocriteTypeReferenceGrainService);

        private readonly IRemoteGrainServiceFactory _remoteGrainServiceFactory;
        private readonly ILogger<IDemocriteTypeReferenceGrainService> _logger;
        private readonly IClusterManifestProvider _clusterManifestProvider;

        private readonly HashSet<ReferenceTarget> _referenceTargets;
        private readonly ReaderWriterLockSlim _locker;
        private readonly Dictionary<Uri, ReferenceTarget> _indexByFullUri;
        private readonly Dictionary<string, HashSet<ReferenceTarget>> _indexByNamespace;
        private readonly Dictionary<string, HashSet<ReferenceTarget>> _indexBySimpleNameIdentifier;
        private readonly Dictionary<RefTypeEnum, HashSet<ReferenceTarget>> _indexByRefType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteTypeReferenceGrainService"/> class.
        /// </summary>
        public DemocriteTypeReferenceGrainService(GrainId grainId,
                                                  Silo silo,
                                                  ILoggerFactory loggerFactory,
                                                  IClusterManifestProvider clusterManifestProvider,
                                                  IRemoteGrainServiceFactory remoteGrainServiceFactory,
                                                  ILogger<IDemocriteTypeReferenceGrainService> logger)
            : base(grainId, silo, loggerFactory)
        {
            this._referenceTargets = new HashSet<ReferenceTarget>();
            this._locker = new ReaderWriterLockSlim();

            this._logger = logger;

            this._remoteGrainServiceFactory = remoteGrainServiceFactory;

            this._indexByFullUri = new Dictionary<Uri, ReferenceTarget>(UriComparer.WithFragment);
            this._indexByRefType = new Dictionary<RefTypeEnum, HashSet<ReferenceTarget>>();
            this._indexByNamespace = new Dictionary<string, HashSet<ReferenceTarget>>(StringComparer.OrdinalIgnoreCase);
            this._indexBySimpleNameIdentifier = new Dictionary<string, HashSet<ReferenceTarget>>(StringComparer.OrdinalIgnoreCase);

            this._clusterManifestProvider = clusterManifestProvider;

            var types = CSharpTypeInfo.ScalarTypes
                                      .Append(typeof(string))
                                      .Append(typeof(Guid))
                                      .Select(t => new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, t.Name.ToLowerWithSeparator('-'), DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)t.GetAbstractType()))
                                      .Concat(new[]
                                      {
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "int", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(int).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "big-int", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(Int64).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "double", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(double).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "float", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(float).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "long", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(long).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "string", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(string).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "short", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(short).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "ushort", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(ushort).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "byte", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(byte).GetAbstractType()),
                                          new ReferenceTypeTarget(RefIdHelper.Generate(RefTypeEnum.Type, "sbyte", DemocriteConstants.CSHARP_SYSTEM_NAMESPACE), RefTypeEnum.Type, (ConcretType)typeof(sbyte).GetAbstractType()),
                                      })
                                      .Distinct()
                                      .ToArray();

            foreach (var t in types)
                InjectReferenceTarget(t);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Task RefreshInfoAsync()
        {
            this._locker.EnterWriteLock();
            try
            {
                var entry = Assembly.GetEntryAssembly();
                var exec = Assembly.GetExecutingAssembly();
                var calling = Assembly.GetCallingAssembly();

                var allAssemblies = new[] { entry, exec, calling };
                var refProviders = EnumerableHelper<AssemblyName>.ReadOnly
                                        .Concat(entry?.GetReferencedAssemblies() ?? EnumerableHelper<AssemblyName>.ReadOnly)
                                        .Concat(exec.GetReferencedAssemblies())
                                        .Concat(calling.GetReferencedAssemblies())
                                        .Distinct()
                                        .Select(a => Assembly.Load(a))
                                        .Concat(allAssemblies)
                                        .NotNull()
                                        .Distinct()
                                        .SelectMany(a => a.GetCustomAttributes().OfType<DemocriteReferenceProviderAttribute>())
                                        .ToArray();

                if (refProviders.Any())
                    SafeImportProvider(refProviders);
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Critical, "Error loading source generated RefId Association :  {exception}", ex);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyLoad -= CurrentDomain_AssemblyLoad;
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

                this._locker.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async ValueTask<ReferenceTarget?> GetReferenceTarget(Uri typeRefId, bool askOtherNodes = true)
        {
            if (RefIdHelper.IsRefId(typeRefId) == false)
                throw new InvalidDataException("Must be a valid RefId");

            RefIdHelper.Explode(typeRefId, out var type, out var @namespace, out var sni);

            ReferenceTarget? founded = null;
            var addToCacheIfFounded = false;

            this._locker.EnterReadLock();
            try
            {
                if (!this._indexByFullUri.TryGetValue(typeRefId, out founded))
                {
                    addToCacheIfFounded = true;
                    if (this._indexBySimpleNameIdentifier.TryGetValue(sni, out var targets))
                    {
                        if (targets.Count == 1)
                        {
                            founded = targets.First();
                        }
                        else
                        {
                            var typeMatch = targets.Where(t => t.RefType == type).ToArray();

                            if (type == RefTypeEnum.Method)
                                typeMatch = typeMatch.Where(t => t.RefId.Fragment == typeRefId.Fragment).ToArray();

                            if (typeMatch.Length == 1)
                                return typeMatch.Single();
                            else if (typeMatch.Any() && this._indexByNamespace.TryGetValue(@namespace, out var nameSpacesTargets))
                                founded = typeMatch.Intersect(nameSpacesTargets).SingleOrDefault();
                        }
                    }
                }
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            if (founded is null && askOtherNodes)
            {
                addToCacheIfFounded = true;
                founded = await FetchFromClusterRefAsync(typeRefId);
            }

            if (founded is not null && addToCacheIfFounded)
                PushToCacheFullUri(typeRefId, founded);

            return founded;
        }

        #region Tools

        /// <summary>
        /// Currents the domain assembly load.
        /// </summary>
        private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            this._locker.EnterWriteLock();
            try
            {
                var providers = args.LoadedAssembly.GetCustomAttributes()
                                                  .OfType<DemocriteReferenceProviderAttribute>()
                                                  .ToArray();

                if (providers.Any())
                    SafeImportProvider(providers);
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        private void SafeImportProvider(params DemocriteReferenceProviderAttribute[] refProviders)
        {
            var registry = new DemocriteReferenceRegistry(this.Logger);

            foreach (var provider in refProviders)
            {
                try
                {
                    provider.Populate(registry);
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Critical, "Error loading source generated RefId Association :  {exception} from provider {provider}", ex, provider);
                }
            }

            var newReferences = registry.GetReferences();
            var referenceToAdd = newReferences.Except(this._referenceTargets).ToArray();

            foreach (var newRef in referenceToAdd)
                InjectReferenceTarget(newRef);

            if (referenceToAdd.Any())
            {
                // TODO: Change ETAG and notify the other nodes
            }
        }

        /// <summary>
        /// Injects the reference target.
        /// </summary>
        private void InjectReferenceTarget(ReferenceTarget? newRef)
        {
            if (newRef is null || this._referenceTargets.Contains(newRef))
                return;

            this._referenceTargets.Add(newRef);

            // Compute search index
            if (this._indexByFullUri.ContainsKey(newRef.RefId) == false)
                this._indexByFullUri.Add(newRef.RefId, newRef);

            RefIdHelper.Explode(newRef.RefId, out var refType, out var @namespace, out var simpleNameIdentifier);

            if (!this._indexByNamespace.TryGetValue(@namespace, out var targetsByNamespaces))
            {
                targetsByNamespaces = new HashSet<ReferenceTarget>();
                this._indexByNamespace.Add(@namespace, targetsByNamespaces);
            }

            if (!this._indexByRefType.TryGetValue(refType, out var targetsByRefTypes))
            {
                targetsByRefTypes = new HashSet<ReferenceTarget>();
                this._indexByRefType.Add(refType, targetsByNamespaces);
            }

            if (!this._indexBySimpleNameIdentifier.TryGetValue(simpleNameIdentifier, out var targetsBySimpleNameIdentifier))
            {
                targetsBySimpleNameIdentifier = new HashSet<ReferenceTarget>();
                this._indexBySimpleNameIdentifier.Add(simpleNameIdentifier, targetsBySimpleNameIdentifier);
            }

            targetsByNamespaces.Add(newRef);
            targetsByRefTypes.Add(newRef);
            targetsBySimpleNameIdentifier.Add(newRef);
        }

        /// <summary>
        /// Pushes to cache full URI the association <paramref name="typeRefId"/> with <paramref name="founded"/>.
        /// </summary>
        private void PushToCacheFullUri(Uri typeRefId, ReferenceTarget founded)
        {
            this._locker.EnterWriteLock();
            try
            {
                if (this._indexByFullUri.ContainsKey(typeRefId) == false)
                    this._indexByFullUri.Add(typeRefId, founded);
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Fetches from cluster reference asynchronous.
        /// </summary>
        // TODO : auto test multi nodes
        private async Task<ReferenceTarget?> FetchFromClusterRefAsync(Uri refId)
        {
            var otherSiloAddresses = this._clusterManifestProvider.Current.Silos
                                                                  .Select(s => s.Key)
                                                                  .Where(s => s != base.Silo)
                                                                  .ToArray();

            foreach (var otherSiloAddress in otherSiloAddresses)
            {
                var grainId = SystemTargetGrainId.Create(this.GrainReference.GrainId.Type, otherSiloAddress);
                var remoteGrain = this._remoteGrainServiceFactory.GetRemoteGrainService<IDemocriteTypeReferenceGrainService>(grainId.GrainId);

                var target = await remoteGrain.GetReferenceTarget(refId, false);

                if (target is not null)
                    return target;
            }

            return null;
        }

        #endregion

        #endregion
    }
}
