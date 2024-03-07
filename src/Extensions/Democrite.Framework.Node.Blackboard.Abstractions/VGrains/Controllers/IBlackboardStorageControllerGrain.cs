// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;

    using System.Threading.Tasks;

    /// <summary>
    /// Controller in charge to managed storage issue
    /// </summary>
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public interface IBlackboardStorageControllerGrain : IBlackboardBaseControllerGrain
    {
        /// <summary>
        /// Called to solve the issue occured during push operation
        /// </summary>
        Task<IReadOnlyCollection<BlackboardCommand>?> ResolvePushIssueAsync<TData>(BlackboardProcessingStorageIssue issue,
                                                                                   DataRecordContainer<TData?> sourceInjected,
                                                                                   GrainCancellationToken token);
    }
}
