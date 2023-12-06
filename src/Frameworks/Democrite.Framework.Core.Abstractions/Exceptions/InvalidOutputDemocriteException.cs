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
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidOutputDemocriteException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOutputDemocriteException"/> class.
        /// </summary>
        /// <param name="desiredType">Type of the output desired.</param>
        /// <param name="outputType">The execuption result provided.</param>
        /// <param name="executionInformation">The execution source information to understand when this occured, sequence input, stage, ...</param>
        public InvalidOutputDemocriteException(Type desiredType,
                                               Type outputType,
                                               string? executionInformation = null,
                                               Exception? inner = null)
            : base(DemocriteExceptionSR.InvalidOutputDemocriteExceptionMessage
                                       .WithArguments(desiredType,
                                                      outputType,
                                                      executionInformation ?? string.Empty),

                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence,
                                             DemocriteErrorCodes.PartType.Output,
                                             DemocriteErrorCodes.ErrorType.Invalid,
                                             0),
                   inner)
        {
            this.Data.Add(nameof(desiredType), desiredType);
            this.Data.Add(nameof(outputType), outputType);
            this.Data.Add(nameof(executionInformation), executionInformation ?? string.Empty);
        }
    }
}
