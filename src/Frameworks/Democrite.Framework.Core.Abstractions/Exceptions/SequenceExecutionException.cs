// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Raised when an unespected exception occured during <see cref="ISequenceExcutor"/> execution
    /// </summary>
    /// <seealso cref="Democrite.Framework.Core.Abstractions.Exceptions.DemocriteBaseException" />
    public sealed class SequenceExecutionException : DemocriteBaseException
    {
        public SequenceExecutionException(string message, Exception? innerException = null)
            : base(message,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence, DemocriteErrorCodes.PartType.Execution),
                   innerException)
        {

        }
    }
}
