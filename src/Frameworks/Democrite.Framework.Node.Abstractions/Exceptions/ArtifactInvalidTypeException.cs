// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when an artefact is of wrong type
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class ArtifactInvalidTypeException : DemocriteExecutionBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactInvalidTypeException"/> class.
        /// </summary>
        public ArtifactInvalidTypeException(Guid artifactId,
                                            Type artifactType,
                                            Type expectedType,
                                            IExecutionContext executionContext,
                                            Exception? innerException = null)
            : base(NodeAbstractionExceptionSR.ArtifactWrongType.WithArguments(artifactId, artifactType, expectedType),
                   executionContext,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Artifact,
                                             DemocriteErrorCodes.PartType.Type,
                                             DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            base.Data.Add(nameof(artifactId), artifactId);
            base.Data.Add(nameof(artifactType), artifactType);
            base.Data.Add(nameof(expectedType), expectedType);
        }

        #endregion
    }
}
