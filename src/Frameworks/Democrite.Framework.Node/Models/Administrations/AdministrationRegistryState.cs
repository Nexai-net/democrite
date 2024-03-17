// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models.Administrations
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Abstractions.Administrations;
    using Democrite.Framework.Node.Abstractions.Enums;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Main state of the <see cref="AdministrationRegistryVGrain"/>
    /// </summary>
    internal sealed class AdministrationRegistryState
    {
        #region Fields

        private readonly Dictionary<Guid, DedicatedGrainId<IAdminEventReceiver>> _subscriptions;
        private readonly Dictionary<AdminEventTypeEnum, HashSet<Guid>> _byCategorySubscriptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationRegistryState"/> class.
        /// </summary>
        public AdministrationRegistryState(IReadOnlyCollection<Subscription> subscriptions, ClusterRouteRegistryState routeRegistryState)
        {
            subscriptions ??= EnumerableHelper<Subscription>.ReadOnly;

            this._subscriptions = new Dictionary<Guid, DedicatedGrainId<IAdminEventReceiver>>();
            this._byCategorySubscriptions = new Dictionary<AdminEventTypeEnum, HashSet<Guid>>();

            foreach (var subscription in subscriptions)
            {
                this._subscriptions[subscription.Uid] = subscription.Grain;

                HashSet<Guid>? subscriptionId;
                if (!this._byCategorySubscriptions.TryGetValue(subscription.Type, out subscriptionId))
                {
                    subscriptionId = new HashSet<Guid>();
                    this._byCategorySubscriptions.Add(subscription.Type, subscriptionId);
                }

                subscriptionId.Add(subscription.Uid);
            }

            this.RouteRegistryState = routeRegistryState ?? ClusterRouteRegistryStateConverter.Instance.ConvertFromSurrogate(default);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the state of the route registry.
        /// </summary>
        public ClusterRouteRegistryState RouteRegistryState { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the subscription.
        /// </summary>
        public IReadOnlyCollection<DedicatedGrainId<IAdminEventReceiver>> GetSubscription(AdminEventTypeEnum adminEventType)
        {
            if (this._byCategorySubscriptions.TryGetValue(adminEventType, out var hash))
            {
                return hash.Select(id =>
                           {
                               if (this._subscriptions.TryGetValue(id, out var grainId))
                                   return grainId;
                               return default;
                           })
                           .Where(grd => grd != default)
                           .ToReadOnly();
            }

            return EnumerableHelper<DedicatedGrainId<IAdminEventReceiver>>.ReadOnly;
        }

        /// <summary>
        /// Subscribes the specified grain identifier.
        /// </summary>
        public Guid Subscribe(DedicatedGrainId<IAdminEventReceiver> grainId, AdminEventTypeEnum type)
        {
            var id = Guid.NewGuid();

            this._subscriptions[id] = grainId;

            HashSet<Guid>? subscriptionId;
            if (!this._byCategorySubscriptions.TryGetValue(type, out subscriptionId))
            {
                subscriptionId = new HashSet<Guid>();
                this._byCategorySubscriptions.Add(type, subscriptionId);
            }

            subscriptionId.Add(id);
            return id;
        }

        /// <summary>
        /// Unsubscribes the specified subscription identifier.
        /// </summary>
        public void Unsubscribe(Guid id)
        {
            if (this._subscriptions.Remove(id))
            {
                foreach (var kv in this._byCategorySubscriptions)
                    kv.Value.Remove(id);
            }
        }

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public AdministrationRegistryStateSurrogate ToSurrogate()
        {
            var sub = this._byCategorySubscriptions.SelectMany(s => s.Value.Select(kv => new Subscription(kv, s.Key, this._subscriptions[kv])))
                                                   .Distinct()
                                                   .ToArray();

            return new AdministrationRegistryStateSurrogate(sub,
                                                            ClusterRouteRegistryStateConverter.Instance.ConvertToSurrogate(this.RouteRegistryState));
        }

        #endregion
    }

    [GenerateSerializer]
    internal record struct Subscription(Guid Uid, AdminEventTypeEnum Type, DedicatedGrainId<IAdminEventReceiver> Grain);

    [GenerateSerializer]
    internal record struct AdministrationRegistryStateSurrogate(IReadOnlyCollection<Subscription> Subscriptions, ClusterRouteRegistryStateSurrogate ClusterRouteRegistryState);

    [RegisterConverter]
    internal sealed class AdministrationRegistryStateConverter : IConverter<AdministrationRegistryState, AdministrationRegistryStateSurrogate>
    {
        /// <inheritdoc />
        public AdministrationRegistryState ConvertFromSurrogate(in AdministrationRegistryStateSurrogate surrogate)
        {
            return new AdministrationRegistryState(surrogate.Subscriptions,
                                                   ClusterRouteRegistryStateConverter.Instance.ConvertFromSurrogate(surrogate.ClusterRouteRegistryState));
        }

        /// <inheritdoc />
        public AdministrationRegistryStateSurrogate ConvertToSurrogate(in AdministrationRegistryState value)
        {
            return value?.ToSurrogate() ?? new AdministrationRegistryStateSurrogate(EnumerableHelper<Subscription>.ReadOnly, default);
        }
    }
}
