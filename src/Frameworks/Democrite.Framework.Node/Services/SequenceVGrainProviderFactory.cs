// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Abstractions.Disposables;
    using Elvex.Toolbox.Disposables;

    using System.Linq;

    internal sealed class SequenceVGrainProviderFactory : ISequenceVGrainProviderFactory
    {
        #region Fields

        private readonly DisposableStructContainer<ISequenceVGrainProvider> _defaultSequenceVGrainProviderAction;
        private readonly ISequenceVGrainProvider _defaultSequenceVGrainProvider;

        private readonly IVGrainRouteService _rootVGrainRouteService;
        private readonly IVGrainProvider _rootGrainProvider;
        private readonly IVGrainIdFactory _grainIdFactory;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceVGrainProviderFactory"/> class.
        /// </summary>
        public SequenceVGrainProviderFactory(IVGrainIdFactory grainIdFactory,
                                             IVGrainProvider rootGrainProvider,
                                             IGrainFactory grainFactory,
                                             IVGrainRouteService rootVGrainRouteService)
        {
            this._grainFactory = grainFactory;
            this._grainIdFactory = grainIdFactory;
            this._rootGrainProvider = rootGrainProvider;
            this._rootVGrainRouteService = rootVGrainRouteService;

            this._defaultSequenceVGrainProvider = new NoRedirectionSequenceVGrainProvider(rootGrainProvider);
            this._defaultSequenceVGrainProviderAction = new DisposableStructContainer<ISequenceVGrainProvider>(this._defaultSequenceVGrainProvider);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISafeDisposable<ISequenceVGrainProvider> GetProvider(in ExecutionCustomizationDescriptions? customization)
        {
            if (customization is null || customization.Value.VGrainRedirection is null || !customization.Value.VGrainRedirection.Any())
                return _defaultSequenceVGrainProviderAction;

            var seq = new SequenceVGrainProvider(this._rootGrainProvider, this._grainFactory, this._grainIdFactory, this._rootVGrainRouteService);

            var valTask = seq.InitializationAsync(customization);
            valTask.GetAwaiter().GetResult();

            return new DisposableStructContainer<ISequenceVGrainProvider>(seq, true);
        }

        #endregion
    }
}
