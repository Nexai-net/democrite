// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when an vgrain if failed to be generated
    /// </summary>
    public sealed class VGrainIdGenerationException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdGenerationException"/> class.
        /// </summary>
        public VGrainIdGenerationException(Type vgrainType, Exception? innerException = null)
            : base(DemocriteExceptionSR.VGrainIdGenerationExceptionMessage.WithArguments(vgrainType),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, DemocriteErrorCodes.PartType.Identifier),
                   innerException)
        {
            this.Data.Add(nameof(vgrainType), vgrainType);
        }
    }
}
