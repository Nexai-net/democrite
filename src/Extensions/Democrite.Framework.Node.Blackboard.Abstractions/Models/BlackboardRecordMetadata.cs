// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using System;

    /// <summary>
    /// Define a record meta-data
    /// </summary>
    [GenerateSerializer]
    public record struct BlackboardRecordMetadata(Guid Uid,
                                                  string LogicalType,
                                                  string DisplayName,
                                                  IConcretTypeSurrogate? ContainsType,
                                                  RecordContainerTypeEnum RecordContainerType,
                                                  RecordStatusEnum Status,
                                                  DateTime UTCCreationTime,
                                                  string? CreatorIdentity,
                                                  DateTime UTCLastUpdateTime,
                                                  string? LastUpdaterIdentity,
                                                  RecordMetadata? CustomMetadata);
}
