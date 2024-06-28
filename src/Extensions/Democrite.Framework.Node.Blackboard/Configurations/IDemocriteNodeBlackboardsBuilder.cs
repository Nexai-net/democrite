// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Configurations
{
    using Democrite.Framework.Node.Blackboard.Abstractions;

    /// <summary>
    /// Builder use to configure all the blackboard information
    /// </summary>
    public interface IDemocriteNodeBlackboardsBuilder
    {
        /// <summary>
        /// Adds in memory definition related to blackboard.
        /// </summary>
        IDemocriteNodeBlackboardsBuilder AddInMemoryDefinitionProvider(Action<IDemocriteNodeBlackboardsLocalDefinitionBuilder> builder);

        /// <summary>
        /// Uses the default memory storage for blackboard registry state storage
        /// </summary>
        /// <remarks>
        ///     .AddMemoryGrainStorage(<see cref="BlackboardConstants.BlackboardRegistryStorageConfigurationKey"/>)
        /// </remarks>
        IDemocriteNodeBlackboardsBuilder UseInMemoryStorageForRegistryState();

        /// <summary>
        /// Uses the default memory storage for <see cref="IBlackboardRef"/> state storage
        /// </summary>
        /// <remarks>
        ///     .AddMemoryGrainStorage(<see cref="BlackboardConstants.BlackboardStateStorageConfigurationKey"/>)
        /// </remarks>
        IDemocriteNodeBlackboardsBuilder UseInMemoryStorageForBoardState();

        /// <summary>
        /// Uses the default memory storage for <see cref="IBlackboardRef"/> records storage
        /// </summary>
        /// <remarks>
        ///     .AddMemoryGrainStorage(<see cref="BlackboardConstants.BlackboardStorageRecordsConfigurationKey"/>)
        /// </remarks>
        IDemocriteNodeBlackboardsBuilder UseInMemoryStorageForRecords();
    }
}
