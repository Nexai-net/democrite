// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    /// <summary>
    /// Singleton grain used as registry to refer all the blackboard created
    /// </summary>
    /// <seealso cref="IVGrain" />
    [DemocriteSystemVGrain]
    [VGrainIdSingleton(singletonValue: IBlackboardRegistryGrain.GrainId)]
    internal interface IBlackboardRegistryGrain : IVGrain, IGrainWithStringKey
    {
        /// <summary>
        /// The grain singleton identifier
        /// </summary>
        const string GrainId = "BlackboardRegistry";

        /// <summary>
        /// Try get <see cref="IBlackboardRef"/> based on his unique identifier <paramref name="uid"/>
        /// </summary>
        [ReadOnly]
        Task<Tuple<BlackboardId, GrainId>?> TryGetAsync(Guid uid);

        /// <summary>
        /// Try get <see cref="IBlackboardRef"/> based on his name and template name identifier <paramref name="uid"/>
        /// </summary>
        [ReadOnly]
        Task<Tuple<BlackboardId, GrainId>?> TryGetAsync(string name, string templateName);

        /// <summary>
        /// Get existing or create a <see cref="IBlackboardRef"/> based on his unique couple identifier <paramref name="boardName"/> & <paramref name="blackboardTemplateKey"/>
        /// </summary>
        Task<Tuple<BlackboardId, GrainId>> GetOrCreateAsync(string boardName, string blackboardTemplateKey, GrainCancellationToken token, Guid? callContextId = null);

        /// <summary>
        /// Unregisters the specified blackboard.
        /// </summary>
        Task Unregister(Guid uid);
    }
}
