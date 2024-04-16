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
            get 
            {
                if (this._indexedEvents.TryGetValue(BlackboardEventTypeEnum.Storage, out var events))
                    return events.Count;

                return 0;
            }
        }

        #endregion

        #region Methods

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
            if (this._indexedEvents.TryGetValue(BlackboardEventTypeEnum.Storage, out var storages) && storages is not null)
            {
                var events = storages.Where(kv => kv.Event is BlackboardEventStorage);

                if (filter is not null)
                    events = events.Where(kv => filter((BlackboardEventStorage)kv.Event));

                return events.OrderBy(e => e.Index)
                             .Select(e => e.Event)
                             .Cast<BlackboardEventStorage>()
                             .ToArray();
            }

            return EnumerableHelper<BlackboardEventStorage>.ReadOnly;
        }

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
