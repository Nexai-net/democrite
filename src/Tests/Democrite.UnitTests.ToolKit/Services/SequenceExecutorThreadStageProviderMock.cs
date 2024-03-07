// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Services
{
    using Democrite.Framework.Node.Configurations;
    using Democrite.Framework.Node.ThreadExecutors;

    using Microsoft.Extensions.DependencyInjection;

    public static class SequenceExecutorThreadStageProviderMock
    {
        public static object Create(IServiceCollection? services = null)
        {
            services ??= new ServiceCollection();
            services.SetupSequenceExecutorThreadStageProvider();

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<ISequenceExecutorThreadStageProvider>();
        }
    }
}