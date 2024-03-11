// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when the vgrain doesn't have been found using the information provided
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainNotFoundException : DemocriteBaseException<VGrainNotFoundException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainNotFoundException"/> class.
        /// </summary>
        public VGrainNotFoundException(AbstractType vgrainType, Exception? innerException = null)
            : this(DemocriteExceptionSR.VGrainTypeNotFounded.WithArguments(vgrainType), innerException)
        {
            this.Data.Add(nameof(vgrainType), vgrainType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainNotFoundException"/> class.
        /// </summary>
        public VGrainNotFoundException(Guid vgrainId, Exception? innerException = null)
            : this(DemocriteExceptionSR.VGrainIdNotFounded.WithArguments(vgrainId), innerException)
        {
            this.Data.Add(nameof(vgrainId), vgrainId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainNotFoundException"/> class.
        /// </summary>
        internal VGrainNotFoundException(string message, Exception? innerException = null)
            : base(message,
                  DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, genericType: DemocriteErrorCodes.ErrorType.NotFounded),
                  innerException)
        {
        }
    }

    [GenerateSerializer]
    public struct VGrainNotFoundExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainNotFoundExceptionConverter : IConverter<VGrainNotFoundException, VGrainNotFoundExceptionSurrogate>
    {
        public VGrainNotFoundException ConvertFromSurrogate(in VGrainNotFoundExceptionSurrogate surrogate)
        {
            return new VGrainNotFoundException(surrogate.Message, surrogate.InnerException);
        }

        public VGrainNotFoundExceptionSurrogate ConvertToSurrogate(in VGrainNotFoundException value)
        {
            return new VGrainNotFoundExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException
            };
        }
    }
}
