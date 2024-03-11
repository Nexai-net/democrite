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
    public sealed class InvalidOutputDemocriteException : DemocriteBaseException<InvalidOutputDemocriteException>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOutputDemocriteException"/> class.
        /// </summary>
        /// <param name="desiredType">ArtifactType of the output desired.</param>
        /// <param name="outputType">The execuption result provided.</param>
        /// <param name="executionInformation">The execution source information to understand when this occured, sequence input, stage, ...</param>
        public InvalidOutputDemocriteException(Type desiredType,
                                               Type outputType,
                                               string? executionInformation = null,
                                               Exception? inner = null)
            : this(DemocriteExceptionSR.InvalidOutputDemocriteExceptionMessage
                                       .WithArguments(desiredType,
                                                      outputType,
                                                      executionInformation ?? string.Empty),

                   desiredType.GetAbstractType(),
                   outputType.GetAbstractType(),
                   executionInformation,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence,
                                             DemocriteErrorCodes.PartType.Output,
                                             DemocriteErrorCodes.ErrorType.Invalid,
                                             0),
                   inner)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOutputDemocriteException"/> class.
        /// </summary>
        internal InvalidOutputDemocriteException(string message,
                                                 AbstractType desiredType,
                                                 AbstractType outputType,
                                                 string? executionInformation,
                                                 ulong errorCode,
                                                 Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(InvalidOutputDemocriteExceptionSurrogate.DesiredType), desiredType);
            this.Data.Add(nameof(InvalidOutputDemocriteExceptionSurrogate.OutputType), outputType);
            this.Data.Add(nameof(InvalidOutputDemocriteExceptionSurrogate.ExecutionInformation), executionInformation ?? string.Empty);
        }
    }

    [GenerateSerializer]
    public struct InvalidOutputDemocriteExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public AbstractType DesiredType { get; set; }

        [Id(3)]
        public AbstractType OutputType { get; set; }

        [Id(4)]
        public string ExecutionInformation { get; set; }

        [Id(5)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class InvalidOutputDemocriteExceptionConverter : IConverter<InvalidOutputDemocriteException, InvalidOutputDemocriteExceptionSurrogate>
    {
        public InvalidOutputDemocriteException ConvertFromSurrogate(in InvalidOutputDemocriteExceptionSurrogate surrogate)
        {
            return new InvalidOutputDemocriteException(surrogate.Message,
                                                       surrogate.DesiredType,
                                                       surrogate.OutputType,
                                                       surrogate.ExecutionInformation,
                                                       surrogate.ErrorCode,
                                                       surrogate.InnerException);
        }

        public InvalidOutputDemocriteExceptionSurrogate ConvertToSurrogate(in InvalidOutputDemocriteException value)
        {
            return new InvalidOutputDemocriteExceptionSurrogate()
            {
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                DesiredType = (AbstractType)value.Data[nameof(InvalidOutputDemocriteExceptionSurrogate.DesiredType)]!,
                OutputType = (AbstractType)value.Data[nameof(InvalidOutputDemocriteExceptionSurrogate.OutputType)]!,
                ExecutionInformation = (string)value.Data[nameof(InvalidOutputDemocriteExceptionSurrogate.ExecutionInformation)]!
            };
        }
    }
}
