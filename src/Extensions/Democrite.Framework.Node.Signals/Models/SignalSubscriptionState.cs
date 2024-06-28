// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Base State use to managed signal subscription
    /// </summary>
    internal abstract class SignalSubscriptionState : IEquatable<SignalSubscriptionState>
    {
        #region Fields

        private ImmutableDictionary<IDedicatedGrainId, SignalSubscription> _subscriptions;
        private readonly ReaderWriterLockSlim _subscriptionLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSubscriptionState"/> class.
        /// </summary>
        public SignalSubscriptionState(IEnumerable<SignalSubscription> subscriptions)
        {
            this._subscriptionLocker = new ReaderWriterLockSlim();
            this._subscriptions = subscriptions?.ToImmutableDictionary(s => (IDedicatedGrainId?)s.TargetGrainId ?? (IDedicatedGrainId?)s.TargetReadOnlyGrainId ?? throw new InvalidCastException("Invalid subscriptions"))
                                        ?? ImmutableDictionary<IDedicatedGrainId, SignalSubscription>.Empty;
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

        /// <summary>
        /// Gets a value indicating whether this instance has subscriptions.
        /// </summary>
        public bool HasSubscriptions
        {
            get { return this._subscriptions.Count > 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get suscription by <paramref name="targetGrainId"/>
        /// </summary>
        public Guid? GetSuscription(DedicatedGrainId<ISignalReceiver> targetGrainId)
        {
            return GetSubscriptionImpl(targetGrainId);
        }

        /// <summary>
        /// Get suscription by <paramref name="targetGrainId"/>
        /// </summary>
        public Guid? GetSuscription(DedicatedGrainId<ISignalReceiverReadOnly> targetGrainId)
        {
            return GetSubscriptionImpl(targetGrainId);
        }

        /// <summary>
        /// Adds or update a suscriptions.
        /// </summary>
        public Guid AddOrUpdateSubscription(DedicatedGrainId<ISignalReceiver> targetGrainId)
        {
            return AddOrUpdateSubscriptionImpl(targetGrainId);
        }

        /// <summary>
        /// Adds or update a suscriptions.
        /// </summary>
        public Guid AddOrUpdateSubscription(DedicatedGrainId<ISignalReceiverReadOnly> targetGrainId)
        {
            return AddOrUpdateSubscriptionImpl(targetGrainId);
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
        }

        /// <inheritdoc />
        public bool Equals(SignalSubscriptionState? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            this._subscriptionLocker.EnterReadLock();

            try
            {
                return other.Subscriptions.SequenceEqual(this.Subscriptions) &&
                       OnEquals(other!);
            }
            finally
            {
                this._subscriptionLocker.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            this._subscriptionLocker.EnterReadLock();

            try
            {
                return HashCode.Combine(this._subscriptions.OrderBy(o => o.Value.Uid)
                                                           .Aggregate(0, (acc, kv) => acc ^ kv.Value.GetHashCode()),
                                        OnGetHashCode());
            }
            finally
            {
                this._subscriptionLocker.ExitReadLock();
            }
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected virtual int OnGetHashCode()
        {
            return 0;
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        /// <inheritdoc cref="IEquatable{SignalSubscriptionState}.Equals(SignalSubscriptionState?)" />
        /// <remarks>
        ///     Null check and reference check have already been done
        /// </remarks>
        protected virtual bool OnEquals(SignalSubscriptionState signalSubscriptionState)
        {
            return true;
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is SignalSubscriptionState other)
                return Equals(other);
            return false;
        }

        #region Tools

        /// <summary>
        /// Get suscription by <paramref name="targetGrainId"/>
        /// </summary>
        private Guid? GetSubscriptionImpl(IDedicatedGrainId targetGrainId)
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

            return null;
        }

        /// <summary>
        /// Adds or update a suscriptions.
        /// </summary>
        private Guid AddOrUpdateSubscriptionImpl(IDedicatedGrainId targetGrainId)
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

                var subscription = new SignalSubscription(Guid.NewGuid(),
                                                          targetGrainId as DedicatedGrainId<ISignalReceiver>?,
                                                          targetGrainId as DedicatedGrainId<ISignalReceiverReadOnly>?);

                this._subscriptions = this._subscriptions.Add(targetGrainId, subscription);

                return subscription.Uid;
            }
            finally
            {
                this._subscriptionLocker.ExitWriteLock();
            }
        }

        #endregion

        #endregion
    }
}
