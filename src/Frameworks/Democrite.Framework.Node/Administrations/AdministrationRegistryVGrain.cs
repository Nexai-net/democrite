// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Administrations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Node.Abstractions.Administrations;
    using Democrite.Framework.Node.Abstractions.Enums;
    using Democrite.Framework.Node.Abstractions.Models.Administrations;
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Models.Administrations;

    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Global administration registry center. 
    /// This grain centralise all the administration information.
    /// A main registry have been created to easy the devellopement, maintenance and administration information interaction.
    /// Each administration section is split in multiple interface to allow separation later on
    /// </summary>
    /// <seealso cref="VGrainBase{}" />
    /// <seealso cref="" />
    internal sealed class AdministrationRegistryVGrain : VGrainBase<AdministrationRegistryState, AdministrationRegistryStateSurrogate, AdministrationRegistryStateConverter, IGlobalAdministrationRegisterVGrain>, IGlobalAdministrationRegisterVGrain, IClusterRouteRegistryVGrain
    {
        #region Fields
        
        private readonly IGrainOrleanFactory _grainOrleanFactory;
        private readonly IRemoteGrainServiceFactory _remoteGrainServiceFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationRegistryVGrain"/> class.
        /// </summary>
        public AdministrationRegistryVGrain(ILogger<IGlobalAdministrationRegisterVGrain> logger,
                                            [PersistentState(DemocriteConstants.DefaultDemocriteAdminStateConfigurationKey, DemocriteConstants.DefaultDemocriteAdminStateConfigurationKey)] IPersistentState<AdministrationRegistryStateSurrogate> persistentState,
                                            IGrainOrleanFactory grainOrleanFactory,
                                            IRemoteGrainServiceFactory remoteGrainServiceFactory)
            : base(logger, persistentState)
        {
            this._grainOrleanFactory = grainOrleanFactory;
            this._remoteGrainServiceFactory = remoteGrainServiceFactory;
        }

        #endregion

        #region Methods

        #region IClusterRouteRegistryVGrain

        /// <inheritdoc />
        public Task<EtagContainer<IReadOnlyCollection<VGrainRedirectionDefinition>>?> GetGlobalRedirection(string etagInCache, GrainCancellationToken cancellationToken)
        {
            EtagContainer<IReadOnlyCollection<VGrainRedirectionDefinition>>? container = null;
            var routeState = this.State!.RouteRegistryState;

            if (!string.Equals(routeState.Etag, etagInCache))
                container = new EtagContainer<IReadOnlyCollection<VGrainRedirectionDefinition>>(routeState.Etag, routeState.GetVGrainRedirections().ToArray());
            return Task.FromResult(container);
        }

        /// <inheritdoc />
        public Task<bool> RequestAppendRedirectionAsync(VGrainRedirectionDefinition grainRedirections, IIdentityCard identity)
        {
            return RequestAppendRedirectionAsync(new[] { grainRedirections }, identity);
        }

        /// <inheritdoc />
        public Task<bool> RequestPopRedirectionAsync(Guid redirectionId, IIdentityCard identity)
        {
            return RequestPopRedirectionAsync(new[] { redirectionId }, identity);
        }

        /// <inheritdoc />
        public async Task<bool> RequestAppendRedirectionAsync(IReadOnlyCollection<VGrainRedirectionDefinition> grainRedirections, IIdentityCard identity)
        {
            // TODO : Call ISecurityAgentService for right to execute the following action
            var result = this.State!.RouteRegistryState.PushRedirections(grainRedirections.ToArray());

            await IfSuccessPushAndNotify(AdminEventTypeEnum.RouteChange, this.State!.RouteRegistryState.Etag, result);

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> RequestPopRedirectionAsync(IReadOnlyCollection<Guid> redirectionIds, IIdentityCard identity)
        {
            // TODO : Call ISecurityAgentService for right to execute the following action
            var result = this.State!.RouteRegistryState.PopRedirections(redirectionIds.ToArray());

            await IfSuccessPushAndNotify(AdminEventTypeEnum.RouteChange, this.State!.RouteRegistryState.Etag, result);

            return result;
        }

        /// <inheritdoc />
        public async Task<Guid> SubscribeRouteChangeAsync(DedicatedGrainId<IAdminEventReceiver> target, GrainCancellationToken cancellationToken)
        {
            var id = this.State!.Subscribe(target, AdminEventTypeEnum.RouteChange);

            await PushStateAsync(default);

            return id;
        }

        #endregion

        /// <inheritdoc />
        public Task UnsubscribeAsync(Guid subscriptionId)
        {
            this.State!.Unsubscribe(subscriptionId);
            return Task.CompletedTask;
        }

        #region Tools

        /// <inheritdoc cref="NotifySubscribersAsync(AdminEventArg)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask IfSuccessPushAndNotify(AdminEventTypeEnum eventType, string etag, bool result)
        {
            if (result)
            {
                await PushStateAsync(default);
                await NotifySubscribersAsync(AdminEventTypeEnum.RouteChange, this.State!.RouteRegistryState.Etag);
            }
        }

        /// <inheritdoc cref="NotifySubscribersAsync(AdminEventArg)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask NotifySubscribersAsync(AdminEventTypeEnum eventType, string etag)
        {
            return NotifySubscribersAsync(new AdminEventArg(eventType, etag));
        }

        /// <summary>
        /// Notify the specific subscribers that change occured on the domain <paramref name="eventType"/>
        /// </summary>
        private async ValueTask NotifySubscribersAsync<TEventArg>(TEventArg eventArg)
            where TEventArg : AdminEventArg
        {
            var subscriptions = this.State!.GetSubscription(eventArg.Type);

            if (subscriptions is null || subscriptions.Any() == false)
                return;

            foreach (var subscriptionGrainId in subscriptions)
            {
                using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(2)))
                using (var grainCancellation = timeout.Content.ToGrainCancellationTokenSource())
                {
                    try
                    {
                        IAdminEventReceiver? grain = null;

                        if (subscriptionGrainId.IsGrainService)
                        {
                            grain = this._remoteGrainServiceFactory.GetRemoteGrainService<IAdminEventReceiver>(subscriptionGrainId.Target, subscriptionGrainId.GrainInterface.ToType());
                        }
                        else
                        {
                            var addressable = this._grainOrleanFactory.GetGrain(subscriptionGrainId.Target);

                            // Test mainly need for unit testing
                            if (addressable is IAdminEventReceiver receiver)
                                grain = receiver;
                            else
                                grain = addressable.AsReference<IAdminEventReceiver>();
                        }

                        if (grain is null)
                        {
                            this.Logger.OptiLog(LogLevel.Critical,
                                                "Target not founded {eventType} ({etag}) target {grainTargetInfo}",
                                                eventArg.Type,
                                                eventArg.Etag,
                                                subscriptionGrainId);
                            continue;
                        }

                        await grain.ReceiveAsync(eventArg, grainCancellation.Token);
                    }
                    // TODO : Catch offline and remove subscription
                    catch (Exception ex)
                    {
                        this.Logger.OptiLog(LogLevel.Critical,
                                            "Error during notification of change {eventType} ({etag}) target {grainTargetInfo} : {exception}",
                                            eventArg.Type,
                                            eventArg.Etag,
                                            subscriptionGrainId,
                                            ex);
                    }
                }
            }
        }

        #endregion

        #endregion

    }
}
