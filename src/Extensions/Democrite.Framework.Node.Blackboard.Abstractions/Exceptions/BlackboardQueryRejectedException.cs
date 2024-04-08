// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when push data failed
    /// </summary>
    /// <seealso cref="DemocriteBaseException{BlackboardQueryRejectedException}" />
    public sealed class BlackboardQueryRejectedException : DemocriteBaseException<BlackboardQueryRejectedException>
    {
        public BlackboardQueryRejectedException(Guid queryId,
                                                string? reason,
                                                Exception? innerException)
            : this(BlackboardErrorSR.BlackboardQueryRejectedException.WithArguments(queryId, reason),
                   queryId,
                   reason,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Query,
                                             DemocriteErrorCodes.PartType.Execution,
                                             DemocriteErrorCodes.ErrorType.Rejected),
                   innerException)
        {
        }

        internal BlackboardQueryRejectedException(string message,
                                                  Guid queryId,
                                                  string? reason,
                                                  ulong errorCode,
                                                  Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardQueryRejectedExceptionSurrogate.QueryId), queryId);
            this.Data.Add(nameof(BlackboardQueryRejectedExceptionSurrogate.Reason), reason);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardQueryRejectedExceptionSurrogate(string Message,
                                                                   Guid QueryId,
                                                                   string? Reason,
                                                                   ulong ErrorCode,
                                                                   Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardQueryRejectedExceptionConverter : IConverter<BlackboardQueryRejectedException, BlackboardQueryRejectedExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardQueryRejectedException ConvertFromSurrogate(in BlackboardQueryRejectedExceptionSurrogate surrogate)
        {
            return new BlackboardQueryRejectedException(surrogate.Message,
                                                        surrogate.QueryId,
                                                        surrogate.Reason,
                                                        surrogate.ErrorCode,
                                                        surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardQueryRejectedExceptionSurrogate ConvertToSurrogate(in BlackboardQueryRejectedException value)
        {
            return new BlackboardQueryRejectedExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                QueryId = (Guid)value.Data[nameof(BlackboardQueryRejectedExceptionSurrogate.QueryId)]!,
                Reason = (string)value.Data[nameof(BlackboardQueryRejectedExceptionSurrogate.Reason)]!
            };
        }
    }
}
