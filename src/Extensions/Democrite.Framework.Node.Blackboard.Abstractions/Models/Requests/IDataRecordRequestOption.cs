// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Requests
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using System;

    /// <summary>
    /// Option used to configure a blackdoard's push/pull request behavior
    /// </summary>
    public interface IDataRecordRequestOption
    {
        /// <summary>
        /// Gets or sets the board identifier.
        /// </summary>
        public Guid? BoardId { get; }

        /// <summary>
        /// Gets or sets the board identifier.
        /// </summary>
        /// <remarks>
        ///     If this option is used to identity a blackboard the template MUST also be set
        /// </remarks>
        public string? BoardName { get; }

        /// <summary>
        /// Gets the board template identifier.
        /// </summary>
        /// <remarks>
        ///     If this option is used to identity a blackboard the name MUST also be set
        /// </remarks>
        public string? BoardTemplateName { get; }

        /// <summary>
        /// Gets or sets a value to block extraction from data stored in the <see cref="IExecutionContext"/>
        /// </summary>
        public bool DontExtractDataFromContext { get; }
    }

    /// <summary>
    /// Option used to configure the blackdoard pull behavior
    /// </summary>
    public interface IDataRecordPullRequestOption : IDataRecordRequestOption
    {
        /// <summary>
        ///     Get the state to filter; If null the filter not applied
        /// </summary>
        /// <remarks>
        ///     Could be cumulated by flag
        /// </remarks>
        public RecordStatusEnum? RecordStatusFilter { get; }

        /// <summary>
        /// Gets the maximum indicating the maximum of item to get.
        /// </summary>
        public int? MaxItems { get; }

        /// <summary>
        /// Gets logical's type pattern to get data from
        /// </summary>
        public string? LogicalTypePattern { get; }
    }

    /// <summary>
    /// Option used to configure the blackdoard pull behavior
    /// </summary>
    public interface IDataRecordPushRequestOption : IDataRecordRequestOption
    {
        /// <summary>
        /// Gets a value indicating whether [force new value].
        /// </summary>
        bool ForceValueCreation { get; }

        /// <summary>
        /// Gets logical's type to set for the target entity pushed.
        /// </summary>
        public string? LogicalType { get; }

        /// <summary>
        /// Gets the type of pull action.
        /// </summary>
        public DataRecordPushRequestTypeEnum PushActionType { get; }

        /// <summary>
        ///     Get the new value to inject for data; Default value <see cref="RecordStatusEnum.Ready"/>
        /// </summary>
        /// <remarks>
        ///     If multiple value are used (flag) the push will be rejected
        /// </remarks>
        public RecordStatusEnum? NewRecordStatus { get; }
    }
}
