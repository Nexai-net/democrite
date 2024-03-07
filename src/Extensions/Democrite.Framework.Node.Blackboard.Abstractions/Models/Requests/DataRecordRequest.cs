// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using System;

    /// <summary>
    /// Define a target information used to create, pull, push or update data
    /// </summary>
    [GenerateSerializer]
    public record struct DataRecordRequest(Guid BoardUid,
                                           DataRecordRequestTypeEnum ActionType) : IDataRecordRequest;

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    [GenerateSerializer]
    public record struct PushDataRecordRequest(Guid BoardUid,
                                               DataRecordPushRequestTypeEnum PushActionType,
                                               Guid? TargetUid,
                                               string? LogicalRecordType,
                                               RecordStatusEnum? NewRecordStatus) : IPushDataRecordRequest
    {
        /// <inheritdoc />
        public DataRecordRequestTypeEnum ActionType
        {
            get { return DataRecordRequestTypeEnum.Push; }
        }
    }

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    [GenerateSerializer]
    public record struct PushDataRecordRequest<TData>(Guid BoardUid,
                                                      DataRecordPushRequestTypeEnum PushActionType,
                                                      Guid? TargetUid,
                                                      string? LogicalRecordType,
                                                      RecordStatusEnum? NewRecordStatus,
                                                      TData? Data) : IPushDataRecordRequest<TData>
    {
        /// <inheritdoc />
        public DataRecordRequestTypeEnum ActionType
        {
            get { return DataRecordRequestTypeEnum.Push; }
        }
    }

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    [GenerateSerializer]
    public record struct PullDataRecordRequest(Guid BoardUid,
                                               Guid? DataTargetUid,
                                               string? LogicalRecordTypePattern,
                                               RecordStatusEnum? RecordStatusFilter) : IPullDataRecordRequest
    {
        /// <inheritdoc />
        public DataRecordRequestTypeEnum ActionType
        {
            get { return DataRecordRequestTypeEnum.Pull; }
        }
    }

    /// <summary>
    /// Define a target information used to push an information
    /// </summary>
    /// <seealso cref="IDataRecordRequest" />
    [GenerateSerializer]
    public record struct PullDataRecordRequest<TDataSource>(Guid BoardUid,
                                                            Guid? DataTargetUid,
                                                            string? LogicalRecordTypePattern,
                                                            RecordStatusEnum? RecordStatusFilter,
                                                            TDataSource? Source) : IPullDataRecordRequest<TDataSource>
    {
        /// <inheritdoc />
        public DataRecordRequestTypeEnum ActionType
        {
            get { return DataRecordRequestTypeEnum.Pull; }
        }
    }
}
