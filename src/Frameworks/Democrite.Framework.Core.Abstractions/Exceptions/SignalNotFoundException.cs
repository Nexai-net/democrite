// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when signal name provide is missing from the <see cref="ISignalDefinitionProvider"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class SignalNotFoundException : DemocriteBaseException<SignalNotFoundException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNotFoundException"/> class.
        /// </summary>
        public SignalNotFoundException(string signalName, Exception? innerException = null)
            : this(DemocriteExceptionSR.SignalNotFounded.WithArguments(signalName),
                   signalName,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Signal, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.NotFounded),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNotFoundException"/> class.
        /// </summary>
        internal SignalNotFoundException(string message,
                                         string signalName,
                                         ulong errorCode,
                                         Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(SignalNotFoundExceptionSurrogate.SignalName), signalName);
        }
    }

    [GenerateSerializer]
    public struct SignalNotFoundExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public string SignalName { get; set; }

        [Id(3)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class SignalNotFoundExceptionConverter : IConverter<SignalNotFoundException, SignalNotFoundExceptionSurrogate>
    {
        public SignalNotFoundException ConvertFromSurrogate(in SignalNotFoundExceptionSurrogate surrogate)
        {
           return new SignalNotFoundException(surrogate.Message, surrogate.SignalName, surrogate.ErrorCode, surrogate.InnerException);
        }

        public SignalNotFoundExceptionSurrogate ConvertToSurrogate(in SignalNotFoundException value)
        {
            return new SignalNotFoundExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                SignalName = (string)value.Data[nameof(SignalNotFoundExceptionSurrogate.SignalName)]!
            };
        }
    }
}
