// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
    using Democrite.Framework.Node.Blackboard.Models;

    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Default controller in charge to provide basic algorithme 
    /// </summary>
    /// <seealso cref="IDefaultBlackboardControllerGrain" />
    public class DefaultBlackboardControllerGrain : BlackboardBaseControllerGrain<DefaultBlackboardControllerState, IDefaultBlackboardControllerGrain>, IDefaultBlackboardControllerGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DefaultBlackboardControllerGrain"/> class.
        /// </summary>
        static DefaultBlackboardControllerGrain()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBlackboardControllerGrain"/> class.
        /// </summary>
        public DefaultBlackboardControllerGrain(ILogger<IDefaultBlackboardControllerGrain> logger,
                                                IBlackboardProvider blackboardProvider,
                                                [PersistentState(DemocriteConstants.DefaultDemocriteStateConfigurationKey, DemocriteConstants.DefaultDemocriteStateConfigurationKey)] IPersistentState<DefaultBlackboardControllerState> persistentState)
            : base(logger, blackboardProvider, persistentState)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task<IReadOnlyCollection<BlackboardCommand>?> OnInitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            this.State!.Option = (option as DefaultControllerOptions) ?? DefaultControllerOptions.Default;
            await PushStateAsync(cancellationToken.CancellationToken);

            return null;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ResolvePushIssueAsync<TData>(BlackboardProcessingStorageIssue issue, DataRecordContainer<TData?> sourceInjected, GrainCancellationToken token)
        {
            IReadOnlyCollection<BlackboardCommand>? resolutionAction = null;

            if (issue is null || sourceInjected is null)
                return null;

            if (issue.IssueStorageType == BlackboardProcessingIssueStorageTypeEnum.Limits)
                resolutionAction = await ResolvePushLimitIssueAsync(issue as MaxRecordBlackboardProcessingRuleIssue, sourceInjected, token.CancellationToken);

            return resolutionAction;

        }

        #region Tools
        
        /// <summary>
        /// Resolves the push limit issue.
        /// </summary>
        private ValueTask<IReadOnlyCollection<BlackboardCommand>?> ResolvePushLimitIssueAsync<TData>(MaxRecordBlackboardProcessingRuleIssue? issue, DataRecordContainer<TData?> sourceInjected, CancellationToken cancellationToken)
        {
            // Don't know how to resolve an other issue than type MaxRecordBlackboardProcessingRuleIssue
            if (issue is null)
            {
                this.Logger.OptiLog(LogLevel.Warning, "[Limit Resolution] does't manage limit issue from another type than MaxRecordBlackboardProcessingRuleIssue : {from}", sourceInjected);
                return ValueTask.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
            }

            var resolutionMode = issue!.PreferenceResolution ?? this.State!.Option?.LimitResolutionPreference ?? BlackboardProcessingResolutionLimitTypeEnum.Reject;

            // No conflict resolution possible
            if (resolutionMode == BlackboardProcessingResolutionLimitTypeEnum.Reject)
                return ValueTask.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);

            cancellationToken.ThrowIfCancellationRequested();

            // In the worse scenario we have to create a command to remove each entry and add new one
            var resolutionActions = new List<BlackboardCommand>(issue!.MaxRecordAllow + 1);

            // if a preparation slot is waiting then will this on instead of inserting a new one
            if (issue.ConflictRecords.Any(c => c.Status == RecordStatusEnum.Preparation && c.LogicalType == sourceInjected.LogicalType))
            {
                var availableSlot = issue.ConflictRecords.First(c => c.Status == RecordStatusEnum.Preparation);
                var cmds = new BlackboardCommand[]
                {
                    new BlackboardCommandStorageAddRecord<TData>(new DataRecordContainer<TData?>(sourceInjected.LogicalType,
                                                                                                 availableSlot.Uid,
                                                                                                 sourceInjected.DisplayName,
                                                                                                 sourceInjected.Data,
                                                                                                 sourceInjected.Status,
                                                                                                 availableSlot.UTCCreationTime,
                                                                                                 availableSlot.CreatorIdentity,
                                                                                                 sourceInjected.UTCLastUpdateTime,
                                                                                                 sourceInjected.LastUpdaterIdentity,
                                                                                                 sourceInjected.CustomMetadata ?? availableSlot.CustomMetadata), true, true)
                };
                return ValueTask.FromResult<IReadOnlyCollection<BlackboardCommand>?>(cmds);
            }

            IEnumerable<Guid>? toRemove = null;
            var allRecords = issue.ConflictRecords.Select(c => (Uid: c.Uid, UTCLastUpdateTime: c.UTCLastUpdateTime, UTCCreationTime: c.UTCCreationTime))
                                                  .Append((Uid: sourceInjected.Uid, UTCLastUpdateTime: sourceInjected.UTCLastUpdateTime, UTCCreationTime: sourceInjected.UTCCreationTime))
                                                  .ToArray();

            bool addNew = true;

            switch (resolutionMode)
            {
                case BlackboardProcessingResolutionLimitTypeEnum.ClearAll:
                    toRemove = issue.ConflictRecords.Select(c => c.Uid);
                    break;

                case BlackboardProcessingResolutionLimitTypeEnum.KeepNewest:
                    toRemove = allRecords.OrderByDescending(r => r.UTCCreationTime).Skip(issue.MaxRecordAllow).Select(c => c.Uid);
                    break;

                case BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated:
                    toRemove = allRecords.OrderByDescending(r => r.UTCLastUpdateTime).Skip(issue.MaxRecordAllow).Select(c => c.Uid);
                    break;

                case BlackboardProcessingResolutionLimitTypeEnum.KeepOldest:
                    toRemove = allRecords.OrderBy(r => r.UTCCreationTime).Skip(issue.MaxRecordAllow).Select(c => c.Uid);
                    break;

                case BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated:
                    toRemove = allRecords.OrderBy(r => r.UTCLastUpdateTime).Skip(issue.MaxRecordAllow).Select(c => c.Uid);
                    break;

                default:
                    this.Logger.OptiLog(LogLevel.Warning, "[Limit Resolution] resolution mode ({resolutionMode}) not supported : {issue} \n from {from}", resolutionMode, issue, sourceInjected);
                    return ValueTask.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
            }

            if (toRemove is not null)
            {
                var finalized = toRemove.ToArray();

                // Check if the element to add follow the condition to be added
                addNew = finalized.Any(rmUid => rmUid == sourceInjected.Uid) == false;

                if (addNew)
                {
                    var resolutionRemoveMode = issue!.PreferenceRemoveResolution ?? this.State!.Option?.RemoveResolutionPreference ?? BlackboardProcessingResolutionRemoveTypeEnum.Remove;
                    resolutionActions.AddRange(finalized.Select<Guid, BlackboardCommand>(uid => (resolutionRemoveMode == BlackboardProcessingResolutionRemoveTypeEnum.Decommission ? new BlackboardCommandStorageDecommissionRecord(uid) : new BlackboardCommandStorageRemoveRecord(uid)) ));
                }
            }

            if (addNew)
                resolutionActions.Add(new BlackboardCommandStorageAddRecord<TData>(sourceInjected, true, true));

            if (resolutionActions.Any() == false)
                resolutionActions.Add(new BlackboardCommandRejectAction(issue));

            return ValueTask.FromResult<IReadOnlyCollection<BlackboardCommand>?>(resolutionActions);
        }
        
        #endregion

        #endregion
    }
}
