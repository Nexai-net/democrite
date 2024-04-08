// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Raised when definition required is missing
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class DemocriteException : DemocriteBaseException<DemocriteException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteException"/> class.
        /// </summary>
        public DemocriteException(string message,
                                  ulong errorCode = 0,
                                  Exception? innerException = null) 
            : base(message, errorCode, innerException)
        {
        }
    }

    [GenerateSerializer]
    public record struct DemocriteExceptionSurrogate(string Message,
                                                     ulong ErrorCode,
                                                     Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class DemocriteExceptionConverter : IConverter<DemocriteException, DemocriteExceptionSurrogate>
    {
        public DemocriteException ConvertFromSurrogate(in DemocriteExceptionSurrogate surrogate)
        {
            return new DemocriteException(surrogate.Message,
                                          surrogate.ErrorCode,
                                          surrogate.InnerException);
        }

        public DemocriteExceptionSurrogate ConvertToSurrogate(in DemocriteException value)
        {
            return new DemocriteExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
            };
        }
    }
}
