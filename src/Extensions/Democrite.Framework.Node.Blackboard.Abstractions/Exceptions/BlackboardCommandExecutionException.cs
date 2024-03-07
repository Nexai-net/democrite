// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when push data failed
    /// </summary>
    /// <seealso cref="DemocriteBaseException{BlackboardCommandExecutionException}" />
    public sealed class BlackboardCommandExecutionException : DemocriteBaseException<BlackboardCommandExecutionException>
    {
        public BlackboardCommandExecutionException(BlackboardCommand cmd,
                                                   string reason,
                                                   Exception? innerException)
            : this(BlackboardErrorSR.BlackboardCommandExecutionException.WithArguments(reason, cmd.ToDebugDisplayName()),
                   cmd.ToDebugDisplayName(),
                   reason,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Data,
                                             DemocriteErrorCodes.PartType.Insert,
                                             DemocriteErrorCodes.ErrorType.Conflict),
                   innerException)
        {
        }

        internal BlackboardCommandExecutionException(string message,
                                                     string commandDetails,
                                                     string reason,
                                                     ulong errorCode,
                                                     Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardCommandExecutionExceptionSurrogate.CommandDetails), commandDetails);
            this.Data.Add(nameof(BlackboardCommandExecutionExceptionSurrogate.Reason), reason);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardCommandExecutionExceptionSurrogate(string Message,
                                                                      string CommandDetails,
                                                                      string Reason,
                                                                      ulong ErrorCode,
                                                                      Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardCommandExecutionExceptionConverter : IConverter<BlackboardCommandExecutionException, BlackboardCommandExecutionExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardCommandExecutionException ConvertFromSurrogate(in BlackboardCommandExecutionExceptionSurrogate surrogate)
        {
            return new BlackboardCommandExecutionException(surrogate.Message,
                                                           surrogate.CommandDetails,
                                                           surrogate.Reason,
                                                           surrogate.ErrorCode,
                                                           surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardCommandExecutionExceptionSurrogate ConvertToSurrogate(in BlackboardCommandExecutionException value)
        {
            return new BlackboardCommandExecutionExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                CommandDetails = (string)value.Data[nameof(BlackboardCommandExecutionExceptionSurrogate.CommandDetails)]!,
                Reason = (string)value.Data[nameof(BlackboardCommandExecutionExceptionSurrogate.Reason)]!
            };
        }
    }
}
