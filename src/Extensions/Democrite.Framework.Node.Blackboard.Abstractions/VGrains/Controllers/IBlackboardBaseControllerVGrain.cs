// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;

    using System.Threading.Tasks;

    /// <summary>
    /// A controller is grain responsible of a blackboard
    /// 
    /// This co
    /// </summary>
    /// <remarks>
    ///    - No Execution context this grain will directly be called by the blackboard in case of events
    ///    - His <see cref="GrainId"/> match the blackboard unique id (Guid)
    /// </remarks>
    public interface IBlackboardBaseControllerGrain : IVGrain, IGrainWithGuidKey
    {
        /// <summary>
        /// Initializations the controller using the dedicated <paramref name="blackboardTemplate"/>
        /// </summary>
        /// <remarks>
        ///     If definition template is needed pass a surrogate of it don't pass the definition itself.
        ///     Don't pass id because a blackboard doesn't allow the template build on to changed (Copy in his state)
        /// </remarks>
        Task<IReadOnlyCollection<BlackboardCommand>?> InitializationAsync(ControllerBaseOptions? option,
                                                                          GrainCancellationToken cancellationToken);
    }

    /// <summary>
    /// Define a black board controller that need a specific <typeparamref name="TOption"/> to be configured
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <seealso cref="IGrainWithGuidKey" />
    public interface IBlackboardBaseControllerGrain<TControllerOption> : IBlackboardBaseControllerGrain
        where TControllerOption : IControllerOptions
    {

    }
}
