// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain opening the blackboard manipulation through sequences
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = "{input.BoardUid}", FirstParameterFallback = "BlackboardDataGlobalProvider")]
    public interface IBlackboardStorageVGrain : IVGrain
    {
        /// <summary>
        /// Pulls the first that march the request
        /// </summary>
        Task<TData?> PullFirstDataAsync<TData>(IPullDataRecordRequest request, IExecutionContext executionContext);

        /// <summary>
        /// Pulls the first that march the request
        /// </summary>
        Task<DataRecordContainer<TData?>?> PullFirstDataContainerAsync<TData>(IPullDataRecordRequest request, IExecutionContext executionContext);

        /// <summary>
        /// Pulls all data march the request
        /// </summary>
        Task<DataRecordCollectionContainer<TData?>> PullDataContainersAsync<TData>(IPullDataRecordRequest dataTargetInfo, IExecutionContext executionContext);

        /// <summary>
        /// Pulls all data march the request
        /// </summary>
        Task<IReadOnlyCollection<TData?>> PullDataAsync<TData>(IPullDataRecordRequest dataTargetInfo, IExecutionContext executionContext);

        /// <summary>
        /// Pushes a data in the blackboard
        /// </summary>
        Task<bool> PushDataAsync<TData>(IPushDataRecordRequest<TData?> data, IExecutionContext ctx);
    }
}
