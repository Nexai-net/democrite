// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;

    using System;

    /// <summary>
    /// Base exception during execution
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public abstract class DemocriteExecutionBaseException<TChild> : DemocriteBaseException<TChild>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteExecutionBaseException"/> class.
        /// </summary>
        public DemocriteExecutionBaseException(string message,
                                               Guid flowUid,
                                               Guid? parentExecutionId,
                                               Guid currentExecutionId,
                                               ulong errorCode,
                                               Exception? innerException = null)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(IExecutionContext.FlowUID), flowUid);
            this.Data.Add(nameof(IExecutionContext.ParentExecutionId), parentExecutionId);
            this.Data.Add(nameof(IExecutionContext.CurrentExecutionId), currentExecutionId);
        }

        #endregion
    }
}
