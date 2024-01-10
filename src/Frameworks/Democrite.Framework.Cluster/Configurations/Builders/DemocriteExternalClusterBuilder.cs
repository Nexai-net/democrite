// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Configurations.Builders
{
    using Democrite.Framework.Configurations;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Implementation of external information builder
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <seealso cref="DemocriteExternalBaseServiceBuilder{TOption, IDemocriteClusterBuilder{TOption}}" />
    public sealed class DemocriteExternalClusterBuilder<TOption> : DemocriteExternalBaseServiceBuilder<TOption, IDemocriteClusterExternalBuilder<TOption>>, IDemocriteClusterExternalBuilder<TOption>
        where TOption : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteExternalClusterBuilder{TOption}"/> class.
        /// </summary>
        public DemocriteExternalClusterBuilder(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
