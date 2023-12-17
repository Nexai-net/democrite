// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Exceptions
{
    using Democrite.Framework.Cluster.Abstractions.Resources;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Raised when a configuration provide is not in the expected format
    /// </summary>
    public sealed class InvalidFormatDemocriteConfigurationException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFormatDemocriteConfigurationException"/> class.
        /// </summary>
        public InvalidFormatDemocriteConfigurationException(string configurationProperty,
                                                            string configurationValue,
                                                            string expectedFormatDefinition,
                                                            Exception? innerException = null)
            : base(ErrorSR.InvalidFormatDemocriteCondigurationMessage.WithArguments(configurationProperty, configurationValue, expectedFormatDefinition),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Configuration, DemocriteErrorCodes.PartType.Property, DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            this.Data.Add(nameof(configurationProperty), configurationProperty);
            this.Data.Add(nameof(configurationValue), configurationValue);
            this.Data.Add(nameof(expectedFormatDefinition), expectedFormatDefinition);
        }

        #endregion
    }
}
