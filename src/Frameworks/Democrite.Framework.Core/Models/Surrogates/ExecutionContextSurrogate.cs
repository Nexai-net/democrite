// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models.Surrogates
{
    using Orleans;

    using System;

    /// <summary>
    /// Surrogate class using to serialize and restore <see cref="ExecutionContext"/>
    /// </summary>
    [GenerateSerializer]
    public struct ExecutionContextSurrogate
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
    }

    [RegisterConverter]
    public sealed class ExecutionContextSurrogateConverter : IConverter<ExecutionContext, ExecutionContextSurrogate>
    {
        /// <inheritdoc/>
        public ExecutionContext ConvertFromSurrogate(in ExecutionContextSurrogate surrogate)
        {
            return new ExecutionContext(surrogate.FlowUID,
                                        surrogate.CurrentExecutionId,
                                        surrogate.ParentExecutionId);
        }

        /// <inheritdoc/>
        public ExecutionContextSurrogate ConvertToSurrogate(in ExecutionContext value)
        {
            return new ExecutionContextSurrogate()
            {
                CurrentExecutionId = value.CurrentExecutionId,
                ParentExecutionId = value.ParentExecutionId,
                FlowUID = value.FlowUID
            };
        }
    }
}