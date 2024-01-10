// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Configurations.Builders
{
    using Democrite.Framework.Cluster.Abstractions.Models;
    using Democrite.Framework.Configurations;

    using Microsoft.Extensions.Configuration;

    using System;

    /// <summary>
    /// Base implementation of <see cref="IDemocriteExternalServiceBuilder{TOption, TBuilder}"/> to help access to external service
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <seealso cref="IDemocriteExternalServiceBuilder{TOption, TBuilder}" />
    public abstract class DemocriteExternalBaseServiceBuilder<TOption, TBuilder> : IDemocriteExternalServiceBuilder<TOption, TBuilder>
        where TOption : class, new()
        where TBuilder : IDemocriteExternalServiceBuilder<TOption, TBuilder>
    {
        #region Fields

        private readonly IConfiguration _configuration;

        private string? _connectionString;
        private TOption? _option;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteExternalBaseServiceBuilder{TOption, TBuilder}"/> class.
        /// </summary>
        protected DemocriteExternalBaseServiceBuilder(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TBuilder ConnectionString(string connectionString)
        {
            this._connectionString = connectionString;
            return GetSourceBuilder();
        }

        /// <inheritdoc />
        public TBuilder ConnectionString(Func<IConfiguration, string> connectionStringFunc)
        {
            ArgumentNullException.ThrowIfNull(connectionStringFunc);

            this._connectionString = connectionStringFunc(this._configuration);
            return GetSourceBuilder();
        }

        /// <inheritdoc />
        public TBuilder ManualConfig(Action<TOption, IConfiguration> optionBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionBuilder);

            var opt = new TOption();

            optionBuilder(opt, this._configuration);

            this._option = opt;

            return GetSourceBuilder();
        }

        /// <summary>
        /// Builds result.
        /// </summary>
        public ExternalServiceBuild<TOption> Build()
        {
            return new ExternalServiceBuild<TOption>(this._connectionString, this._option);
        }

        #region Tools

        /// <summary>
        /// Gets source builder instance.
        /// </summary>
        protected virtual TBuilder GetSourceBuilder()
        {
            return (TBuilder)(object)this;
        }

        #endregion

        #endregion
    }
}
