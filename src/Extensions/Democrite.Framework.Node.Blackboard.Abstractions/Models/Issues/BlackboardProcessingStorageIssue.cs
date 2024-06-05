// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues
{
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Numerics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class for any rule issue occured during any process in the blackboard
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class BlackboardProcessingStorageIssue(DataRecordContainer NewRecord,
                                                                  BlackboardProcessingIssueTypeEnum SectorType,
                                                                  BlackboardProcessingIssueStorageTypeEnum IssueStorageType)

                                            : BlackboardProcessingIssue(SectorType | BlackboardProcessingIssueTypeEnum.Storage);

    /// <summary>
    /// Emit when adding a new record will exceed <paramref name="MaxRecordAllow"/> rule
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class ConflictBlackboardProcessingRuleIssue(IReadOnlyCollection<BlackboardRecordMetadata> ConflictRecords,
                                                              DataRecordContainer NewRecord)

                                             : BlackboardProcessingStorageIssue(NewRecord,
                                                                                BlackboardProcessingIssueTypeEnum.Storage,
                                                                                BlackboardProcessingIssueStorageTypeEnum.Conflict)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.SectorType:G}] [{this.IssueStorageType:G}] - Conflict - ({this.ConflictRecords.Count})";
        }
    }

    /// <summary>
    /// Emit when adding a new record will exceed <paramref name="MaxRecordAllow"/> rule
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class MaxRecordBlackboardProcessingRuleIssue(ushort MaxRecordAllow,
                                                               IReadOnlyCollection<BlackboardRecordMetadata> ConflictRecords,
                                                               DataRecordContainer NewRecord,
                                                               BlackboardProcessingResolutionLimitTypeEnum? PreferenceResolution,
                                                               BlackboardProcessingResolutionRemoveTypeEnum? PreferenceRemoveResolution)

                                             : BlackboardProcessingStorageIssue(NewRecord,
                                                                                BlackboardProcessingIssueTypeEnum.Rule,
                                                                                BlackboardProcessingIssueStorageTypeEnum.Limits)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.SectorType:G}] [{this.IssueStorageType:G}] - Max Record - Limit {this.MaxRecordAllow} - [PreferenceResolution : {this.PreferenceResolution}]";
        }
    }

    /// <summary>
    /// Emit when adding a new record in that conflict with record
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class UniqueBlackboardProcessingRuleIssue(DataRecordContainer NewRecord)

                                             : BlackboardProcessingStorageIssue(NewRecord,
                                                                                BlackboardProcessingIssueTypeEnum.Rule,
                                                                                BlackboardProcessingIssueStorageTypeEnum.Conflict)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.SectorType:G}] [{this.IssueStorageType:G}] - Unique'";
        }
    }


    /// <summary>
    /// Emit when adding a new record in string format that doesn't respect the regec format
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class StringRegexBlackboardProcessingRuleIssue(string regexFormat,
                                                                 DataRecordContainer NewRecord)

                                             : BlackboardProcessingStorageIssue(NewRecord,
                                                                                BlackboardProcessingIssueTypeEnum.Rule,
                                                                                BlackboardProcessingIssueStorageTypeEnum.Format)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.SectorType:G}] [{this.IssueStorageType:G}] - string regex - '{this.regexFormat}'";
        }
    }

    /// <summary>
    /// Emit when adding a new record type number doesn't follow the allowed range
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class NumberRangeBlackboardProcessingRuleIssue(double? Min,
                                                                 double? Max,                        
                                                                 DataRecordContainer NewRecord)

                                             : BlackboardProcessingStorageIssue(NewRecord,
                                                                                BlackboardProcessingIssueTypeEnum.Rule,
                                                                                BlackboardProcessingIssueStorageTypeEnum.Limits)
    {
        /// <summary>
        /// Helper to create a new <see cref="NumberRangeBlackboardProcessingRuleIssue"/>
        /// </summary>
        public static NumberRangeBlackboardProcessingRuleIssue Create<TNumber>(TNumber? min, TNumber? max, DataRecordContainer data)
            where TNumber : struct, INumber<TNumber>
        {
            double? minDouble = null;
            double? maxDouble = null;

            if (min is not null)
                minDouble = Convert.ToDouble(min);

            if (max is not null)
                maxDouble = Convert.ToDouble(max);

            return new NumberRangeBlackboardProcessingRuleIssue(minDouble, maxDouble, data);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.SectorType:G}] [{this.IssueStorageType:G}] - Number Range - [ {this.Min} -> {this.Max} [";
        }
    }

    /// <summary>
    /// Emit when adding a new record in wrong type format
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class DataTypeBlackboardProcessingRuleIssue(IConcretTypeSurrogate ExpectedType,
                                                              DataRecordContainer NewRecord)

                                             : BlackboardProcessingStorageIssue(NewRecord,
                                                                                BlackboardProcessingIssueTypeEnum.Rule | BlackboardProcessingIssueTypeEnum.Type,
                                                                                BlackboardProcessingIssueStorageTypeEnum.Format)
    {
        /// <summary>
        /// Helper to create a new <see cref="DataTypeBlackboardProcessingRuleIssue"/>
        /// </summary>
        public static DataTypeBlackboardProcessingRuleIssue Create(Type type, DataRecordContainer data)
        {
            return new DataTypeBlackboardProcessingRuleIssue(ConcretBaseTypeConverter.ConvertToSurrogate((ConcretType)type.GetAbstractType()),
                                                             data);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.SectorType:G}] [{this.IssueStorageType:G}] - Data Type - '{this.ExpectedType.AssemblyQualifiedName}'";
        }
    }
}
