// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when an artefact is of wrong type
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class ArtifactInvalidTypeException : DemocriteExecutionBaseException<ArtifactInvalidTypeException>
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
            : this(DemocriteExceptionSR.ArtifactWrongType.WithArguments(artifactId, artifactType, expectedType),
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   artifactId,
                   artifactType.GetAbstractType(),
                   expectedType.GetAbstractType(),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Artifact,
                                             DemocriteErrorCodes.PartType.Type,
                                             DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactInvalidTypeException"/> class.
        /// </summary>
        internal ArtifactInvalidTypeException(string message,
                                              Guid flowUID,
                                              Guid? parentExecutionId,
                                              Guid currentExecutionId,
                                              Guid artifactId,
                                              AbstractType artifactType,
                                              AbstractType expectedType,
                                              ulong errorCode,
                                              Exception? innerException)
            : base(message, flowUID, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
            base.Data.Add(nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactId), artifactId);
            base.Data.Add(nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactType), artifactType);
            base.Data.Add(nameof(ArtifactInvalidTypeExceptionSurrogate.ExpectedType), expectedType);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct ArtifactInvalidTypeExceptionSurrogate : IDemocriteBaseExceptionSurrogate
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
        public AbstractType ArtifactType { get; set; }

        [Id(6)]
        public AbstractType ExpectedType { get; set; }

        [Id(7)]
        public Exception? InnerException { get; set; }

        [Id(8)]
        public Guid ArtifactId { get; set; }
    }

    [RegisterConverter]
    public sealed class ArtifactInvalidTypeExceptionConverter : IConverter<ArtifactInvalidTypeException, ArtifactInvalidTypeExceptionSurrogate>
    {
        public ArtifactInvalidTypeException ConvertFromSurrogate(in ArtifactInvalidTypeExceptionSurrogate surrogate)
        {
            return new ArtifactInvalidTypeException(surrogate.Message,
                                                    surrogate.FlowUid,
                                                    surrogate.ParentExecutionId,
                                                    surrogate.CurrentExecutionId,
                                                    surrogate.ArtifactId,
                                                    surrogate.ArtifactType,
                                                    surrogate.ExpectedType,
                                                    surrogate.ErrorCode,
                                                    surrogate.InnerException);
        }

        public ArtifactInvalidTypeExceptionSurrogate ConvertToSurrogate(in ArtifactInvalidTypeException value)
        {
            return new ArtifactInvalidTypeExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,

                ArtifactId = (Guid)value.Data[nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactId)]!,
                ArtifactType = (AbstractType)value.Data[nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactType)]!,
                ExpectedType = (AbstractType)value.Data[nameof(ArtifactInvalidTypeExceptionSurrogate.ExpectedType)]!,
            };
        }
    }
}
