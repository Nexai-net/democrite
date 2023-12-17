// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Exceptions
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Cluster.Abstractions.Resources;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Exception raised when the a required configuration is missing
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class MissingRequiredAutoDemocriteConfigurationException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingRequiredAutoDemocriteConfigurationException"/> class.
        /// </summary>
        private MissingRequiredAutoDemocriteConfigurationException(Type autoConfiguratorType,
                                                                   string? autoPropKeyValue,
                                                                   Exception? innerException = null)
            : base(ErrorSR.RequiredAutoConfigurationMissing.WithArguments(autoConfiguratorType, autoPropKeyValue),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Build, DemocriteErrorCodes.PartType.Configuration, DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {
            this.Data.Add(nameof(autoConfiguratorType), autoConfiguratorType);
            this.Data.Add(nameof(autoPropKeyValue), autoPropKeyValue);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an exception <see cref="MissingRequiredDemocriteConfigurationException"/> with parameter rulled.
        /// </summary>
        public static MissingRequiredAutoDemocriteConfigurationException Create<TAutoConfigurator>(string? autoPropKeyValue)
            where TAutoConfigurator : IAutoConfigurator
        {
            return new MissingRequiredAutoDemocriteConfigurationException(typeof(TAutoConfigurator), autoPropKeyValue);
        }

        #endregion
    }
}
