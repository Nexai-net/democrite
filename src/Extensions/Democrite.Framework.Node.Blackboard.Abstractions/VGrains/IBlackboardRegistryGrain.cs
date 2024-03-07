// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Runtime;

    /// <summary>
    /// Singleton grain used as registry to refer all the blackboard created
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainIdSingleton(singletonValue: IBlackboardRegistryGrain.GrainId)]
    [DemocriteSystemVGrain]
    internal interface IBlackboardRegistryGrain : IVGrain, IGrainWithStringKey
    {
        /// <summary>
        /// The grain singleton identifier
        /// </summary>
        const string GrainId = "BlackboardRegistry";

        /// <summary>
        /// Try get <see cref="IBlackboardRef"/> based on his unique identifier <paramref name="uid"/>
        /// </summary>
        Task<GrainId?> TryGetAsync(Guid uid);

        /// <summary>
        /// Get existing or create a <see cref="IBlackboardRef"/> based on his unique couple identifier <paramref name="boardName"/> & <paramref name="blackboardTemplateKey"/>
        /// </summary>
        Task<GrainId> GetOrCreateAsync(string boardName, string blackboardTemplateKey);

        /// <summary>
        /// Unregisters the specified blackboard.
        /// </summary>
        Task Unregister(Guid uid);
    }
}
