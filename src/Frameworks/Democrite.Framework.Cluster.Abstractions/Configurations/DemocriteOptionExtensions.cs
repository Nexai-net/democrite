// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Elvex.Toolbox.Extensions;

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
        ///     Return option
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
                var serviceDesc = services.FirstOrDefault(s => s.ServiceType == typeof(TOption));

                if (serviceDesc != null)
                {
                    if (serviceDesc.ImplementationInstance is TOption optionInst)
                        return optionInst;

                    if (serviceDesc.ImplementationFactory != null)
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        return (TOption)serviceDesc.ImplementationFactory(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                    throw new InvalidOperationException("Option already registred but could not extract the value");
                }

                serviceDesc = services.First(s => s.ServiceType == typeof(IOptions<TOption>));

                if (serviceDesc != null)
                {
                    if (serviceDesc.ImplementationInstance is IOptions<TOption> optionIncInst)
                        return optionIncInst.Value;

                    if (serviceDesc.ImplementationFactory != null)
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        return ((IOptions<TOption>)serviceDesc.ImplementationFactory(null))?.Value;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                    throw new InvalidOperationException("Option already registred but could not extract the value");
                }

                throw new InvalidOperationException("Option already registred but could not extract the value");
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
