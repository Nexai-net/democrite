// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Exceptions
{
    using Democrite.Framework.Builders.Resources;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Raised when the build parameters are invalid or missing 
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidParameterException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParameterException"/> class.
        /// </summary>
        public InvalidParameterException(string paramName,
                                         string message,
                                         Exception? innerException = null)
            : base(BuildErrorSR.InvalidParameterExceptionMessage.WithArguments(paramName, message),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Build,
                                             DemocriteErrorCodes.PartType.Property,
                                             DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            this.Data.Add(nameof(paramName), paramName);
        }
    }
}
