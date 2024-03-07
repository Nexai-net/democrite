// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when access a blackboard by a direct uid missing
    /// </summary>
    /// <seealso cref="DemocriteBaseException{BlackboardMissingDemocriteException}" />
    public sealed class BlackboardMissingDemocriteException : DemocriteBaseException<BlackboardMissingDemocriteException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardMissingDemocriteException"/> class.
        /// </summary>
        public BlackboardMissingDemocriteException(Guid? uid,
                                                   string? blackboardName,
                                                   string? blackboardTemplate,
                                                   Exception? innerException = null)
            : this(uid,
                   blackboardName,
                   blackboardTemplate,
                   BlackboardErrorSR.BlackboardMissingDemocriteException.WithArguments(uid, blackboardName, blackboardTemplate),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, genericType: DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardMissingDemocriteException"/> class.
        /// </summary>
        internal BlackboardMissingDemocriteException(Guid? searchUid,
                                                     string? blackboardName,
                                                     string? blackboardTemplate,
                                                     string message,
                                                     ulong errorCode,
                                                     Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardMissingDemocriteExceptionSurrogate.SearchUid), searchUid);
            this.Data.Add(nameof(BlackboardMissingDemocriteExceptionSurrogate.BlackboardName), blackboardName);
            this.Data.Add(nameof(BlackboardMissingDemocriteExceptionSurrogate.BlackboardTemplate), blackboardTemplate);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardMissingDemocriteExceptionSurrogate(Guid? SearchUid,
                                                                      string? BlackboardName,
                                                                      string? BlackboardTemplate,
                                                                      string Message,
                                                                      ulong ErrorCode,
                                                                      Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardMissingDemocriteExceptionConverter : IConverter<BlackboardMissingDemocriteException, BlackboardMissingDemocriteExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardMissingDemocriteException ConvertFromSurrogate(in BlackboardMissingDemocriteExceptionSurrogate surrogate)
        {
            return new BlackboardMissingDemocriteException(surrogate.SearchUid,
                                                           surrogate.BlackboardName,
                                                           surrogate.BlackboardTemplate,
                                                           surrogate.Message,
                                                           surrogate.ErrorCode,
                                                           surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardMissingDemocriteExceptionSurrogate ConvertToSurrogate(in BlackboardMissingDemocriteException value)
        {
            return new BlackboardMissingDemocriteExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                SearchUid = (Guid?)value.Data[nameof(BlackboardMissingDemocriteExceptionSurrogate.SearchUid)]!,
                BlackboardName = (string?)value.Data[nameof(BlackboardMissingDemocriteExceptionSurrogate.BlackboardName)]!,
                BlackboardTemplate = (string?)value.Data[nameof(BlackboardMissingDemocriteExceptionSurrogate.BlackboardTemplate)]!
            };
        }
    }
}
