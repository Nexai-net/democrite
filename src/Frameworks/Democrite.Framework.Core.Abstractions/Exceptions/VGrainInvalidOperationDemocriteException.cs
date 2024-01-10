// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    /// <summary>
    /// Raised when a not autorize action is done that MUST not arrived
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainInvalidOperationDemocriteException : DemocriteExecutionBaseException<VGrainInvalidOperationDemocriteException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMethodDemocriteException"/> class.
        /// </summary>
        public VGrainInvalidOperationDemocriteException(string message,
                                                        IExecutionContext executionContext,
                                                        Exception? innerException = null)
            : this(message,
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence, DemocriteErrorCodes.PartType.Execution, DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainInvalidOperationDemocriteException"/> class.
        /// </summary>
        internal VGrainInvalidOperationDemocriteException(string message,
                                                          Guid flowUid,
                                                          Guid? parentExecutionId,
                                                          Guid currentExecutionId,
                                                          ulong errorCode,
                                                          Exception? innerException = null)
            : base(message, flowUid, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
        }

        #endregion
    }

    [GenerateSerializer]
    public struct VGrainInvalidOperationDemocriteExceptionSurrogate : IDemocriteBaseExceptionSurrogate
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
    }

    [RegisterConverter]
    public sealed class VGrainInvalidOperationDemocriteExceptionConverter : IConverter<VGrainInvalidOperationDemocriteException, VGrainInvalidOperationDemocriteExceptionSurrogate>
    {
        public VGrainInvalidOperationDemocriteException ConvertFromSurrogate(in VGrainInvalidOperationDemocriteExceptionSurrogate surrogate)
        {
            return new VGrainInvalidOperationDemocriteException(surrogate.Message,
                                                                surrogate.FlowUid,
                                                                surrogate.ParentExecutionId,
                                                                surrogate.CurrentExecutionId,
                                                                surrogate.ErrorCode,
                                                                surrogate.InnerException);
        }

        public VGrainInvalidOperationDemocriteExceptionSurrogate ConvertToSurrogate(in VGrainInvalidOperationDemocriteException value)
        {
            return new VGrainInvalidOperationDemocriteExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,
                InnerException = value.InnerException
            };
        }
    }
}
