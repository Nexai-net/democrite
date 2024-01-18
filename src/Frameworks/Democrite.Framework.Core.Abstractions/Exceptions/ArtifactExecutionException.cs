// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when external artifact failed during execution
    /// </summary>
    /// <seealso cref="DemocriteExecutionBaseException{ArtifactExecutionException}" />
    public sealed class ArtifactExecutionException : DemocriteExecutionBaseException<ArtifactExecutionException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutionException"/> class.
        /// </summary>
        public ArtifactExecutionException(Guid artifactUid,
                                          string? details,     
                                          IExecutionContext executionContext,
                                          Exception? innerException = null) 
            : this(DemocriteExceptionSR.ArtifactExecutionException.WithArguments(artifactUid, details),
                   artifactUid,
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Artifact, DemocriteErrorCodes.PartType.Execution),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutionException"/> class.
        /// </summary>
        internal ArtifactExecutionException(string message,
                                            Guid artifactUid,
                                            Guid flowUid,
                                            Guid? parentExecutionId,
                                            Guid currentExecutionId,
                                            ulong errorCode,
                                            Exception? innerException = null)
            : base(message, flowUid, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
            this.Data[nameof(ArtifactExecutionExceptionSurrogate.ArtifactUid)] = artifactUid;
        }
    }

    [GenerateSerializer]
    public struct ArtifactExecutionExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        public string Message { get; set; }

        public ulong ErrorCode { get; set; }

        public Exception? InnerException { get; set; }

        public Guid ArtifactUid { get; set; }
        
        public Guid FlowUid { get; set; }

        public Guid? ParentExecutionId { get; set; }

        public Guid CurrentExecutionId { get; set; }
    }

    [RegisterConverter]
    public sealed class ArtifactExecutionExceptionConverter : IConverter<ArtifactExecutionException, ArtifactExecutionExceptionSurrogate>
    {
        public ArtifactExecutionException ConvertFromSurrogate(in ArtifactExecutionExceptionSurrogate surrogate)
        {
            return new ArtifactExecutionException(surrogate.Message,
                                                  surrogate.ArtifactUid,
                                                  surrogate.FlowUid,
                                                  surrogate.ParentExecutionId,
                                                  surrogate.CurrentExecutionId,
                                                  surrogate.ErrorCode,
                                                  surrogate.InnerException);
        }

        public ArtifactExecutionExceptionSurrogate ConvertToSurrogate(in ArtifactExecutionException value)
        {
            return new ArtifactExecutionExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,
                ArtifactUid = (Guid)value.Data[nameof(ArtifactExecutionExceptionSurrogate.ArtifactUid)]!,
            };
        }
    }
}
