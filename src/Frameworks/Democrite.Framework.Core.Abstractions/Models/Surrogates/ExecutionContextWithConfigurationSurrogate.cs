// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Core.Models;

    using Orleans;

    using System;

    /// <summary>
    /// Surrogate class using to serialize and restore <see cref="ExecutionContextWithConfiguration{TConfiguration}"/>
    /// </summary>
    [GenerateSerializer]
    public struct ExecutionContextWithConfigurationSurrogate<TConfiguration> : IExecutionContextSurrogate
    {
        /// <inheritdoc cref="ExecutionContext.FlowUID" />
        [Id(0)]
        public Guid FlowUID { get; set; }

        /// <inheritdoc cref="ExecutionContext.CurrentExecutionId" />
        [Id(1)]
        public Guid CurrentExecutionId { get; set; }

        /// <inheritdoc cref="ExecutionContext.ParentExecutionId" />
        [Id(2)]
        public Guid? ParentExecutionId { get; set; }

        [Id(3)]
        public TConfiguration? Configuration { get; set; }

        [Id(4)]
        public IReadOnlyCollection<IContextDataContainer> ContextDataContainers { get; set; }
    }
}
