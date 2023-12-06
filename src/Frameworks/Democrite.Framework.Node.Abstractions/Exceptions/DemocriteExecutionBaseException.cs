// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Base exception during execution
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public abstract class DemocriteExecutionBaseException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteExecutionBaseException"/> class.
        /// </summary>
        public DemocriteExecutionBaseException(string message,
                                               IExecutionContext executionContext,
                                               ulong errorCode,
                                               Exception? innerException = null)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(IExecutionContext), executionContext);
        }

        #endregion
    }
}
