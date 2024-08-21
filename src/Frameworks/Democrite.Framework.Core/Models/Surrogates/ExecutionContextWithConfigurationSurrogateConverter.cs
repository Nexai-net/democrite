// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models.Surrogates
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models.Surrogates;

    using Orleans;

    [RegisterConverter]
    public sealed class ExecutionContextWithConfigurationSurrogateConverter<TConfiguration> : IConverter<ExecutionContextWithConfiguration<TConfiguration>, ExecutionContextWithConfigurationSurrogate<TConfiguration>>
    {
        /// <inheritdoc/>
        public ExecutionContextWithConfiguration<TConfiguration> ConvertFromSurrogate(in ExecutionContextWithConfigurationSurrogate<TConfiguration> surrogate)
        {
            var ctx = new ExecutionContextWithConfiguration<TConfiguration>(surrogate.FlowUID,
                                                                            surrogate.CurrentExecutionId,
                                                                            surrogate.ParentExecutionId,
                                                                            surrogate.Configuration);
            ctx.InjectAllDataContext(surrogate.ContextDataContainers);
            return ctx;
        }

        /// <inheritdoc/>
        public ExecutionContextWithConfigurationSurrogate<TConfiguration> ConvertToSurrogate(in ExecutionContextWithConfiguration<TConfiguration> value)
        {
            return new ExecutionContextWithConfigurationSurrogate<TConfiguration>()
            {
                Configuration = value.Configuration,
                ContextDataContainers = value.GetAllDataContext(),
                CurrentExecutionId = value.CurrentExecutionId,
                ParentExecutionId = value.ParentExecutionId,
                FlowUID = value.FlowUID,
            };
        }
    }
}
