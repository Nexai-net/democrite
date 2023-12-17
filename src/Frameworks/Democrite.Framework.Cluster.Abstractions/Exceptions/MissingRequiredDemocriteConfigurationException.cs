// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Exceptions
{
    using Democrite.Framework.Cluster.Abstractions.Resources;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Exception raised when the a required configuration is missing
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class MissingRequiredDemocriteConfigurationException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingRequiredDemocriteConfigurationException"/> class.
        /// </summary>
        public MissingRequiredDemocriteConfigurationException(string configurationKey,
                                                              string configurationDetail,
                                                              Exception? innerException = null)
            : base(ErrorSR.RequiredAutoConfigurationMissing.WithArguments(configurationKey, configurationDetail),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Build, DemocriteErrorCodes.PartType.Configuration, DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {
            this.Data.Add(nameof(configurationKey), configurationKey);
            this.Data.Add(nameof(configurationDetail), configurationDetail);
        }

        #endregion
    }
}
