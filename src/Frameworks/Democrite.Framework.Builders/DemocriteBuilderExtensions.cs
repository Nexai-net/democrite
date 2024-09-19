// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// KEEP : Democrite.Framework.Configurations
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Builders;

    using Microsoft.Extensions.DependencyInjection;

    public static class DemocriteBuilderExtensions
    {
        /// <summary>
        /// Adds the definition compiler.
        /// </summary>
        public static TWizard AddDefinitionCompiler<TWizard>(this TWizard builder)
            where TWizard : IBuilderDemocriteBaseWizard
        {
            builder.GetServiceCollection().AddSingleton<IDefinitionCompiler, DefinitionCompiler>();
            return builder;
        }
    }
}
