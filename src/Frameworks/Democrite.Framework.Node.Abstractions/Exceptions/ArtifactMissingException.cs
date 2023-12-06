// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when an artefact is missing from the <see cref="IArtifactResourceProvider"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class ArtifactMissingException : DemocriteExecutionBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactMissingException"/> class.
        /// </summary>
        public ArtifactMissingException(Guid artifactId,
                                        string artifactTypeRequired,
                                        IExecutionContext executionContext,
                                        Exception? innerException = null)
            : base(NodeAbstractionExceptionSR.ArtifactMissing.WithArguments(artifactId, artifactTypeRequired),
                   executionContext,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Artifact, genericType: DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {
            base.Data.Add(nameof(artifactId), artifactId);
            base.Data.Add(nameof(artifactTypeRequired), artifactTypeRequired);
        }

        #endregion
    }
}
