// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Microsoft.Extensions.Configuration;

    using System;

    /// <summary>
    /// Base service used to build external connection
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    public interface IDemocriteExternalServiceBuilder<TOption, TBuilder>
        where TOption : class, new()
        where TBuilder : IDemocriteExternalServiceBuilder<TOption, TBuilder>
    {
        /// <summary>
        /// Adds cluster rendez point connection string.
        /// </summary>
        TBuilder ConnectionString(string connectionString);

        /// <summary>
        /// Adds cluster rendez point connection string.
        /// </summary>
        TBuilder ConnectionString(Func<IConfiguration, string> connectionStringFunc);

        /// <summary>
        /// Add and fill cluster rendez point option
        /// </summary>
        TBuilder ManualConfig(Action<TOption, IConfiguration> optionBuilder);
    }
}
