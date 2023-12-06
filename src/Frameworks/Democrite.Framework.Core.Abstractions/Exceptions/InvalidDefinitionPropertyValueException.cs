// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when definition's property is invalid.
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidDefinitionPropertyValueException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDefinitionPropertyValueException"/> class.
        /// </summary>
        public InvalidDefinitionPropertyValueException(Type definitionType,
                                                       string propertyName,
                                                       string value,
                                                       string expectedDetails,
                                                       Exception? innerException = null)
            : base(DemocriteExceptionSR.InvalidDefinitionPropertyValueExceptionMessage.WithArguments(propertyName, definitionType, value, expectedDetails),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Definition, DemocriteErrorCodes.PartType.Property, DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            this.Data.Add(nameof(definitionType), definitionType);
            this.Data.Add(nameof(propertyName), propertyName);
            this.Data.Add(nameof(value), value);
            this.Data.Add(nameof(expectedDetails), expectedDetails);
        }
    }
}
