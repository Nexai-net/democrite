﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Models
{
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Base State use to managed signal subscription
    /// </summary>
    internal abstract class SignalSubscriptionState
    {
        #region Fields

        private ImmutableDictionary<GrainId, SignalSubscription> _subscriptions;
        private readonly ReaderWriterLockSlim _subscriptionLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSubscriptionState"/> class.
        /// </summary>
        public SignalSubscriptionState(IEnumerable<SignalSubscription> subscriptions)
        {
            this._subscriptionLocker = new ReaderWriterLockSlim();
            this._subscriptions = subscriptions?.ToImmutableDictionary(s => s.TargetGrainId)
                                        ?? ImmutableDictionary<GrainId, SignalSubscription>.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the subscriptions.
        /// </summary>
        public IEnumerable<SignalSubscription> Subscriptions
        {
            get { return this._subscriptions.Values; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds or update a suscriptions.
        /// </summary>
        public Guid AddOrUpdateSuscription(GrainId targetGrainId)
        {
            this._subscriptionLocker.EnterReadLock();

            try
            {
                if (this._subscriptions.TryGetValue(targetGrainId, out var existingSubscription))
                    return existingSubscription.Uid;
            }
            finally
            {
                this._subscriptionLocker.ExitReadLock();
            }

            this._subscriptionLocker.EnterWriteLock();

            try
            {
                if (this._subscriptions.TryGetValue(targetGrainId, out var existingSubscription))
                    return existingSubscription.Uid;

                var subscription = new SignalSubscription(Guid.NewGuid(), targetGrainId);

                this._subscriptions = this._subscriptions.Add(subscription.TargetGrainId, subscription);

                return subscription.Uid;
            }
            finally
            {
                this._subscriptionLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes suscription by his id if exists
        /// </summary>
        public bool RemoveSuscription(Guid subscritionId)
        {
            this._subscriptionLocker.EnterWriteLock();

            try
            {
                var existingSubscription = this._subscriptions.FirstOrDefault(kv => kv.Value.Uid == subscritionId);

                if (existingSubscription.Value != null)
                    this._subscriptions = this._subscriptions.Remove(existingSubscription.Key);

                return false;
            }
            finally
            {
                this._subscriptionLocker.ExitWriteLock();

            }

            #endregion
        }
    }
}
