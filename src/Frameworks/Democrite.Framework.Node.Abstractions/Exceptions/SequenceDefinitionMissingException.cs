// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when sequence id provide is missing from the <see cref="IWorkfloDefintionManager"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class SequenceDefinitionMissingException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinitionMissingException"/> class.
        /// </summary>
        public SequenceDefinitionMissingException(Guid worflowDefinitionId, Exception? innerException = null)
            : base(NodeAbstractionExceptionSR.SequenceDefinitionMissing.WithArguments(worflowDefinitionId),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {

        }
    }
}
