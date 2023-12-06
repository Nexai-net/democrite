// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using System;

    /// <summary>
    /// Command used to pass argument to remote execution
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    internal sealed class RemoteExecutionCommand<TContent>
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteExecutionCommand{TContent}"/> class.
        /// </summary>
        public RemoteExecutionCommand(Guid executionId, TContent? content)
        {
            this.ExecutionId = executionId;
            this.Content = content;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the execution (sequence) identifier.
        /// </summary>
        public Guid ExecutionId { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public TContent? Content { get; }

        #endregion
    }
}
