// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Resources;

    using System;
    using System.Text.Json;

    /// <summary>
    /// Raised when push data failed
    /// </summary>
    /// <seealso cref="DemocriteBaseException{BlackboardPushException}" />
    public sealed class BlackboardPushException : DemocriteBaseException<BlackboardPushException>
    {
        public BlackboardPushException(DataRecordContainer input,
                                                 string reason,
                                                 Exception? innerException)
            : this(BlackboardErrorSR.BlackboardPushException.WithArguments(reason, input.ToDebugDisplayName()),
                   JsonSerializer.Serialize(input),
                   reason,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Data,
                                             DemocriteErrorCodes.PartType.Insert,
                                             DemocriteErrorCodes.ErrorType.Conflict),
                   innerException)
        {
        }

        internal BlackboardPushException(string message,
                                                   string inputJson,
                                                   string reason,
                                                   ulong errorCode,
                                                   Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardPushExceptionSurrogate.InputJson), inputJson);
            this.Data.Add(nameof(BlackboardPushExceptionSurrogate.Reason), reason);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardPushExceptionSurrogate(string Message,
                                                          string InputJson,
                                                          string Reason,
                                                          ulong ErrorCode,
                                                          Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardPushExceptionConverter : IConverter<BlackboardPushException, BlackboardPushExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardPushException ConvertFromSurrogate(in BlackboardPushExceptionSurrogate surrogate)
        {
            return new BlackboardPushException(surrogate.Message,
                                                         surrogate.InputJson,
                                                         surrogate.Reason,
                                                         surrogate.ErrorCode,
                                                         surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardPushExceptionSurrogate ConvertToSurrogate(in BlackboardPushException value)
        {
            return new BlackboardPushExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                InputJson = (string)value.Data[nameof(BlackboardPushExceptionSurrogate.InputJson)]!,
                Reason = (string)value.Data[nameof(BlackboardPushExceptionSurrogate.Reason)]!
            };
        }
    }
}
