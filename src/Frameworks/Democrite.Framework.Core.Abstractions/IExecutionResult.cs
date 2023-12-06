// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using System;

    /// <summary>
    /// Result information about any execution trigger by <see cref="IDemocriteExecutionHandler"/>
    /// </summary>
    public interface IExecutionResult
    {
        #region Properties

        /// <summary>
        /// Gets the execution identifier.
        /// </summary>
        /// <remarks>
        ///     Unique id used on all the execution, log and data save
        /// </remarks>
        Guid ExecutionId { get; }

        /// <summary>
        /// Gets a value indicating whether this execution succeeded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if succeeded; otherwise, <c>false</c>.
        /// </value>
        bool Succeeded { get; }

        /// <summary>
        /// Gets a value indicating whether this execution cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancelled; otherwise, <c>false</c>.
        /// </value>
        bool Cancelled { get; }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        ///     Custom code used to simply the automatic error managment
        /// </value>
        string? ErrorCode { get; }

        /// <summary>
        /// Gets a message that qualify the execution.
        /// </summary>
        /// <value>
        ///     The message could be an error label, warning, or information.
        /// </value>
        string? Message { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IExecutionResult"/> has output.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="IExecutionResult"/> has output; otherwise, <c>false</c>.
        /// </value>
        bool HasOutput { get; }

        /// <summary>
        /// Gets the type of the output if <see cref="HasOutput"/> is true; otherwise null;
        /// </summary>
        string? OutputType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the output if <see cref="HasOutput"/> is true; otherwise null;
        /// </summary>
        object? GetOutput();

        #endregion
    }

    /// <summary>
    /// Result information about any execution trigger by <see cref="IDemocriteExecutionHandler"/>
    /// </summary>
    public interface IExecutionResult<TOutput> : IExecutionResult
    {
        /// <summary>
        /// Gets the typed output.
        /// </summary>
        TOutput? Output { get; }
    }
}
