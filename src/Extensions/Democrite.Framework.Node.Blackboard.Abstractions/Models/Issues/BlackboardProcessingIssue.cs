// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues
{
    using Elvex.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class for any issue occured during any process in the blackboard
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class BlackboardProcessingIssue(BlackboardProcessingIssueTypeEnum SectorType) : ISupportDebugDisplayName
    {
        /// <inheritdoc />
        public abstract string ToDebugDisplayName();
    }
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardAggregateProcessingIssue(IReadOnlyCollection<BlackboardProcessingIssue> Issues) : BlackboardProcessingIssue(BlackboardProcessingIssueTypeEnum.Aggregate)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return "Issues [Aggregate:{0}] :\n {1}".WithArguments(this.Issues.Count, string.Join("\n", this.Issues.Select(i => i.ToDebugDisplayName())));
        }
    }

    /// <summary>
    /// Not supported process issue
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardNotSupportedProcessingIssue() : BlackboardProcessingIssue(BlackboardProcessingIssueTypeEnum.NotSupported)
    {
        #region Ctor        

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static BlackboardNotSupportedProcessingIssue()
        {
            Default = new BlackboardNotSupportedProcessingIssue();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static BlackboardNotSupportedProcessingIssue Default { get; }

        #endregion

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return "NotSupported";
        }
    }

    /// <summary>
    /// Aggregate class for any issue occured during any process in the blackboard
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardProcessingAggregateIssues(string Details, IReadOnlyCollection<BlackboardProcessingIssue> Issues) 
                            : BlackboardProcessingIssue(BlackboardProcessingIssueTypeEnum.Aggregate)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"Aggregate {this.Details} : {Environment.NewLine}{string.Join(Environment.NewLine, this.Issues.Select(i => i.ToDebugDisplayName()))}";
        }
    }

    /// <summary>
    /// Generic issue about rule
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardProcessingGenericRuleIssues(string Details)
                            : BlackboardProcessingIssue(BlackboardProcessingIssueTypeEnum.Rule)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.SectorType:G} - Generic Rule - {this.Details}";
        }
    }
}
