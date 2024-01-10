// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Resources;

    using Orleans;

    using System;

    /// <summary>
    /// Raised when <see cref="IExternalCodeExecutorPreparationStep"/> setup/unstep failed
    /// </summary>
    /// <seealso cref="DemocriteExecutionBaseException" />
    public sealed class ArtifactPreparationFailedException : DemocriteExecutionBaseException<ArtifactPreparationFailedException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactPreparationFailedException"/> class.
        /// </summary>
        public ArtifactPreparationFailedException(string externalCodeExecutorPreparationStep,
                                                  IExecutionContext executionContext,
                                                  Exception? innerException = null)
            : this(DemocriteExceptionSR.ArtifactPreparationFailed.WithArguments(externalCodeExecutorPreparationStep),
                   externalCodeExecutorPreparationStep?.ToString() ?? string.Empty,
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Artifact,
                                             DemocriteErrorCodes.PartType.Setup,
                                             DemocriteErrorCodes.ErrorType.Failed),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactPreparationFailedException"/> class.
        /// </summary>
        internal ArtifactPreparationFailedException(string message,
                                                    string externalCodeExecutorPreparationStepString,
                                                    Guid flowUid,
                                                    Guid? parentExecutionId,
                                                    Guid currentExecutionId,
                                                    ulong errorCode,
                                                    Exception? innerException = null) 
            : base(message, flowUid, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
            this.Data.Add(nameof(ArtifactPreparationFailedExceptionSurrogate.ExternalCodeExecutorPreparationStep), externalCodeExecutorPreparationStepString);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct ArtifactPreparationFailedExceptionSurrogate : IDemocriteBaseExceptionSurrogate
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
        public string ExternalCodeExecutorPreparationStep { get; set; }
    }

    [RegisterConverter]
    public sealed class ArtifactPreparationFailedExceptionConverter : IConverter<ArtifactPreparationFailedException, ArtifactPreparationFailedExceptionSurrogate>
    {
        public ArtifactPreparationFailedException ConvertFromSurrogate(in ArtifactPreparationFailedExceptionSurrogate surrogate)
        {
           return new ArtifactPreparationFailedException(surrogate.Message,
                                                         surrogate.ExternalCodeExecutorPreparationStep,
                                                         surrogate.FlowUid,
                                                         surrogate.ParentExecutionId,
                                                         surrogate.CurrentExecutionId,
                                                         surrogate.ErrorCode,
                                                         surrogate.InnerException);
        }

        public ArtifactPreparationFailedExceptionSurrogate ConvertToSurrogate(in ArtifactPreparationFailedException value)
        {
            return new ArtifactPreparationFailedExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                ExternalCodeExecutorPreparationStep = (string)value.Data[nameof(ArtifactPreparationFailedExceptionSurrogate.ExternalCodeExecutorPreparationStep)]!,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,
            };
        }
    }
}
