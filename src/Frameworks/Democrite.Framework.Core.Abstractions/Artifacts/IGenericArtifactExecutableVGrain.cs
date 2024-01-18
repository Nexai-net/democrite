// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    /// <summary>
    /// VGrain executing remotign code
    /// </summary>
    /// <remarks>
    ///     Context MUST correspond to an artefact Id
    /// </remarks>
    /// <seealso cref="IVGrainRemoteController{Guid}" />

    [VGrainIdFormat(Enums.IdFormatTypeEnum.CompositionGuidString,
                    FirstParameterTemplate = "{executionContext." + nameof(IExecutionContext<string>.Configuration) + "}")]

    public interface IGenericArtifactExecutableVGrain : IVGrain, IGrainWithGuidCompoundKey, IGenericContextedExecutor<Guid>
    {
    }
}
