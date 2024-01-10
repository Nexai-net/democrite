// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    /// <summary>
    /// Build used to setup generic cluster options
    /// </summary>
    public interface IClusterOptionBuilder
    {
        /// <summary>
        /// Blocks the automatic configuration.
        /// </summary>
        IClusterOptionBuilder BlockAutoConfig();
    }
}
