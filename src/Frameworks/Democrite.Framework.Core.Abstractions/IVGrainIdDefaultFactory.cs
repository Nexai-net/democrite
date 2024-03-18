// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Factory in charge to generate default vgrain id based on description attribute used
    /// </summary>
    public interface IVGrainIdDefaultFactory
    {
        #region Methods

        /// <summary>
        /// Builds a new vgrain identifier base on information provide by the <paramref name="input"/> and <paramref name="executionContext"/>
        /// </summary>
        IVGrainId BuildNewId(Type vgrainType, object? input, IExecutionContext? executionContext, ILogger logger);

        #endregion
    }
}
