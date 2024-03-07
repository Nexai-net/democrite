// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Rasied when access is not allowed
    /// </summary>
    /// <seealso cref="DemocriteBaseException{DemocriteSecurityIssueException}" />
    public sealed class DemocriteSecurityIssueException : DemocriteBaseException<DemocriteSecurityIssueException>
    {
        #region Ctor

        public DemocriteSecurityIssueException(string details,
                                               string right,
                                               Exception? innerException = null)
            : this(DemocriteExceptionSR.DemocriteSecurityIssueExceptionMessage.WithArguments(right, details),
                   right,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Security, DemocriteErrorCodes.PartType.Right, DemocriteErrorCodes.ErrorType.Failed),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteSecurityIssueException"/> class.
        /// </summary>
        internal DemocriteSecurityIssueException(string message,
                                               string right,
                                               ulong errorCode,
                                               Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(DemocriteSecurityIssueExceptionSurrogate.Right), right);
        }

        #endregion
    }

    [GenerateSerializer]
    public record struct DemocriteSecurityIssueExceptionSurrogate(string Message,
                                                                  string Right,
                                                                  ulong ErrorCode,
                                                                  Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class DemocriteSecurityIssueExceptionConverter : IConverter<DemocriteSecurityIssueException, DemocriteSecurityIssueExceptionSurrogate>
    {
        /// <inheritdoc />
        public DemocriteSecurityIssueException ConvertFromSurrogate(in DemocriteSecurityIssueExceptionSurrogate surrogate)
        {
            return new DemocriteSecurityIssueException(surrogate.Message, surrogate.Right, surrogate.ErrorCode, surrogate.InnerException);
        }

        /// <inheritdoc />
        public DemocriteSecurityIssueExceptionSurrogate ConvertToSurrogate(in DemocriteSecurityIssueException value)
        {
            return new DemocriteSecurityIssueExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                Right = (string)value.Data[nameof(DemocriteSecurityIssueExceptionSurrogate.Right)]!
            };
        }
    }
}
