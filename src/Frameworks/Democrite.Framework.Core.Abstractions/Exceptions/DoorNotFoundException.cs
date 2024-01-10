// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when door information provide is missing from the <see cref="IDoorDefinitionProvider"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class DoorNotFoundException : DemocriteBaseException<DoorNotFoundException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoorNotFoundException"/> class.
        /// </summary>
        public DoorNotFoundException(string doorIdentifier, Exception? innerException = null)
            : this(DemocriteExceptionSR.DoorNotFounded.WithArguments(doorIdentifier),
                   doorIdentifier,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Door, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.NotFounded),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorNotFoundException"/> class.
        /// </summary>
        internal DoorNotFoundException(string message,
                                       string doorIdentifier,
                                       ulong errorCode,
                                       Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(DoorNotFoundExceptionSurrogate.DoorIdentifier), doorIdentifier);
        }
    }

    [GenerateSerializer]
    public struct DoorNotFoundExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public string DoorIdentifier { get; set; }

        [Id(2)]
        public ulong ErrorCode { get; set; }

        [Id(3)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class DoorNotFoundExceptionConverter : IConverter<DoorNotFoundException, DoorNotFoundExceptionSurrogate>
    {
        public DoorNotFoundException ConvertFromSurrogate(in DoorNotFoundExceptionSurrogate surrogate)
        {
            return new DoorNotFoundException(surrogate.Message,
                                             surrogate.DoorIdentifier,
                                             surrogate.ErrorCode,
                                             surrogate.InnerException);
        }

        public DoorNotFoundExceptionSurrogate ConvertToSurrogate(in DoorNotFoundException value)
        {
            return new DoorNotFoundExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                DoorIdentifier = (string)value.Data[nameof(DoorNotFoundExceptionSurrogate.DoorIdentifier)]!
            };
        }
    }
}
