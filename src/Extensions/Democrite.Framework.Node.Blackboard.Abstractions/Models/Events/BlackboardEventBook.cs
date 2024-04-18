// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Events
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Collection of <see cref="BlackboardEvent"/> with easy feature to search and exploit events
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class BlackboardEventBook
    {
        #region Fields

        private readonly Dictionary<BlackboardEventTypeEnum, IReadOnlyCollection<(BlackboardEvent Event, int Index)>> _indexedEvents;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardEventBook"/> class.
        /// </summary>
        public BlackboardEventBook(IEnumerable<BlackboardEvent> events)
        {
            this.Events = events?.ToArray() ?? EnumerableHelper<BlackboardEvent>.ReadOnlyArray;

            this._indexedEvents = this.Events.Select((e, indx) => (e, indx))
                                             .GroupBy(kv => kv.e.EventType)
                                             .ToDictionary(k => k.Key, kv => kv.Select(ei => (Event: ei.e, Index: ei.indx)).ToReadOnly());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the events.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<BlackboardEvent> Events { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        [DataMember]
        public int Count
        {
            get { return this.Events.Count; }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        [IgnoreDataMember]
        public int StorageEventCount
        {
            get { return CountEvents(BlackboardEventTypeEnum.Storage); }
        }

        /// <summary>
        /// Gets the count of LifeStatusChanged events.
        /// </summary>
        [IgnoreDataMember]
        public int LifeStatusChangedEventCount
        {
            get { return CountEvents(BlackboardEventTypeEnum.LifeStatusChanged); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the number of events related to <paramref name="eventType"/>
        /// </summary>
        public int CountEvents(BlackboardEventTypeEnum eventType)
        {
            if (this._indexedEvents.TryGetValue(eventType, out var events))
                return events.Count;

            return 0;
        }

        /// <summary>
        /// Gets the event storages filtered by type
        /// </summary>
        public IReadOnlyCollection<BlackboardEventStorage> GetEventStorages(BlackboardEventStorageTypeEnum action)
        {
            return GetEventStorages(e => e.Action == action);
        }

        /// <summary>
        /// Gets the event storages filtered
        /// </summary>
        public IReadOnlyCollection<BlackboardEventStorage> GetEventStorages(Func<BlackboardEventStorage, bool>? filter = null)
        {
            return GetFilteredEvents(BlackboardEventTypeEnum.Storage, filter);
        }

        /// <summary>
        /// Gets the event storages filtered by type
        /// </summary>
        public IReadOnlyCollection<BlackboardEventLifeStatusChanged> GetLifeStatusChangedEvent(BlackboardLifeStatusEnum status)
        {
            return GetLifeStatusChangedEvent(e => e.NewStatus == status);
        }

        /// <summary>
        /// Gets the event storages filtered
        /// </summary>
        public IReadOnlyCollection<BlackboardEventLifeStatusChanged> GetLifeStatusChangedEvent(Func<BlackboardEventLifeStatusChanged, bool>? filter = null)
        {
            return GetFilteredEvents(BlackboardEventTypeEnum.LifeStatusChanged, filter);
        }

        #region Toolbox

        /// <summary>
        /// Gets the event storages filtered
        /// </summary>
        private IReadOnlyCollection<TEvent> GetFilteredEvents<TEvent>(BlackboardEventTypeEnum eventType, Func<TEvent, bool>? filter = null)
            where TEvent : BlackboardEvent
        {
            if (this._indexedEvents.TryGetValue(eventType, out var allEvents) && allEvents is not null)
            {
                var events = allEvents.Where(kv => kv.Event is TEvent);

                if (filter is not null)
                    events = events.Where(kv => filter((TEvent)kv.Event));

                return events.OrderBy(e => e.Index)
                             .Select(e => e.Event)
                             .Cast<TEvent>()
                             .ToArray();
            }

            return EnumerableHelper<TEvent>.ReadOnly;
        }

        #endregion

        #endregion
    }

    [GenerateSerializer]
    public record struct BlackboardEventBookSurrogate(IEnumerable<BlackboardEvent> Events);

    [RegisterConverter]
    public sealed class BlackboardEventBookConverter : IConverter<BlackboardEventBook, BlackboardEventBookSurrogate>
    {
        /// <inheritdoc />
        public BlackboardEventBook ConvertFromSurrogate(in BlackboardEventBookSurrogate surrogate)
        {
            return new BlackboardEventBook(surrogate.Events);
        }

        /// <inheritdoc />
        public BlackboardEventBookSurrogate ConvertToSurrogate(in BlackboardEventBook value)
        {
            return new BlackboardEventBookSurrogate(value.Events);
        }
    }
}
