// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Elvex.Toolbox.Extensions;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
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
        #region Fields

        private static readonly MethodInfo s_genericCreateWithResultMethod;

        #endregion

        #region ctor

        /// <summary>
        /// Initializes the <see cref="ExecutionResult"/> class.
        /// </summary>
        static ExecutionResult()
        {
            var genericCreateWithResultMethod = typeof(ExecutionResult).GetMethods()
                                                                       .Where(m => m.IsPublic && m.IsStatic && m.Name == nameof(Create) && m.IsGenericMethod)
                                                                       .Select(m => m.GetGenericMethodDefinition())
                                                                       .FirstOrDefault();

            Debug.Assert(genericCreateWithResultMethod is not null);
            s_genericCreateWithResultMethod = genericCreateWithResultMethod;
        }

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
        public static IExecutionResult CreateWithResult(IExecutionContext executionContext,
                                                        object? result,
                                                        Type resultType,
                                                        Exception? exception,
                                                        string? message = null,
                                                        bool? succeeded = null)
        {
            return (IExecutionResult)s_genericCreateWithResultMethod.MakeGenericMethod(resultType).Invoke(null, new object?[] { executionContext, result, exception, message, succeeded })!;
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
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(message))
                builder.AppendLine(message);

            var exceptionStr = exception?.GetFullString(true);

            if (!string.IsNullOrEmpty(exceptionStr))
                builder.Append(exceptionStr);

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

    /// <summary>
    /// Result container that provide information about execution and result <typeparamref name="TResult"/>
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="IExecutionResult" />
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ExecutionResultStruct<TResult>(TResult? Output,
                                                        Guid ExecutionId,
                                                        bool Succeeded,
                                                        bool Cancelled,
                                                        string? ErrorCode,
                                                        string? Message,
                                                        bool HasOutput,
                                                        string? OutputType) : IExecutionResult<TResult>
    {
        /// <inheritdoc />
        public object? GetOutput()
        {
            return (object?)this.Output;
        }
    }
}
