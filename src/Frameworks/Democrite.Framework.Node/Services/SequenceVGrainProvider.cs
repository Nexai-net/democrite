// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Supports;

    using Orleans;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Simple relay
    /// </summary>
    /// <seealso cref="Democrite.Framework.Node.Services.ISequenceVGrainProvider" />
    internal sealed class NoRedirectionSequenceVGrainProvider : ISequenceVGrainProvider
    {
        #region Fields
        
        private readonly IVGrainProvider _defaultGrainProvider;
        
        #endregion
        
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoRedirectionSequenceVGrainProvider"/> class.
        /// </summary>
        public NoRedirectionSequenceVGrainProvider(IVGrainProvider defaultGrainProvider)
        {
            this._defaultGrainProvider = defaultGrainProvider;
        }
        
        #endregion

        #region Methods

        /// <inheritdoc />
        public IVGrainProvider GetGrainProvider(ref readonly SequenceStageDefinition _)
        {
            return this._defaultGrainProvider;
        }

        #endregion
    }

    /// <summary>
    /// Provider based on sequence customization to apply redirection and other
    /// </summary>
    /// <seealso cref="ISequenceVGrainProvider" />
    internal sealed class SequenceVGrainProvider : SupportBaseInitialization<ExecutionCustomizationDescriptions?>, ISequenceVGrainProvider, ISupportInitialization<ExecutionCustomizationDescriptions?>
    {
        #region Fields

        private readonly IVGrainRouteService _grainRouteScopedService;
        private readonly IVGrainIdFactory _grainIdFactory;
        private readonly IVGrainProvider _vgrainProvider;
        private readonly IGrainFactory _grainFactory;

        private IReadOnlyDictionary<Guid, IVGrainProvider> _providerScope;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceVGrainProvider"/> class.
        /// </summary>
        public SequenceVGrainProvider(IVGrainProvider vgrainProvider,
                                      IGrainFactory grainFactory,
                                      IVGrainIdFactory grainIdFactory,
                                      IVGrainRouteService grainRouteScopedService)
        {
            this._vgrainProvider = vgrainProvider;
            this._grainFactory = grainFactory;
            this._grainIdFactory = grainIdFactory;
            this._grainRouteScopedService = grainRouteScopedService;
            this._providerScope = ImmutableDictionary<Guid, IVGrainProvider>.Empty;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IVGrainProvider GetGrainProvider(ref readonly SequenceStageDefinition step)
        {
            if (this._providerScope.TryGetValue(step.Uid, out var provider))
                return provider;

            if (this._providerScope.TryGetValue(Guid.Empty, out var globalProvider))
                return globalProvider;

            return this._vgrainProvider;
        }

        /// <inheritdoc />
        protected override ValueTask OnInitializingAsync(ExecutionCustomizationDescriptions? customization, CancellationToken token)
        {
            if (customization is not null)
            {
                var providerScope = new Dictionary<Guid, IVGrainProvider>();
                var redirectionIndexed = customization.Value
                                                      .VGrainRedirection
                                                      .GroupBy(stg => stg.StageUid ?? Guid.Empty)
                                                      .ToDictionary(k => k.Key, v => v.ToReadOnly());

                IEnumerable<VGrainRedirectionDefinition> globalRedirections = EnumerableHelper<VGrainRedirectionDefinition>.ReadOnly;

                if (redirectionIndexed.TryGetValue(Guid.Empty, out var stageRedirection))
                    globalRedirections = stageRedirection.Select(g => g.RedirectionDefinition);

                var globalIndexedBySource = globalRedirections.ToDictionary(k => k.Source);

                foreach (var speRedirections in redirectionIndexed)
                {
                    var speRedirectionIndexedBySource = speRedirections.Value
                                                                       .Select(kv => kv.RedirectionDefinition)
                                                                       .ToDictionary(k => k.Source);

                    var globalOnes = globalIndexedBySource.Keys.Except(speRedirectionIndexedBySource.Keys);

                    var speRedirectionDef = speRedirectionIndexedBySource.Values
                                                                         .Concat(globalOnes.Select(gl => globalIndexedBySource[gl])).ToArray();

                    providerScope.Add(speRedirections.Key, new VGrainProvider(new GrainFactoryScoped(this._grainFactory, new GrainRouteFixedService(speRedirectionDef, this._grainRouteScopedService)), this._grainIdFactory));
                }

                /* 
                 * Why not frozen ? 
                 *      -> Frozen apport huge performance grain in read process but are around 5 times slower to build.
                 *      -> This SequenceVGrainProvider is build at each sequence execution initialization time cannot be ignored
                 */
                this._providerScope = providerScope.ToDictionary();
            }

            return ValueTask.CompletedTask;
        }

        #endregion
    }
}
