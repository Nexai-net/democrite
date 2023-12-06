// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models.Surrogates
{
    using Orleans;

    using System;

    /// <summary>
    /// Surrogate class using to serialize and restore <see cref="ExecutionContextWithConfiguration{TConfiguration}"/>
    /// </summary>
    [GenerateSerializer]
    public struct ExecutionContextWithConfigurationSurrogate<TConfiguration>
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
    }

    [RegisterConverter]
    public sealed class ExecutionContextWithConfigurationSurrogateConverter<TConfiguration> : IConverter<ExecutionContextWithConfiguration<TConfiguration>, ExecutionContextWithConfigurationSurrogate<TConfiguration>>
    {
        /// <inheritdoc/>
        public ExecutionContextWithConfiguration<TConfiguration> ConvertFromSurrogate(in ExecutionContextWithConfigurationSurrogate<TConfiguration> surrogate)
        {
            return new ExecutionContextWithConfiguration<TConfiguration>(surrogate.FlowUID,
                                                                         surrogate.CurrentExecutionId,
                                                                         surrogate.ParentExecutionId,
                                                                         surrogate.Configuration);
        }

        /// <inheritdoc/>
        public ExecutionContextWithConfigurationSurrogate<TConfiguration> ConvertToSurrogate(in ExecutionContextWithConfiguration<TConfiguration> value)
        {
            return new ExecutionContextWithConfigurationSurrogate<TConfiguration>()
            {
                CurrentExecutionId = value.CurrentExecutionId,
                ParentExecutionId = value.ParentExecutionId,
                FlowUID = value.FlowUID,
                Configuration = value.Configuration
            };
        }
    }
}
