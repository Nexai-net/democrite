// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using System;

    [GenerateSerializer]
    public record struct MetaDataRecordContainerSurrogate(string LogicalType,
                                                          Guid Uid,
                                                          string DisplayName,
                                                          IConcretTypeSurrogate? ContainsType,
                                                          RecordStatusEnum Status,
                                                          DateTime UTCCreationTime,
                                                          string? CreatorIdentity,
                                                          DateTime UTCLastUpdateTime,
                                                          string? LastUpdaterIdentity,
                                                          RecordMetadata? CustomMetadata);

    [RegisterConverter]
    public sealed class MetaDataRecordContainerConverter : IConverter<MetaDataRecordContainer, MetaDataRecordContainerSurrogate>
    {
        /// <inheritdoc />
        public MetaDataRecordContainer ConvertFromSurrogate(in MetaDataRecordContainerSurrogate surrogate)
        {
            return new MetaDataRecordContainer(surrogate.LogicalType,
                                               surrogate.Uid,
                                               surrogate.DisplayName,
                                               surrogate.ContainsType is null ? null : ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.ContainsType!),
                                               surrogate.Status,
                                               surrogate.UTCCreationTime,
                                               surrogate.CreatorIdentity,
                                               surrogate.UTCLastUpdateTime,
                                               surrogate.LastUpdaterIdentity,
                                               surrogate.CustomMetadata);
        }

        /// <inheritdoc />
        public MetaDataRecordContainerSurrogate ConvertToSurrogate(in MetaDataRecordContainer value)
        {
            return new MetaDataRecordContainerSurrogate()
            {
                ContainsType = value.ContainsType is null ? null : ConcretBaseTypeConverter.ConvertToSurrogate(value.ContainsType),
                Status = value.Status,
                DisplayName = value.DisplayName,
                LogicalType = value.LogicalType,
                LastUpdaterIdentity = value.LastUpdaterIdentity,
                UTCLastUpdateTime = value.UTCLastUpdateTime,
                Uid = value.Uid,
                CreatorIdentity = value.CreatorIdentity,
                UTCCreationTime = value.UTCCreationTime,
                CustomMetadata = value.CustomMetadata
            };
        }
    }
}
