// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [GenerateSerializer]
    public struct DataRecordContainerSurrogate<TData>
    {
        [Id(0)]
        [DataMember]
        public string LogicalType { get; set; }

        [Id(1)]
        [DataMember]
        public Guid Uid { get; set; }

        [Id(2)]
        [DataMember]
        public string DisplayName { get; set; }

        [Id(3)]
        [DataMember]
        public RecordStatusEnum Status { get; set; }

        [Id(4)]
        [DataMember]
        public DateTime UTCCreationTime { get; set; }

        [Id(5)]
        [DataMember]
        public string? CreatorIdentity { get; set; }

        [Id(6)]
        [DataMember]
        public DateTime UTCLastUpdateTime { get; set; }

        [Id(7)]
        [DataMember]
        public string? LastUpdaterIdentity { get; set; }

        [Id(8)]
        [DataMember]
        public TData? Data { get; set; }

        [Id(9)]
        [DataMember]
        public RecordContainerTypeEnum RecordContainerType { get; set; }

        [Id(10)]
        [DataMember]
        public RecordMetadata? CustomMetadata { get; set; }
    }

    [RegisterConverter]
    public sealed class DataRecordContainerConvert<TData> : IConverter<DataRecordContainer<TData>, DataRecordContainerSurrogate<TData>>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DataRecordContainerConvert{TData}"/> class.
        /// </summary>
        static DataRecordContainerConvert()
        {
            Default = new DataRecordContainerConvert<TData>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static DataRecordContainerConvert<TData> Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public DataRecordContainer<TData> ConvertFromSurrogate(in DataRecordContainerSurrogate<TData> surrogate)
        {
            return new DataRecordContainer<TData>(surrogate.LogicalType,
                                                  surrogate.Uid,
                                                  surrogate.DisplayName,
                                                  surrogate.Data,
                                                  surrogate.Status,
                                                  surrogate.UTCCreationTime,
                                                  surrogate.CreatorIdentity,
                                                  surrogate.UTCLastUpdateTime,
                                                  surrogate.LastUpdaterIdentity,
                                                  surrogate.CustomMetadata);
        }

        /// <inheritdoc />
        public DataRecordContainerSurrogate<TData> ConvertToSurrogate(in DataRecordContainer<TData> value)
        {
            return new DataRecordContainerSurrogate<TData>()
            {
                Status = value.Status,
                Data = value.Data,
                DisplayName = value.DisplayName,
                RecordContainerType = value.RecordContainerType,
                LogicalType = value.LogicalType,
                Uid = value.Uid,
                CreatorIdentity = value.CreatorIdentity,
                UTCCreationTime = value.UTCCreationTime,
                LastUpdaterIdentity = value.LastUpdaterIdentity,
                UTCLastUpdateTime = value.UTCLastUpdateTime,
                CustomMetadata = value.CustomMetadata
            };
        }

        #endregion
    }
}
