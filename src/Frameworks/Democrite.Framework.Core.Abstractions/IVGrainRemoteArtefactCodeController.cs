// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using System;

    /// <summary>
    /// VGrain executing remotign code
    /// </summary>
    /// <remarks>
    ///     Context MUST correspond to an artefact Id
    /// </remarks>
    /// <seealso cref="IVGrainRemoteController{Guid}" />
    
    [VGrainIdFormat(Enums.IdFormatTypeEnum.CompositionGuidString,
                           FirstParameterTemplate = "{executionContext." + nameof(IExecutionContext<string>.Configuration) + "}",
                           SecondParameterTemplate = "34EEE26B-FD10-43CF-84B7-79E4D67C0456")]

    public interface IVGrainRemoteArtefactCodeController : IVGrainRemoteController<Guid>
    {
    }
}
