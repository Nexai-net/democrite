// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations
{
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using System.Linq;

    /// <summary>
    /// Extensions method used to help setup option and managed config file, manual, default ...
    /// </summary>
    public static class DemocriteOptionExtensions
    {
        /// <summary>
        /// Adds option from implementation or instance
        /// </summary>
        /// <returns>
        ///     Return option inserted
        /// </returns>
        public static TOption? AddOptionFromInstOrConfig<TOption>(this IServiceCollection services,
                                                                  IConfiguration configuration,
                                                                  string? configurationSection = null,
                                                                  TOption? option = default,
                                                                  bool overrideValue = true)
            where TOption : class
        {
            if (overrideValue == false &&
                (services.Any(s => s.ServiceType == typeof(TOption)) ||
                 services.Any(s => s.ServiceType == typeof(IOptions<TOption>))))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(configurationSection))
            {
                var cfg = configuration.GetSection(configurationSection);

                if (cfg != null && cfg.Exists())
                {
                    services.Configure<TOption>(cfg);

                    var resultMapped = cfg.Get<TOption>();
                    return resultMapped;
                }
            }

            if (option != default)
            {
                services.AddSingleton(option)
                        .AddSingleton(option.ToOption())
                        .AddSingleton(option.ToMonitorOption());

                return option;
            }

            return null;
        }
    }
}
