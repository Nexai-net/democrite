// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when an vgrain doesn't expose the attribute <see cref="ExpectedVGrainIdFormat"/>
    /// </summary>
    /// <remarks>
    ///     Attention this attribute is not herited.
    /// </remarks>
    public sealed class VGrainIdTemplateMissingException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdTemplateMissingException"/> class.
        /// </summary>
        public VGrainIdTemplateMissingException(Type vgrainType)
            : base(DemocriteExceptionSR.VGrainIdTemplateMissingExceptionMessage.WithArguments(vgrainType),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, DemocriteErrorCodes.PartType.MetaInformation, DemocriteErrorCodes.ErrorType.Missing))
        {
            this.Data.Add(nameof(vgrainType), vgrainType);
        }
    }
}
