// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Factory in charge to generate vgrain id following the format describe by <see cref="ExpectedVGrainIdFormatAttribute"/>
    /// </summary>
    public interface IVGrainIdFactory
    {
        #region Methods

        /// <summary>
        /// Builds a new vgrain identifier base on information provide by the <paramref name="input"/> and <paramref name="executionContext"/> 
        /// following the format describe by the attribute <see cref="ExpectedVGrainIdFormatAttribute"/> tag above the <typeparamref name="TVGrain"/> interface
        /// </summary>
        /// <exception cref="VGrainIdTemplateMissingException"> Raised when attribute <see cref="ExpectedVGrainIdFormatAttribute"/> is missing </exception>
        /// <exception cref="VGrainIdGenerationException">Raised when information need for the generation are invalid or missings</exception>
        IVGrainId BuildNewId<TVGrain>(object? input, IExecutionContext? executionContext, ILogger logger)
            where TVGrain : IVGrain;

        /// <summary>
        /// Builds a new vgrain identifier base on information provide by the <paramref name="input"/> and <paramref name="executionContext"/> 
        /// following the format describe by the attribute <see cref="ExpectedVGrainIdFormatAttribute"/> tag above the <typeparamref name="TVGrain"/> interface
        /// </summary>
        /// <exception cref="VGrainIdTemplateMissingException"> Raised when attribute <see cref="ExpectedVGrainIdFormatAttribute"/> is missing </exception>
        /// <exception cref="VGrainIdGenerationException">Raised when information need for the generation are invalid or missings</exception>
        IVGrainId BuildNewId(Type vgrainType, object? input, IExecutionContext? executionContext, ILogger logger);

        #endregion
    }
}
