// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class BlackboardGrainState
    {
        #region Fields

        private readonly Dictionary<Guid, BlackboardDeferredQueryState> _queries;
        private readonly Dictionary<Guid, SubscriptionId> _subscriptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardGrainState"/> class.
        /// </summary>
        public BlackboardGrainState(BlackboardTemplateDefinition? templateCopy,
                                    BlackboardId blackboardId,
                                    string name,
                                    BlackboardRecordRegistryState blackboardRecordRegistryState,
                                    IEnumerable<BlackboardDeferredQueryState> queries,
                                    BlackboardLifeStatusEnum currentLifeStatus,
                                    IEnumerable<SubscriptionId> subscriptionIds)
        {
            this.TemplateCopy = templateCopy;
            this.BlackboardId = blackboardId;
            this.Name = name;
            this.Registry = blackboardRecordRegistryState;

            this.CurrentLifeStatus = currentLifeStatus;

            this._queries = queries?.ToDictionary(q => q.DeferredId.Uid) ?? new Dictionary<Guid, BlackboardDeferredQueryState>();
            this._subscriptions = subscriptionIds?.ToDictionary(s => s.SignalId) ?? new Dictionary<Guid, SubscriptionId>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is build.
        /// </summary>
        public bool IsBuild
        {
            get { return this.TemplateCopy is not null; }
        }

        /// <summary>
        /// Gets a copy of the template used during the first initialization
        /// </summary>
        public BlackboardTemplateDefinition? TemplateCopy { get; private set; }

        /// <summary>
        /// Gets the blackboard identifier.
        /// </summary>
        public BlackboardId BlackboardId { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the registry.
        /// </summary>
        public BlackboardRecordRegistryState Registry { get; }

        /// <summary>
        /// Gets the current life status.
        /// </summary>
        public BlackboardLifeStatusEnum CurrentLifeStatus { get; internal set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the requests.
        /// </summary>
        public IReadOnlyCollection<BlackboardDeferredQueryState> GetQueries()
        {
            return this._queries.Values;
        }

        /// <summary>
        /// Adds the or update request.
        /// </summary>
        public bool AddOrUpdateQuery(BlackboardDeferredQueryState deferredQuery)
        {
            if (!this._queries.TryGetValue(deferredQuery.DeferredId.Uid, out var existing) || deferredQuery != existing)
            {
                this._queries[deferredQuery.DeferredId.Uid] = deferredQuery;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the request.
        /// </summary>
        public bool RemoveQuery(BlackboardBaseQuery request)
        {
            return RemoveQuery(request.QueryUid);
        }

        /// <summary>
        /// Removes the request.
        /// </summary>
        public bool RemoveQuery(Guid requestUid)
        {
            return this._queries.Remove(requestUid);
        }

        /// <summary>
        /// Builds the using template.
        /// </summary>
        internal void BuildUsingTemplate(BlackboardTemplateDefinition tmpl, BlackboardId blackboardId)
        {
            this.TemplateCopy = tmpl;
            this.BlackboardId = blackboardId;
        }

        /// <summary>
        /// Adds a signal/door subscriptions.
        /// </summary>
        internal void AddSubscription(in SubscriptionId subscription)
        {
            this._subscriptions[subscription.SignalId] = subscription;
        }

        /// <summary>
        /// Get subscription by signal/door uid
        /// </summary>
        internal SubscriptionId? GetSubscription(in Guid signalUid)
        {
            if (this._subscriptions.TryGetValue(signalUid, out var subscription))
                return subscription;

            return null;
        }

        /// <summary>
        /// Get all subscriptions
        /// </summary>
        internal IEnumerable<SubscriptionId> GetSubscriptions()
        {
            return this._subscriptions.Values;
        }

        /// <summary>
        /// Removes a signal/door subscriptions.
        /// </summary>
        internal void RemoveSubscription(in SubscriptionId subscription)
        {
            this._subscriptions.Remove(subscription.SignalId);
        }

        #endregion
    }
}
