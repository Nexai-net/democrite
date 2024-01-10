// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when an artefact is missing from the <see cref="IArtifactResourceProvider"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class ArtifactMissingException : DemocriteExecutionBaseException<ArtifactMissingException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactMissingException"/> class.
        /// </summary>
        public ArtifactMissingException(Guid artifactId,
                                        string artifactTypeRequired,
                                        IExecutionContext executionContext,
                                        Exception? innerException = null)
            : this(DemocriteExceptionSR.ArtifactMissing.WithArguments(artifactId, artifactTypeRequired),
                   artifactId,
                   artifactTypeRequired,
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Artifact, genericType: DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactMissingException"/> class.
        /// </summary>
        internal ArtifactMissingException(string message,
                                          Guid artifactId,
                                          string artifactTypeRequired,
                                          Guid flowUID,
                                          Guid? parentExecutionId,
                                          Guid currentExecutionId,
                                          ulong errorCode,
                                          Exception? innerException)
            : base(message, flowUID, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
            base.Data.Add(nameof(ArtifactMissingExceptionSurrogate.ArtifactId), artifactId);
            base.Data.Add(nameof(ArtifactMissingExceptionSurrogate.ArtifactTypeRequired), artifactTypeRequired);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct ArtifactMissingExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Guid FlowUid { get; set; }

        [Id(3)]
        public Guid? ParentExecutionId { get; set; }

        [Id(4)]
        public Guid CurrentExecutionId { get; set; }

        [Id(5)]
        public Exception? InnerException { get; set; }

        [Id(6)]
        public Guid ArtifactId { get; set; }

        [Id(7)]
        public string ArtifactTypeRequired { get; set; }
    }

    [RegisterConverter]
    public sealed class ArtifactMissingExceptionConverter : IConverter<ArtifactMissingException, ArtifactMissingExceptionSurrogate>
    {
        public ArtifactMissingException ConvertFromSurrogate(in ArtifactMissingExceptionSurrogate surrogate)
        {
            return new ArtifactMissingException(surrogate.Message,
                                                surrogate.ArtifactId,
                                                surrogate.ArtifactTypeRequired,
                                                surrogate.FlowUid,
                                                surrogate.ParentExecutionId,
                                                surrogate.CurrentExecutionId,
                                                surrogate.ErrorCode,
                                                surrogate.InnerException);
        }

        public ArtifactMissingExceptionSurrogate ConvertToSurrogate(in ArtifactMissingException value)
        {
            return new ArtifactMissingExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                ArtifactId = (Guid)value.Data[nameof(ArtifactMissingExceptionSurrogate.ArtifactId)]!,
                ArtifactTypeRequired = (string)value.Data[nameof(ArtifactMissingExceptionSurrogate.ArtifactTypeRequired)]!,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,
            };
        }
    }
}
