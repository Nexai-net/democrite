// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Requests;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using System.Threading.Tasks;

    /// <summary>
    /// Service used to customize the information like query and execution context into 
    /// </summary>
    /// <seealso cref="IVGrain" />

    [VGrainStatelessWorker]
    public interface IBlackboardStoragePushRequestBuilderVGrain : IBlackboardStorageRequestBuilderVGrain
    {
        /// <summary>
        /// Gets the <see cref="IPullDataRecordRequest"/> from input and simple <see cref="IExecutionContext"/>
        /// </summary>
        Task<IPushDataRecordRequest<TInput>> GetPushTargetFromInputAsync<TInput>(TInput data, IExecutionContext context);

        /// <summary>
        /// Gets the <see cref="IPullDataRecordRequest"/> from input and simple <see cref="IExecutionContext"/>
        /// </summary>
        Task<IPushDataRecordRequest<TInput>> GetPushTargetFromInputAsync<TInput, TContextInfo>(TInput data, IExecutionContext<TContextInfo> context)
            where TContextInfo : IDataRecordPushRequestOption;
    }
}
