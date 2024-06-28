// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Administrations
{
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Node.Abstractions.Administrations;
    using Democrite.Framework.Node.Abstractions.Enums;
    using Democrite.Framework.Node.Abstractions.Models.Administrations;
    using Democrite.Framework.Node.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    internal sealed class AdministrationGrainService : DemocriteVGrainService, IAdministrationGrainService
    {
        #region Fields

        private readonly IVGrainDemocriteSystemProvider _democriteSystemProvider;
        private readonly ILogger<IAdministrationGrainService> _localLogger;

        private readonly GrainRouteSiloRootService _grainRouteSiloRootService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationGrainService"/> class.
        /// </summary>
        public AdministrationGrainService(GrainId grainId,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          IVGrainDemocriteSystemProvider democriteSystemProvider,
                                          GrainRouteSiloRootService grainRouteSiloRootService)
            : base(grainId, silo, loggerFactory)
        {
            this._democriteSystemProvider = democriteSystemProvider;
            this._localLogger = loggerFactory.CreateLogger<IAdministrationGrainService>();
            this._grainRouteSiloRootService = grainRouteSiloRootService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task RefreshInfoAsync()
        {
            using (var cancellation = new GrainCancellationTokenSource())
            {
                var clusterRoute = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(null, this._localLogger);
                await clusterRoute.SubscribeRouteChangeAsync(this.GetDedicatedGrainId<IAdminEventReceiver>(), cancellation.Token);
            }
        }

        /// <inheritdoc />
        public async Task ReceiveAsync<TEvent>(TEvent adminEvent, GrainCancellationToken cancellationToken) where TEvent : AdminEventArg
        {
            if (adminEvent is null)
                return;

            if (adminEvent.Type == AdminEventTypeEnum.RouteChange)
            {
                await this._grainRouteSiloRootService.UpdateGlobalRedirectionAsync(adminEvent.Etag, cancellationToken.CancellationToken);
                return;
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}
