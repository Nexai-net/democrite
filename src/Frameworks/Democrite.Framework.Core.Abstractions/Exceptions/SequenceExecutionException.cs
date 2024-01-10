// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Raised when an unespected exception occured during <see cref="ISequenceExcutor"/> execution
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class SequenceExecutionException : DemocriteBaseException<SequenceExecutionException>
    {
        public SequenceExecutionException(string message, Exception? innerException = null)
            : base(message,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence, DemocriteErrorCodes.PartType.Execution),
                   innerException)
        {
        }
    }

    [GenerateSerializer]
    public struct SequenceExecutionExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class SequenceExecutionExceptionConverter : IConverter<SequenceExecutionException, SequenceExecutionExceptionSurrogate>
    {
        public SequenceExecutionException ConvertFromSurrogate(in SequenceExecutionExceptionSurrogate surrogate)
        {
            return new SequenceExecutionException(surrogate.Message, surrogate.InnerException);
        }

        public SequenceExecutionExceptionSurrogate ConvertToSurrogate(in SequenceExecutionException value)
        {
            return new SequenceExecutionExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode
            };
        }
    }
}
