// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when the vgrain doesn't have been found using the information provided
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainNotFoundException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainNotFoundException"/> class.
        /// </summary>
        public VGrainNotFoundException(Type vgrainType, Exception? innerException = null)
            : this(NodeAbstractionExceptionSR.VGrainTypeNotFounded.WithArguments(vgrainType), innerException)
        {
            this.Data.Add(nameof(vgrainType), vgrainType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainNotFoundException"/> class.
        /// </summary>
        public VGrainNotFoundException(Guid vgrainId, Exception? innerException = null)
            : this(NodeAbstractionExceptionSR.VGrainIdNotFounded.WithArguments(vgrainId), innerException)
        {
            this.Data.Add(nameof(vgrainId), vgrainId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainNotFoundException"/> class.
        /// </summary>
        private VGrainNotFoundException(string message, Exception? innerException = null)
            : base(message,
                  DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, genericType: DemocriteErrorCodes.ErrorType.NotFounded),
                  innerException)
        {
        }
    }
}
