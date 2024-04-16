namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using Elvex.Toolbox.Abstractions.Conditions;

    using Orleans.Concurrency;

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

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
        /// Sealed the blackboard if possible; Maintains the minimal information needed to respond to query based blackboard goals
        /// </summary>
        Task<bool> SealedAsync(GrainCancellationToken token);

        /// <summary>
        /// Initialize the bloackboard. The initialization is allow only once and could insert init data.
        /// </summary>
        Task<bool> InitializeAsync(GrainCancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null);

        /// <summary>
        /// Gets blackboard life status
        /// </summary>
        Task<BlackboardLifeStatusEnum> GetStatusAsync(GrainCancellationToken token);

        /// <summary>
        /// Send query type command without expecting any result
        /// </summary>
        Task<BlackboardQueryResponse> QueryAsync(BlackboardQueryCommand command, GrainCancellationToken token);

        /// <summary>
        /// Send query request with response expected
        /// </summary>
        Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, GrainCancellationToken token);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, GrainCancellationToken token);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, GrainCancellationToken token);

        /// <summary>
        /// Delete data
        /// </summary>
        Task<bool> DeleteDataAsync(GrainCancellationToken token, IIdentityCard identity, params Guid[] slotIds);

        /// <summary>
        /// Pushes the a data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, GrainCancellationToken token);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUids"/>
        /// </summary>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, GrainCancellationToken token);

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
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, GrainCancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, GrainCancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, GrainCancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter, GrainCancellationToken token);

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
        /// Sealed the blackboard if possible; Maintains the minimal information needed to respond to query based blackboard goals
        /// </summary>
        Task<bool> SealedAsync(CancellationToken token);

        /// <summary>
        /// Initialize the bloackboard. The initialization is allow only once and could insert init data.
        /// </summary>
        Task<bool> InitializeAsync(CancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null);

        /// <summary>
        /// Gets blackboard life status
        /// </summary>
        Task<BlackboardLifeStatusEnum> GetStatusAsync(CancellationToken token);

        /// <summary>
        /// Send query type command without expecting any result
        /// </summary>
        Task<BlackboardQueryResponse> QueryAsync(BlackboardQueryCommand command, CancellationToken token);

        /// <summary>
        /// Send query request with response expected
        /// </summary>
        Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, CancellationToken token);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, CancellationToken token);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, CancellationToken token);

        /// <summary>
        /// Delete data
        /// </summary>
        Task<bool> DeleteDataAsync(CancellationToken token, IIdentityCard identity, params Guid[] slotIds);

        /// <summary>
        /// Pushes the a data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUids"/>
        /// </summary>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, CancellationToken token);

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
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter, CancellationToken token);
        /// <summary>
        /// Pushes new or update the data with id <paramref name="uid"/>
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(TData? record,
                                        Guid uid,
                                        [NotNull] string logicType,
                                        string? displayName = null,
                                        RecordStatusEnum recordStatus = RecordStatusEnum.Ready,
                                        DataRecordPushRequestTypeEnum pushType = DataRecordPushRequestTypeEnum.Push,
                                        RecordMetadata? customMetadata = null,
                                        CancellationToken token = default);

        /// <summary>
        /// Initialize the bloackboard. The initialization is allow only once and could insert init data.
        /// </summary>
        Task<bool> InitializeAsync(CancellationToken token, params DataRecordContainer[] initData);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUid"/>
        /// </summary>
        Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, CancellationToken token);

        /// <summary>
        /// Pushes a new data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushNewDataAsync<TData>(TData? record,
                                   [NotNull] string logicType,
                                   string? displayName = null,
                                   RecordStatusEnum recordStatus = RecordStatusEnum.Ready,
                                   DataRecordPushRequestTypeEnum pushType = DataRecordPushRequestTypeEnum.Push,
                                   RecordMetadata? customMetadata = null,
                                   CancellationToken token = default);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(Expression<Func<BlackboardRecordMetadata, bool>> filter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(Expression<Func<BlackboardRecordMetadata, bool>> filter, CancellationToken token);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUids"/>
        /// </summary>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(CancellationToken token, params Guid[] dataUids);

        #endregion
    }
}
