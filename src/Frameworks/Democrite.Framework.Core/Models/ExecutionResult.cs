// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;

    /// <summary>
    /// Result container that provide information about execution
    /// </summary>
    /// <seealso cref="IExecutionResult" />
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class ExecutionResult : IExecutionResult
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionResult"/> class.
        /// </summary>
        public ExecutionResult(Guid executionId,
                               bool succeeded,
                               bool cancelled,
                               string? errorCode,
                               string? message,
                               bool hasOutput,
                               string? outputType)
        {
            this.ExecutionId = executionId;
            this.Succeeded = succeeded;
            this.Cancelled = cancelled;
            this.ErrorCode = errorCode;
            this.Message = message;
            this.HasOutput = hasOutput;
            this.OutputType = outputType;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        public Guid ExecutionId { get; }

        /// <inheritdoc />
        [Id(1)]
        public bool Succeeded { get; }

        /// <inheritdoc />
        [Id(2)]
        public bool Cancelled { get; }

        /// <inheritdoc />
        [Id(3)]
        public string? ErrorCode { get; }

        /// <inheritdoc />
        [Id(4)]
        public string? Message { get; }

        /// <inheritdoc />
        [Id(5)]
        public bool HasOutput { get; }

        /// <inheritdoc />
        [Id(6)]
        public string? OutputType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual object? GetOutput()
        {
            return null;
        }

        /// <summary>
        /// Creates <see cref="IExecutionResult"/> from execution informations
        /// </summary>
        public static IExecutionResult Create(IExecutionContext executionContext,
                                              Exception? exception,
                                              string? message = null,
                                              bool? succeeded = null)
        {
            return new ExecutionResult(executionContext.FlowUID,
                                       exception == null && (succeeded == null || succeeded == true),
                                       exception is OperationCanceledException,
                                       (exception is IDemocriteException democriteException ? democriteException.ErrorCode.ToString() : string.Empty),
                                       GetMessage(exception, message),
                                       false,
                                       null);
        }

        /// <summary>
        /// Creates <see cref="IExecutionResult{TResult}"/> from execution informations with result value <paramref name="result"/>
        /// </summary>
        public static IExecutionResult<TResult> Create<TResult>(IExecutionContext executionContext,
                                                                TResult? result,
                                                                Exception? exception,
                                                                string? message = null,
                                                                bool? succeeded = null)
        {
            return new ExecutionResult<TResult>(executionContext.FlowUID,
                                                exception == null && (succeeded == null || succeeded == true),
                                                exception is OperationCanceledException,
                                                (exception is IDemocriteException democriteException ? democriteException.ErrorCode.ToString() : string.Empty),
                                                GetMessage(exception, message),
                                                result);
        }

        /// <summary>
        /// Build result message from exception and custom message combinaison
        /// </summary>
        private static string? GetMessage(Exception? exception, string? message)
        {
            if (exception == null && string.IsNullOrEmpty(message))
                return string.Empty;

            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(message))
            {
                builder.Append(message);
            }

            if (exception != null)
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

                builder.Append(exception.GetType().Name);
                builder.AppendLine(exception.Message);

                foreach (DictionaryEntry data in exception.Data)
                {
                    builder.Append(' ', 4);
                    builder.Append(data.Key);
                    builder.Append(" : ");
                    builder.Append(data.Value);
                }
            }

            return builder.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Result container that provide information about execution and result <typeparamref name="TResult"/>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="IExecutionResult" />
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class ExecutionResult<TResult> : ExecutionResult, IExecutionResult<TResult>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionResult{TResult}"/> class.
        /// </summary>
        public ExecutionResult(Guid executionId,
                               bool succeeded,
                               bool cancelled,
                               string? errorCode,
                               string? message,
                               TResult? output)
            : base(executionId,
                   succeeded,
                   cancelled,
                   errorCode,
                   message,
                   !EqualityComparer<TResult>.Default.Equals(output, default),
                   typeof(TResult).Name)
        {
            this.Output = output;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        public TResult? Output { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override object? GetOutput()
        {
            return this.Output;
        }

        #endregion
    }
}
