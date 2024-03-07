// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;

    using Orleans;

    using System.Threading.Tasks;

    /// <summary>
    /// Controller in charge to managed blackboard events
    /// </summary>
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public interface IBlackboardEventControllerGrain : IBlackboardBaseControllerGrain
    {
        /// <summary>
        /// Reacts to events
        /// </summary>
        Task<IReadOnlyCollection<BlackboardCommand>?> ReactToEventsAsync(IReadOnlyCollection<BlackboardEvent> events, GrainCancellationToken token);
    }
}
