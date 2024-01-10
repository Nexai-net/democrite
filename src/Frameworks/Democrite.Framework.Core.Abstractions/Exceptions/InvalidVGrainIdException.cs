// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Raised vgrain id is not the one expected
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidVGrainIdException : DemocriteBaseException<InvalidVGrainIdException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVGrainIdException"/> class.
        /// </summary>
        public InvalidVGrainIdException(GrainId receivedId, string expectDetails, Exception? innerException = null)
            : this(DemocriteExceptionSR.InvalidVGrainIdExceptionMessage.WithArguments(receivedId, expectDetails),
                   receivedId,
                   expectDetails,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, DemocriteErrorCodes.PartType.Identifier, DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVGrainIdException"/> class.
        /// </summary>
        internal InvalidVGrainIdException(string message,
                                          GrainId receivedId,
                                          string expectDetails,
                                          ulong errorCode,
                                          Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(InvalidVGrainIdExceptionSurrogate.ReceivedId), receivedId);
            this.Data.Add(nameof(InvalidVGrainIdExceptionSurrogate.ExpectDetails), expectDetails);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct InvalidVGrainIdExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public GrainId ReceivedId { get; set; }

        [Id(3)]
        public string ExpectDetails { get; set; }

        [Id(4)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class InvalidVGrainIdExceptionConverter : IConverter<InvalidVGrainIdException, InvalidVGrainIdExceptionSurrogate>
    {
        public InvalidVGrainIdException ConvertFromSurrogate(in InvalidVGrainIdExceptionSurrogate surrogate)
        {
            return new InvalidVGrainIdException(surrogate.Message,
                                                surrogate.ReceivedId,
                                                surrogate.ExpectDetails,
                                                surrogate.ErrorCode,
                                                surrogate.InnerException);
        }

        public InvalidVGrainIdExceptionSurrogate ConvertToSurrogate(in InvalidVGrainIdException value)
        {
            return new InvalidVGrainIdExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                ReceivedId = (GrainId)value.Data[nameof(InvalidVGrainIdExceptionSurrogate.ReceivedId)]!,
                ExpectDetails = (string)value.Data[nameof(InvalidVGrainIdExceptionSurrogate.ExpectDetails)]!
            };
        }
    }
}
