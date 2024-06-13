// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using System;

    /// <summary>
    /// Response container used to pass argument to remote execution
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    internal sealed class RemoteExecutionResponse
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteExecutionResponse{TContent}"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public RemoteExecutionResponse(Guid executionId,
                                       string? content,
                                       string errorCode,
                                       string message,
                                       bool success = true)
        {
            this.ExecutionId = executionId;
            this.Content = content;
            this.Success = success;
            this.Message = message;
            this.ErrorCode = errorCode;
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
        public string? Content { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="RemoteExecutionResponse{TContent}"/> is sucess.
        /// </summary>
        /// <value>
        ///   <c>true</c> if sucess; otherwise, <c>false</c>.
        /// </value>
        public bool Success { get; }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public string? ErrorCode { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string? Message { get; }

        #endregion
    }
}
