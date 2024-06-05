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
    /// Raised when two entry conflict in the blackoard
    /// </summary>
    /// <seealso cref="DemocriteBaseException{BlackboardPushConflictException}" />
    public sealed class BlackboardPushValidationException : DemocriteBaseException<BlackboardPushValidationException>
    {
        public BlackboardPushValidationException(DataRecordContainer input,
                                                 string reason,
                                                 Exception? innerException)
            : this(BlackboardErrorSR.BlackboardPushValidationException.Replace("\\n", Environment.NewLine).WithArguments(reason, input.ToDebugDisplayName()),
                   JsonSerializer.Serialize(input),
                   reason,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Data,
                                             DemocriteErrorCodes.PartType.Insert,
                                             DemocriteErrorCodes.ErrorType.Conflict),
                   innerException)
        {
        }

        internal BlackboardPushValidationException(string message,
                                                   string inputJson,
                                                   string reason,
                                                   ulong errorCode,
                                                   Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardPushValidationExceptionSurrogate.InputJson), inputJson);
            this.Data.Add(nameof(BlackboardPushValidationExceptionSurrogate.Reason), reason);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardPushValidationExceptionSurrogate(string Message,
                                                                    string InputJson,
                                                                    string Reason,
                                                                    ulong ErrorCode,
                                                                    Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardPushValidationExceptionConverter : IConverter<BlackboardPushValidationException, BlackboardPushValidationExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardPushValidationException ConvertFromSurrogate(in BlackboardPushValidationExceptionSurrogate surrogate)
        {
            return new BlackboardPushValidationException(surrogate.Message,
                                                         surrogate.InputJson,
                                                         surrogate.Reason,
                                                         surrogate.ErrorCode,
                                                         surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardPushValidationExceptionSurrogate ConvertToSurrogate(in BlackboardPushValidationException value)
        {
            return new BlackboardPushValidationExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                InputJson = (string)value.Data[nameof(BlackboardPushValidationExceptionSurrogate.InputJson)]!,
                Reason = (string)value.Data[nameof(BlackboardPushValidationExceptionSurrogate.Reason)]!
            };
        }
    }
}
