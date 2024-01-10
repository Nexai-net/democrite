// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models.Surrogates
{
    using Democrite.Framework.Core.Abstractions.Models.Surrogates;

    using Orleans;

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
            return ExecutionContextSurrogate.From(value);
        }
    }
}