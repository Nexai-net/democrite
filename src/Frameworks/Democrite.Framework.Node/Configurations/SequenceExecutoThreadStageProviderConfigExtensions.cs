// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.ThreadExecutors;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal static class SequenceExecutoThreadStageProviderConfigExtensions
    {
        public static void SetupSequenceExecutorThreadStageProvider(this IServiceCollection serviceCollection)
        {
            // Thread Stage Executor
            serviceCollection.TryAddSingleton<ISequenceExecutorThreadStageProvider, SequenceExecutorThreadStageProvider>();

            // Push to context
            serviceCollection.AddSingleton<SequenceExecutorThreadStagePushToContext>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStagePushToContextDefinition, SequenceExecutorThreadStagePushToContext>>();

            // Foreach
            serviceCollection.AddSingleton<SequenceExecutorThreadStageForeach>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageForeachDefinition, SequenceExecutorThreadStageForeach>>();

            // Call
            serviceCollection.AddSingleton<SequenceExecutorThreadStageCall>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageCallDefinition, SequenceExecutorThreadStageCall>>();

            // Filter
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorFilterThreadStageProvider>();

            // FireAsync Signal
            serviceCollection.AddSingleton<SequenceExecutorThreadStageFireSignal>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageFireSignalDefinition, SequenceExecutorThreadStageFireSignal>>();

            // Select
            serviceCollection.AddSingleton<SequenceExecutorThreadStageSelect>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageSelectDefinition, SequenceExecutorThreadStageSelect>>();

            // Nested Sequence call
            serviceCollection.AddSingleton<SequenceExecutorThreadStageNestedSequenceCall>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageNestedSequenceCallDefinition, SequenceExecutorThreadStageNestedSequenceCall>>();

        }
    }
}
