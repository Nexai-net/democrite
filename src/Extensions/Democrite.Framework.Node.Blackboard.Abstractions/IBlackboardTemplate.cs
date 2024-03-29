﻿namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using System;
    using System.Threading.Tasks;
    using System.Diagnostics.CodeAnalysis;

    using Orleans.Concurrency;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    /// <summary>
    /// A blackboard is a shared space through all the cluster able to:
    ///  - Store data index by type and unique identifier
    ///  - Record modification history
    ///  - Use a controller to managed input data to resolve a specific goal
    /// 
    /// A black board have a unique Uid or could be identity by the pair Name + TemplateName
    /// </summary>
    public interface IBlackboard
    {
        #region Methods

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, GrainCancellationToken token);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, GrainCancellationToken token);

        /// <summary>
        /// Pushes the a data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, GrainCancellationToken token);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUid"/>
        /// </summary>
        Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, GrainCancellationToken token);

        /// <summary>
        /// Gets all stored's data metadata (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(GrainCancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataByTypeAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, GrainCancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, GrainCancellationToken token);

        #endregion
    }

    /// <summary>
    /// Proxy reference to communicate and control a <see cref="IBlackboard"/>
    /// </summary>
    public interface IBlackboardRef
    {

        #region Properties

        /// <summary>
        /// Gets a Blackboard unique identifier
        /// </summary>
        Guid Uid { get; }

        /// <summary>
        /// Gets a Blackboard unique identifier name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a Blackboard template
        /// </summary>
        string TemplateName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, CancellationToken token);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, CancellationToken token);

        /// <summary>
        /// Pushes the a data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUid"/>
        /// </summary>
        Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, CancellationToken token);

        /// <summary>
        /// Gets all stored's data metadata (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataByTypeAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, CancellationToken token);

        /// <summary>
        /// Pushes new or update the data with id <paramref name="uid"/>
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(TData? record, Guid uid, string logicType, string displayName, RecordStatusEnum recordStatus, DataRecordPushRequestTypeEnum pushType, CancellationToken token);

        /// <summary>
        /// Pushes a new data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushNewDataAsync<TData>(TData? record, [NotNull] string logicType, string displayName, RecordStatusEnum? recordStatus, DataRecordPushRequestTypeEnum pushType, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataByTypeAsync(string? logicTypeFilter, CancellationToken token);

        #endregion

    }
}
