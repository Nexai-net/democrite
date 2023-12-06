// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when the input provide is not of the expected type
    /// </summary>
    /// <seealso cref="Democrite.Framework.Core.Abstractions.Exceptions.DemocriteBaseException" />
    public sealed class InvalidInputDemocriteException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputDemocriteException"/> class.
        /// </summary>
        /// <param name="inputType">Type of the input provide.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="executionInformation">The execution source information to understand when this occured, sequence input, stage, ...</param>
        public InvalidInputDemocriteException(Type inputType,
                                              Type expectedType,
                                              string? executionInformation = null,
                                              Exception? inner = null)

            : base(DemocriteExceptionSR.InvalidInputDemocriteExceptionMessage
                                       .WithArguments(inputType,
                                                      expectedType,
                                                      executionInformation ?? string.Empty),

                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence,
                                             DemocriteErrorCodes.PartType.Input,
                                             DemocriteErrorCodes.ErrorType.Invalid,
                                             0),
                   inner)
        {
            this.Data.Add(nameof(inputType), inputType);
            this.Data.Add(nameof(expectedType), expectedType);
            this.Data.Add(nameof(executionInformation), executionInformation ?? string.Empty);
        }
    }
}
