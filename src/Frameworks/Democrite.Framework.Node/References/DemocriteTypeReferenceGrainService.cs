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

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;
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
        /// <summary>
        /// Gets the latest local registry if etag differt from <paramref name="myCurrentEtag"/>
        /// </summary>
        ValueTask<ReferenceTargetRegistry?> GetLatestRegistryAsync(string myCurrentEtag, GrainCancellationToken token);

        /// <summary>
        /// Try update the local registry
        /// </summary>
        Task TryUpdateLocalRegistry();

        /// <summary>
        /// Determines whether if ETAG is the last used
        /// </summary>
        ValueTask<bool> IsToUpdate(string currentRegistryEtag);
    }

    /// <summary>
    /// Client to consume the cluster type/reference
    /// </summary>
    internal interface IDemocriteTypeReferenceGrainServiceClient : IGrainServiceClient<IDemocriteTypeReferenceGrainService>
    {
        /// <summary>
        /// Gets the latest local registry if etag differt from <paramref name="myCurrentEtag"/>
        /// </summary>
        ValueTask<ReferenceTargetRegistry?> GetLatestRegistryAsync(string myCurrentEtag, GrainCancellationToken token);

        /// <summary>
        /// Determines whether if ETAG is the last used
        /// </summary>
        bool IsToUpdate(string currentRegistryEtag);
    }

    /// <inheritdoc cref="IDemocriteTypeReferenceGrainService" />
    /// <remarks>
    ///     Synchronize the registry beetween all cluster using ETAG
    ///     
    ///     1) Build local generate a new ETAG
    ///     2) Notify the other to sync
    ///     
    ///  Synchronization
    ///     1) Ask all the other theire registry and ETAG
    ///     2) If all the node have the same ETAG then stop sync
    ///     3) Aggregate all the targets
    ///          3.1) If after aggregation all the target are the same than used to be just change you ETAG to match the other
    ///          3.2) if change between the previous information then notify the other that change exist then trigger a synchronization circle until all the node have the same ETAG
    /// </remarks>
    internal sealed class DemocriteTypeReferenceGrainService : DemocriteVGrainService, IDemocriteTypeReferenceGrainService
    {
        #region Fields

        private readonly ILogger<IDemocriteTypeReferenceGrainService> _logger;

        private readonly IRemoteGrainServiceFactory _remoteGrainServiceFactory;
        private readonly IClusterManifestProvider _clusterManifestProvider;

        private readonly HashSet<ReferenceTarget> _referenceTargetLocal;
        private readonly HashSet<ReferenceTarget> _referenceTargetRemote;
        private readonly SemaphoreSlim _locker;

        private string _currentEtag;

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
            this._referenceTargetLocal = new HashSet<ReferenceTarget>();
            this._referenceTargetRemote = new HashSet<ReferenceTarget>();
            this._locker = new SemaphoreSlim(1);

            this._logger = logger;

            this._remoteGrainServiceFactory = remoteGrainServiceFactory;

            this._clusterManifestProvider = clusterManifestProvider;

#pragma warning disable IDE0049 // Simplify Names
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
#pragma warning restore IDE0049 // Simplify Names

            foreach (var t in types)
                InjectReferenceTarget(t);

            this._currentEtag = Guid.NewGuid().ToString().Replace("-", "");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<ReferenceTargetRegistry?> GetLatestRegistryAsync(string myCurrentEtag, GrainCancellationToken token)
        {
            ReferenceTargetRegistry? result = null!;
            if (this._currentEtag != myCurrentEtag)
            {
                result = new ReferenceTargetRegistry(this._currentEtag,
                                                     this._referenceTargetLocal.Concat(this._referenceTargetRemote).ToArray());
            }

            token.CancellationToken.ThrowIfCancellationRequested();

            return ValueTask.FromResult<ReferenceTargetRegistry?>(result);
        }

        /// <inheritdoc />
        public async Task TryUpdateLocalRegistry()
        {
            bool localChanges = false;

            using (var disposeToken = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(25)))
            using (var grainToken = disposeToken.Content.ToGrainCancellationTokenSource())
            {
                await this._locker.WaitAsync(disposeToken.Content);
                try
                {
                    var registries = await CallOtherClusterNodes(r => r.GetLatestRegistryAsync(this._currentEtag, grainToken.Token).AsTask());

                    if (registries.All(r => r is null))
                        return;

                    var copyRemote = this._referenceTargetRemote.ToHashSet();
                    var oldRefs = this._referenceTargetLocal.Concat(this._referenceTargetRemote).ToArray();

                    this._referenceTargetRemote.Clear();

                    foreach (var registry in registries.NotNull())
                    {
                        foreach (var target in registry.References)
                        {
                            if (this._referenceTargetLocal.Contains(target))
                                return;

                            this._referenceTargetRemote.Add(target);
                        }
                    }

                    var newRefs = this._referenceTargetLocal.Concat(this._referenceTargetRemote).ToArray();
                    var diff = newRefs.Except(oldRefs).Any();

                    if (diff == false)
                    {
                        this._currentEtag = registries.Where(r => r is not null)
                                                      .GroupBy(r => r!.Etag)
                                                      .OrderByDescending(r => r.Count()).First().Key;
                    }
                    else
                    {
                        UpdateLocalETAG();
                        localChanges = true;
                    }
                }
                finally
                {
                    this._locker.Release();
                }

                if (localChanges)
                    await NotifyChangesAsync();
            }
        }

        /// <inheritdoc />
        public ValueTask<bool> IsToUpdate(string currentRegistryEtag)
        {
            return ValueTask.FromResult(this._currentEtag == currentRegistryEtag);
        }

        #region Tools

        /// <inheritdoc />
        /// <remarks>
        ///     Refresh local info
        /// </remarks>
        protected override async Task RefreshInfoAsync()
        {
            bool haveChanged = false;
            await this._locker.WaitAsync();
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
                    haveChanged |= SafeImportProvider(refProviders);

                if (haveChanged)
                    UpdateLocalETAG();

            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Critical, "Error loading source generated RefId Association :  {exception}", ex);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyLoad -= CurrentDomain_AssemblyLoad;
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

                this._locker.Release();
            }

            if (haveChanged)
                await NotifyChangesAsync();
        }

        /// <summary>
        /// Updates the local etag.
        /// </summary>
        private void UpdateLocalETAG()
        {
            this._currentEtag = Guid.NewGuid().ToString().Replace("-", "");
        }

        ///// <inheritdoc />
        //public async ValueTask<ReferenceTarget?> GetReferenceTarget(Uri typeRefId, bool askOtherNodes = true)
        //{
        //    if (RefIdHelper.IsRefId(typeRefId) == false)
        //        throw new InvalidDataException("Must be a valid RefId");

        //    RefIdHelper.Explode(typeRefId, out var type, out var @namespace, out var sni);

        //    ReferenceTarget? founded = null;
        //    var addToCacheIfFounded = false;

        //    this._locker.EnterReadLock();
        //    try
        //    {
        //        if (!this._indexByFullUri.TryGetValue(typeRefId, out founded))
        //        {
        //            addToCacheIfFounded = true;
        //            if (this._indexBySimpleNameIdentifier.TryGetValue(sni, out var targets))
        //            {
        //                if (targets.Count == 1)
        //                {
        //                    founded = targets.First();
        //                }
        //                else
        //                {
        //                    var typeMatch = targets.Where(t => t.RefType == type).ToArray();

        //                    if (type == RefTypeEnum.Method)
        //                        typeMatch = typeMatch.Where(t => t.RefId.Fragment == typeRefId.Fragment).ToArray();

        //                    if (typeMatch.Length == 1)
        //                        return typeMatch.Single();
        //                    else if (typeMatch.Any() && this._indexByNamespace.TryGetValue(@namespace, out var nameSpacesTargets))
        //                        founded = typeMatch.Intersect(nameSpacesTargets).SingleOrDefault();
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        this._locker.ExitReadLock();
        //    }

        //    if (founded is null && askOtherNodes)
        //    {
        //        addToCacheIfFounded = true;
        //        founded = await FetchFromClusterRefAsync(typeRefId);
        //    }

        //    if (founded is not null && addToCacheIfFounded)
        //        PushToCacheFullUri(typeRefId, founded);

        //    return founded;
        //}

        /// <summary>
        /// Currents the domain assembly load.
        /// </summary>
        private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            bool needClusterSync = false;

            this._locker.Wait();
            try
            {
                var savedEtagState = this._currentEtag;
                var providers = args.LoadedAssembly.GetCustomAttributes()
                                                   .OfType<DemocriteReferenceProviderAttribute>()
                                                   .ToArray();

                if (providers.Any())
                    SafeImportProvider(providers);

                needClusterSync = this._currentEtag != savedEtagState;
            }
            finally
            {
                this._locker.Release();
            }

            if (needClusterSync)
                Task.Run(NotifyChangesAsync);
        }

        /// <summary>
        /// Notifies the other cluster node that reference registry have changes
        /// </summary>
        private async Task NotifyChangesAsync()
        {
            await CallOtherClusterNodes<NoneType>(async f =>
            {
                await f.TryUpdateLocalRegistry();
                return NoneType.Instance;
            });
        }

        /// <summary>
        /// Safes the import provider.
        /// </summary>
        /// <returns>
        ///     return <c>true</c> if new elements have been imported
        /// </returns>
        private bool SafeImportProvider(params DemocriteReferenceProviderAttribute[] refProviders)
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
            var referenceToAdd = newReferences.Except(this._referenceTargetLocal)
                                              .Except(this._referenceTargetRemote)
                                              .ToArray();

            bool haveChanges = false;

            foreach (var newRef in referenceToAdd)
                haveChanges |= InjectReferenceTarget(newRef);

            return haveChanges;
        }

        /// <summary>
        /// Injects the reference target.
        /// </summary>
        private bool InjectReferenceTarget(ReferenceTarget? newRef)
        {
            if (newRef is null)
                return false;

            return this._referenceTargetLocal.Add(newRef);
        }

        /// <summary>
        /// Calls the other cluster nodes.
        /// </summary>
        private async Task<IReadOnlyCollection<TResult?>> CallOtherClusterNodes<TResult>(Func<IDemocriteTypeReferenceGrainService, Task<TResult?>> fetch)
        {
            var otherSiloAddresses = this._clusterManifestProvider.Current.Silos
                                                                  .Select(s => s.Key)
                                                                  .Where(s => s != base.Silo)
                                                                  .ToArray();

            var results = new List<TResult?>();

            foreach (var otherSiloAddress in otherSiloAddresses)
            {
                var grainId = SystemTargetGrainId.Create(this.GrainReference.GrainId.Type, otherSiloAddress);
                var remoteGrain = this._remoteGrainServiceFactory.GetRemoteGrainService<IDemocriteTypeReferenceGrainService>(grainId.GrainId);

                var remoteResult = await fetch(remoteGrain);

                results.Add(remoteResult);
            }

            return results;
        }

        #endregion

        #endregion
    }
}
