// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when the input provide is not of the expected type
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidInputDemocriteException : DemocriteBaseException<InvalidInputDemocriteException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputDemocriteException"/> class.
        /// </summary>
        /// <param name="inputType">ArtifactType of the input provide.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="executionInformation">The execution source information to understand when this occured, sequence input, stage, ...</param>
        public InvalidInputDemocriteException(Type inputType,
                                              Type expectedType,
                                              string? executionInformation = null,
                                              Exception? inner = null)

            : this(DemocriteExceptionSR.InvalidInputDemocriteExceptionMessage
                                       .WithArguments(inputType,
                                                      expectedType,
                                                      executionInformation ?? string.Empty),

                   inputType.GetAbstractType(),
                   expectedType.GetAbstractType(),
                   executionInformation,

                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence,
                                             DemocriteErrorCodes.PartType.Input,
                                             DemocriteErrorCodes.ErrorType.Invalid,
                                             0),
                   inner)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputDemocriteException"/> class.
        /// </summary>
        internal InvalidInputDemocriteException(string message,
                                                AbstractType inputType,
                                                AbstractType expectedType,
                                                string? executionInformation,
                                                ulong errorCode,
                                                Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(InvalidInputDemocriteExceptionSurrogate.InputType), inputType);
            this.Data.Add(nameof(InvalidInputDemocriteExceptionSurrogate.ExpectedType), expectedType);
            this.Data.Add(nameof(InvalidInputDemocriteExceptionSurrogate.ExecutionInformation), executionInformation ?? string.Empty);
        }
    }

    [GenerateSerializer]
    public struct InvalidInputDemocriteExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public AbstractType ExpectedType { get; set; }

        [Id(3)]
        public AbstractType InputType { get; set; }

        [Id(4)]
        public string ExecutionInformation { get; set; }

        [Id(5)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class InvalidInputDemocriteExceptionConverter : IConverter<InvalidInputDemocriteException, InvalidInputDemocriteExceptionSurrogate>
    {
        public InvalidInputDemocriteException ConvertFromSurrogate(in InvalidInputDemocriteExceptionSurrogate surrogate)
        {
            return new InvalidInputDemocriteException(surrogate.Message,
                                                      surrogate.InputType,
                                                      surrogate.ExpectedType,
                                                      surrogate.ExecutionInformation,
                                                      surrogate.ErrorCode,
                                                      surrogate.InnerException);
        }

        public InvalidInputDemocriteExceptionSurrogate ConvertToSurrogate(in InvalidInputDemocriteException value)
        {
            return new InvalidInputDemocriteExceptionSurrogate()
            {
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InputType = (AbstractType)value.Data[nameof(InvalidInputDemocriteExceptionSurrogate.InputType)]!,
                ExpectedType = (AbstractType)value.Data[nameof(InvalidInputDemocriteExceptionSurrogate.ExpectedType)]!,
                ExecutionInformation = (string)value.Data[nameof(InvalidOutputDemocriteExceptionSurrogate.ExecutionInformation)]!
            };
        }
    }
}
