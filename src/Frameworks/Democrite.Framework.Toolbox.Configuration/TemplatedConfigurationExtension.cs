// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Microsoft.Extensions.Configuration
{
    using Democrite.Framework.Toolbox.Configuration;

    /// <summary>
    /// Extension in charge to provide 
    /// </summary>
    public static class TemplatedConfigurationExtension
    {
        /// <summary>
        /// Apply templated configuration
        /// </summary>
        /// 
        /// <returns>
        ///     Return a new <see cref="TemplatedConfigurationBuilder"/> to apply template proxy at the resultion.
        /// </returns>
        /// 
        /// <remarks>
        ///     Use this function result to chain call
        /// </remarks>
        public static IConfigurationBuilder ToTemplatedConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return new TemplatedConfigurationBuilder(configurationBuilder);
        }

        /// <summary>
        /// Activates the templated configuration by proxy the already configured source
        /// </summary>
        /// <remarks>
        ///     This method MUST be called last
        /// </remarks>
        public static IConfigurationBuilder ActivateTemplatedConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            var templatedSource = configurationBuilder.Sources
                                                      .Select(s => s.Build(configurationBuilder))
                                                      .Select(provider => new TemplatedConfigurationProxySourceProvider(provider))
                                                      .ToArray();

            configurationBuilder.Sources.Clear();

            foreach (var source in templatedSource)
            {
                configurationBuilder.Sources.Add(source);
            }

            return configurationBuilder;
        }
    }
}
