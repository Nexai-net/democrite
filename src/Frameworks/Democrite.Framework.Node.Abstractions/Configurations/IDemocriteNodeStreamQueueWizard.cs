// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations
{
    using Democrite.Framework.Core.Abstractions.Streams;

    /// <summary>
    /// Build in charge to setup StreamQueue resources and how to access it
    /// </summary>
    public interface IDemocriteNodeStreamQueueWizard
    {
        /// <summary>
        /// Adds an streamQueue in definition execution.
        /// </summary>
        IDemocriteNodeStreamQueueWizard Register(params StreamQueueDefinition[] streamQueueDefinitions);
    }
}
