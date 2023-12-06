// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Base class exception of all system execeptions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class DemocriteBaseException : Exception, IDemocriteException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteBaseException"/> class.
        /// </summary>
        public DemocriteBaseException(string message, ulong errorCode = 0, Exception? innerException = null)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
            this.Data.Add(DemocriteErrorCodes.KEY, errorCode);

#if DEBUG
            this.Data.Add(DemocriteErrorCodes.KEY + "Debug", DemocriteErrorCodes.ToDecryptErrorCode(errorCode));
#endif

        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public ulong ErrorCode { get; }

        #endregion
    }
}
