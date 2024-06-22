// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Resources;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// Raised when door information provide is missing from the <see cref="IDoorDefinitionProvider"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class StageExecutorNotFoundException : DemocriteBaseException<StageExecutorNotFoundException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StageExecutorNotFoundException"/> class.
        /// </summary>
        public StageExecutorNotFoundException(SequenceStageDefinition stageDefinition, Exception? innerException = null)
            : this(DemocriteExceptionSR.StageExecutorNotFoundException.WithArguments(stageDefinition.Type),
                   stageDefinition.Type,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Door, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.NotFounded),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageExecutorNotFoundException"/> class.
        /// </summary>
        internal StageExecutorNotFoundException(string message,
                                                StageTypeEnum stageType,
                                                ulong errorCode,
                                                Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(StageExecutorNotFoundExceptionSurrogate.StageType), stageType);
        }
    }

    [GenerateSerializer]
    public record struct StageExecutorNotFoundExceptionSurrogate(string Message,
                                                                 StageTypeEnum StageType,
                                                                 ulong ErrorCode,
                                                                 Exception? InnerException) : IDemocriteBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class StageExecutorNotFoundExceptionConverter : IConverter<StageExecutorNotFoundException, StageExecutorNotFoundExceptionSurrogate>
    {
        public StageExecutorNotFoundException ConvertFromSurrogate(in StageExecutorNotFoundExceptionSurrogate surrogate)
        {
            return new StageExecutorNotFoundException(surrogate.Message,
                                                      surrogate.StageType,
                                                      surrogate.ErrorCode,
                                                      surrogate.InnerException);
        }

        public StageExecutorNotFoundExceptionSurrogate ConvertToSurrogate(in StageExecutorNotFoundException value)
        {
            return new StageExecutorNotFoundExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                StageType = (StageTypeEnum)value.Data[nameof(StageExecutorNotFoundExceptionSurrogate.StageType)]!
            };
        }
    }
}
