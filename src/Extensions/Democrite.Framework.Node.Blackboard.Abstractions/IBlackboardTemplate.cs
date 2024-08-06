namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Models;

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
        Task<bool> SealedAsync(GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Initialize the bloackboard. The initialization is allow only once and could insert init data.
        /// </summary>
        Task<bool> InitializeAsync(GrainCancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null, Guid? callContextId = null);

        /// <summary>
        /// Gets blackboard life status
        /// </summary>
        [ReadOnly]
        Task<BlackboardLifeStatusEnum> GetStatusAsync(GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Send query type command without expecting any result
        /// </summary>
        Task<BlackboardQueryResponse?> QueryAsync(BlackboardQueryCommand command, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Send query type command without expecting any result
        /// </summary>
        [OneWay]
        Task FireQueryAsync(BlackboardQueryCommand command, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Send query request with response expected
        /// </summary>
        Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Delete data
        /// </summary>
        Task<bool> DeleteDataAsync(GrainCancellationToken token, Guid? callContextId, IIdentityCard identity, params Guid[] slotIds);

        /// <summary>
        /// Pushes the a data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUids"/>
        /// </summary>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets all stored's data metadata (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>) associate to specific <paramref name="dataUids"/>
        /// </summary>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetStoredMetaDataAsync(IReadOnlyCollection<Guid> dataUids, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets all stored's data metadata (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, uint? limit, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, uint? limit, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, uint? limit, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter, uint? limit, GrainCancellationToken token, Guid? callContextId = null);

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
        Task<bool> SealedAsync(CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Initialize the bloackboard. The initialization is allow only once and could insert init data.
        /// </summary>
        Task<bool> InitializeAsync(CancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null, Guid? callContextId = null);

        /// <summary>
        /// Gets blackboard life status
        /// </summary>
        [ReadOnly]
        Task<BlackboardLifeStatusEnum> GetStatusAsync(CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Send query type command without expecting any result
        /// </summary>
        Task<BlackboardQueryResponse?> QueryAsync(BlackboardQueryCommand command, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Send query type command without expecting any result
        /// </summary>
        [OneWay]
        Task FireQueryAsync(BlackboardQueryCommand command, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Send query request with response expected
        /// </summary>
        Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Pushes the data asynchronous.
        /// </summary>
        Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Delete data
        /// </summary>
        Task<bool> DeleteDataAsync(CancellationToken token, Guid? callContextId, IIdentityCard identity, params Guid[] slotIds);

        /// <summary>
        /// Pushes the a data
        /// </summary>
        /// <returns>
        ///    Return value associate to Data uid record; otherwise null is an issue occured
        /// </returns>
        Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUids"/>
        /// </summary>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets all stored's data metadata (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>) associate to specific <paramref name="dataUids"/>
        /// </summary>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetStoredMetaDataAsync(IReadOnlyCollection<Guid> dataUids, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets all stored's data metadata (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, uint? limit, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, uint? limit, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? statusFilter, uint? limit, CancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        [ReadOnly]
        [AlwaysInterleave]
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter, uint? limit, CancellationToken token, Guid? callContextId = null);
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
                                        CancellationToken token = default,
                                        Guid? callContextId = null);

        /// <summary>
        /// Initialize the bloackboard. The initialization is allow only once and could insert init data.
        /// </summary>
        Task<bool> InitializeAsync(CancellationToken token, Guid? callContextId, params DataRecordContainer[] initData);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUid"/>
        /// </summary>
        Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, CancellationToken token, Guid? callContextId = null);

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
                                   CancellationToken token = default, 
                                   Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, CancellationToken token, uint? limit = null, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, CancellationToken token, uint? limit = null, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/>
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(Expression<Func<BlackboardRecordMetadata, bool>> filter, CancellationToken token, uint? limit = null, Guid? callContextId = null);

        /// <summary>
        /// Gets all stored's data metadata (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>) associate to specific <paramref name="dataUids"/>
        /// </summary>
        [AlwaysInterleave]
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetStoredMetaDataAsync(Guid? callContextId, CancellationToken token, params Guid[] dataUids);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter, CancellationToken token, uint? limit = null, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate with a matching logic type <paramref name="logicTypeFilter"/> (<see cref="DataRecordContainer.RecordContainerType"/> == <see cref="RecordContainerTypeEnum.MetaData"/>)
        /// </summary>
        /// <param name="logicTypeFilter">Filter that support regex expression.</param>
        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(Expression<Func<BlackboardRecordMetadata, bool>> filter, CancellationToken token, uint? limit = null, Guid? callContextId = null);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUids"/>
        /// </summary>
        Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(Guid? callContextId, CancellationToken token, params Guid[] dataUids);

        /// <summary>
        /// Gets the stored data associate to specific <paramref name="dataUids"/>
        /// </summary>
        Task<IReadOnlyCollection<DataRecordContainer>> GetStoredDataAsync(ConcretBaseType dataProjectionType, Guid? callContextId, CancellationToken token, params Guid[] dataUids);

        #endregion
    }
}
