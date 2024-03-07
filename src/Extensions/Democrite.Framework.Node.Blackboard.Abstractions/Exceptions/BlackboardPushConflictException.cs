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
    public sealed class BlackboardPushConflictException : DemocriteBaseException<BlackboardPushConflictException>
    {
        public BlackboardPushConflictException(DataRecordConflict conflict,
                                               DataRecordContainer newConflictValue,
                                               string reason,
                                               Exception? innerException = null)
            : this(BlackboardErrorSR.BlackboardPushConflictException.WithArguments(reason, conflict.ToDebugDisplayName(), newConflictValue.ToDebugDisplayName()),
                   JsonSerializer.Serialize(conflict),
                   JsonSerializer.Serialize(newConflictValue),
                   reason,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Data,
                                             DemocriteErrorCodes.PartType.Insert,
                                             DemocriteErrorCodes.ErrorType.Conflict),
                   innerException)
        {
        }

        internal BlackboardPushConflictException(string message,
                                                 string existingJson,
                                                 string newConflictValueJson,
                                                 string reason,
                                                 ulong errorCode,
                                                 Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardPushConflictExceptionSurrogate.ExistingJson), existingJson);
            this.Data.Add(nameof(BlackboardPushConflictExceptionSurrogate.NewConflictValueJson), newConflictValueJson);
            this.Data.Add(nameof(BlackboardPushConflictExceptionSurrogate.Reason), reason);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardPushConflictExceptionSurrogate(string Message,
                                                                  string ExistingJson,
                                                                  string NewConflictValueJson,
                                                                  string Reason,
                                                                  ulong ErrorCode,
                                                                  Exception? InnerException) : IDemocriteBaseExceptionSurrogate;
    [RegisterConverter]
    public sealed class BlackboardPushConflictExceptionConverter : IConverter<BlackboardPushConflictException, BlackboardPushConflictExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardPushConflictException ConvertFromSurrogate(in BlackboardPushConflictExceptionSurrogate surrogate)
        {
            return new BlackboardPushConflictException(surrogate.Message,
                                                       surrogate.ExistingJson,
                                                       surrogate.NewConflictValueJson,
                                                       surrogate.Reason,
                                                       surrogate.ErrorCode,
                                                       surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardPushConflictExceptionSurrogate ConvertToSurrogate(in BlackboardPushConflictException value)
        {
            return new BlackboardPushConflictExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                ExistingJson = (string)value.Data[nameof(BlackboardPushConflictExceptionSurrogate.ExistingJson)]!,
                NewConflictValueJson = (string)value.Data[nameof(BlackboardPushConflictExceptionSurrogate.NewConflictValueJson)]!,
                Reason = (string)value.Data[nameof(BlackboardPushConflictExceptionSurrogate.Reason)]!
            };
        }
    }
}
