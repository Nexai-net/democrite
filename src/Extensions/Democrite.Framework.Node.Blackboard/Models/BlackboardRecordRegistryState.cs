// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Models.Surrogates;

    using Elvex.Toolbox.Abstractions.Services;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public sealed class BlackboardRecordRegistryState
    {
        #region Fields

        private readonly Dictionary<Guid, BlackboardRecordMetadata> _recordRegistry;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardRecordRegistryState"/> class.
        /// </summary>
        internal BlackboardRecordRegistryState(IEnumerable<BlackboardRecordMetadata> blackboardRecords)
        {
            this._recordRegistry = blackboardRecords?.ToDictionary(k => k.Uid, KeyValuePair => KeyValuePair) ?? new Dictionary<Guid, BlackboardRecordMetadata>();
            this.RecordMetadatas = new ReadOnlyDictionary<Guid, BlackboardRecordMetadata>(this._recordRegistry);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the record metadata.
        /// </summary>
        public IReadOnlyDictionary<Guid, BlackboardRecordMetadata> RecordMetadatas { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        internal BlackboardRecordRegistryStateSurrogate ToSurrogate()
        {
            return new BlackboardRecordRegistryStateSurrogate()
            {
                RecordMetadatas = this._recordRegistry.Values.ToArray()
            };
        }

        /// <summary>
        /// Pushes a new record
        /// </summary>
        internal BlackboardRecordMetadata Push<TData>(DataRecordContainer<TData> record, ITimeManager timeManager)
        {
            BlackboardRecordMetadata exist;
            var foundExisting = this._recordRegistry.TryGetValue(record.Uid, out exist);

            var newEntry = new BlackboardRecordMetadata(record.Uid,
                                                        record.LogicalType,
                                                        record.DisplayName,
                                                        record.ContainsType is not null ? ConcretBaseTypeConverter.ConvertToSurrogate(record.ContainsType) : null,
                                                        record.RecordContainerType,
                                                        record.Status,
                                                        foundExisting ? exist.UTCCreationTime : record.UTCCreationTime,
                                                        foundExisting ? exist.CreatorIdentity : record.CreatorIdentity,
                                                        timeManager.UtcNow,
                                                        record.LastUpdaterIdentity,
                                                        record.CustomMetadata);

            this._recordRegistry[record.Uid] = newEntry;
            return newEntry;
        }

        /// <summary>
        /// Pushes a record by his unique id
        /// </summary>
        internal BlackboardRecordMetadata? Pop(Guid uid)
        {
            if (this._recordRegistry.TryGetValue(uid, out var entry))
            {
                this._recordRegistry.Remove(uid);
                return entry;
            }

            return null;
        }

        /// <summary>
        /// Pushes a record by his unique id
        /// </summary>
        internal BlackboardRecordMetadata? ChangeStatus(Guid uid, RecordStatusEnum recordStatus, DateTime utcNow)
        {
            if (this._recordRegistry.TryGetValue(uid, out var entry))
            {
                var newEntry = new BlackboardRecordMetadata(entry.Uid,
                                                            entry.LogicalType,
                                                            entry.DisplayName,
                                                            entry.ContainsType,
                                                            entry.RecordContainerType,
                                                            recordStatus,
                                                            entry.UTCCreationTime,
                                                            entry.CreatorIdentity,
                                                            utcNow,
                                                            entry.LastUpdaterIdentity,
                                                            entry.CustomMetadata);
                this._recordRegistry[uid] = newEntry;
                return newEntry;
            }

            return null;
        }

        /// <summary>
        /// Changes the meta data.
        /// </summary>
        internal BlackboardRecordMetadata? ChangeMetaData(Guid uid, RecordMetadata newMetaData, DateTime utcNow)
        {
            if (this._recordRegistry.TryGetValue(uid, out var entry))
            {
                var newEntry = new BlackboardRecordMetadata(entry.Uid,
                                                            entry.LogicalType,
                                                            entry.DisplayName,
                                                            entry.ContainsType,
                                                            entry.RecordContainerType,
                                                            entry.Status,
                                                            entry.UTCCreationTime,
                                                            entry.CreatorIdentity,
                                                            utcNow,
                                                            entry.LastUpdaterIdentity,
                                                            newMetaData);
                this._recordRegistry[uid] = newEntry;
                return newEntry;
            }

            return null;
        }

        #endregion
    }
}
