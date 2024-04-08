// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when definition required is missing
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class DemocriteInternalException : DemocriteBaseException<DemocriteInternalException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteInternalException"/> class.
        /// </summary>
        public DemocriteInternalException(DemocriteBaseException originException,
                                          Exception? innerException = null)
            : this(originException?.Message ?? "Relay internal exception",
                   originException?.GetType().GetAbstractType() ?? typeof(Exception).GetAbstractType(),
                   originException?.ErrorCode ?? DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Execution, genericType: DemocriteErrorCodes.ErrorType.Failed),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteInternalException"/> class.
        /// </summary>
        internal DemocriteInternalException(string message,
                                            AbstractType exceptionType,
                                            ulong errorCode,
                                            Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(DemocriteInternalExceptionSurrogate.ExceptionType), exceptionType);
        }
    }

    [GenerateSerializer]
    public record struct DemocriteInternalExceptionSurrogate(string Message,
                                                             ulong ErrorCode,
                                                             AbstractType ExceptionType,
                                                             Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class DemocriteInternalExceptionConverter : IConverter<DemocriteInternalException, DemocriteInternalExceptionSurrogate>
    {
        public DemocriteInternalException ConvertFromSurrogate(in DemocriteInternalExceptionSurrogate surrogate)
        {
            return new DemocriteInternalException(surrogate.Message,
                                                  surrogate.ExceptionType,
                                                  surrogate.ErrorCode,
                                                  surrogate.InnerException);
        }

        public DemocriteInternalExceptionSurrogate ConvertToSurrogate(in DemocriteInternalException value)
        {
            return new DemocriteInternalExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                ExceptionType = (AbstractType)value.Data[nameof(DemocriteInternalExceptionSurrogate.ExceptionType)]!,
            };
        }
    }
}
