// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets
{
    using System;
    using System.Data.Common;

    /// <summary>
    /// Define a target information used to create, pull, push or update data
    /// </summary>
    public interface IDataRecordRequest
    {
        /// <summary>
        /// Gets direct board instance uid.
        /// </summary>
        public Guid BoardUid { get; }

        /// <summary>
        /// Gets the type of pull action.
        /// </summary>
        public DataRecordRequestTypeEnum ActionType { get; }
    }

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    public interface IPushDataRecordRequest : IDataRecordRequest
    {
        /// <summary>
        /// Gets the data target uid; To use as a new value push set this value to null
        /// </summary>
        public Guid? TargetUid { get; }

        /// <summary>
        /// Gets the logical record type
        /// </summary>
        public string? LogicalRecordType { get; }

        /// <summary>
        ///     Get the new value to inject for data; Default value <see cref="RecordStatusEnum.Ready"/>
        /// </summary>
        /// <remarks>
        ///     If multiple value are used (flag) the push will be rejected
        /// </remarks>
        public RecordStatusEnum? NewRecordStatus { get; }

        /// <summary>
        /// Gets the type of pull action.
        /// </summary>
        public DataRecordPushRequestTypeEnum PushActionType { get; }
    }

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    public interface IPushDataRecordRequest<out TData> : IPushDataRecordRequest
    {
        /// <summary>
        /// Gets the source responsible of the request
        /// </summary>
        TData? Data { get; }
    }

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    public interface IPullDataRecordRequest : IDataRecordRequest
    {
        /// <summary>
        /// Gets the data target uid searched
        /// </summary>
        public Guid? DataTargetUid { get; }

        /// <summary>
        /// Gets the logical record type pattern to record all the matching record
        /// </summary>
        public string? LogicalRecordTypePattern { get; }

        /// <summary>
        ///     Get the state to filter; If null the filter not applied
        /// </summary>
        /// <remarks>
        ///     Could be cumulated by flag
        /// </remarks>
        public RecordStatusEnum? RecordStatusFilter { get; }
    }

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    public interface IPullDataRecordRequest<out TDataSource> : IPullDataRecordRequest
    {
        /// <summary>
        /// Gets the source responsible of the request
        /// </summary>
        TDataSource? Source { get; }
    }
}
