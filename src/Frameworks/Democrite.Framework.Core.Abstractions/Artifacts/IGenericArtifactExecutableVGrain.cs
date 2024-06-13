// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Concurrency;

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
        /// <summary>
        /// Execute and return result
        /// </summary>
        /// <remarks>
        ///     Method used to allow multiple call executed on the same time (Attention: Artifact local installation is threadsafe)
        /// </remarks>
        [ReadOnly]
        Task<TOutput?> ConcurrentRunAsync<TOutput>(IExecutionContext<Guid> executionContext);

        /// <summary>
        /// Execute with <paramref name="input"/> and return result
        /// </summary>
        /// <remarks>
        ///     Method used to allow multiple call executed on the same time (Attention: Artifact local installation is threadsafe)
        /// </remarks>
        [ReadOnly]
        Task<TOutput?> ConcurrentRunAsync<TOutput, TInput>(TInput? input, IExecutionContext<Guid> executionContext);

        /// <summary>
        /// Execute and wait the end
        /// </summary>
        /// <remarks>
        ///     Method used to allow multiple call executed on the same time (Attention: Artifact local installation is threadsafe)
        /// </remarks>
        [ReadOnly]
        Task ConcurrentRunAsync(IExecutionContext<Guid> executionContext);

        /// <summary>
        /// Execute and wait the end
        /// </summary>
        /// <remarks>
        ///     Method used to allow multiple call executed on the same time (Attention: Artifact local installation is threadsafe)
        /// </remarks>
        [ReadOnly]
        Task ConcurrentRunWithInputAsync<TInput>(TInput? input, IExecutionContext<Guid> executionContext);
    }
}
