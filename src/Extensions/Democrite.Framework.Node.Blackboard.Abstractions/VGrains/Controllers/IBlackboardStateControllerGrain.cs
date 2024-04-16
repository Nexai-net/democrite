// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;

    /// <summary>
    /// Controller in charge to managed blackboard state
    /// </summary>
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public interface IBlackboardStateControllerGrain : IBlackboardBaseControllerGrain
    {
        /// <summary>
        /// Called when blackboard is initialize.
        /// </summary>
        Task<IReadOnlyCollection<BlackboardCommand>?> OnInitialize(IReadOnlyCollection<DataRecordContainer> containers, GrainCancellationToken token);

        /// <summary>
        /// Called when blackboard state changed.
        /// </summary>
        Task<IReadOnlyCollection<BlackboardCommand>?> OnStateChanged(BlackboardLifeStatusEnum NewState, BlackboardLifeStatusEnum OldState, GrainCancellationToken token);

        /// <summary>
        /// Called when blackboard is sealed.
        /// </summary>
        Task<IReadOnlyCollection<BlackboardCommand>?> OnSealed(IReadOnlyCollection<BlackboardRecordMetadata> toKeepOnSealed, IReadOnlyCollection<BlackboardRecordMetadata> others, GrainCancellationToken token);
    }
}
