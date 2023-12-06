// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Models
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Generic remote call/reponse message
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    internal class RemoteCallMessage
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteCallMessage"/> class.
        /// </summary>
        public RemoteCallMessage(string instanceId,
                                 Guid executionId,
                                 string? jsonContent,
                                 string? error,
                                 bool isCancelled)
        {
            this.InstanceId = instanceId;
            this.ExecutionId = executionId;
            this.JsonContent = jsonContent;
            this.Error = error;
            this.IsCancelled = isCancelled;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the instance identifier.
        /// </summary>
        public string InstanceId { get; }

        /// <summary>
        /// Gets or sets the unique execution identifier.
        /// </summary>
        public Guid ExecutionId { get; }

        /// <summary>
        /// Gets or sets the content format in json.
        /// </summary>
        /// <remarks>
        ///     This message is used to call but also to get the result
        /// </remarks>
        public string? JsonContent { get; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCancelled { get; }

        #endregion
    }
}