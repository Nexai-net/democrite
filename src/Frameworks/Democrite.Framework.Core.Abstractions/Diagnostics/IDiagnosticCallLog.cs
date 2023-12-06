// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    /// <summary>
    /// Contain all information used to trace vgrain input and vgrain output
    /// </summary>
    public interface IDiagnosticCallLog : IDiagnosticLog
    {
        #region Properties

        /// <summary>
        /// Gets the full source instance identifier seialized.
        /// </summary>
        string? SourceInstanceId { get; }

        /// <summary>
        /// Gets the full target instance identifier seialized.
        /// </summary>
        string? TargetInstanceId { get; }

        /// <summary>
        /// Gets the type of the target vgrain.
        /// </summary>
        string? TargetVGrainType { get; }

        /// <summary>
        /// Gets the target method.
        /// </summary>
        string? TargetMethod { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is use by orlean system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is orlean system; otherwise, <c>false</c>.
        /// </value>
        bool IsOrleanSystem { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is use by democrite system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is democrite system; otherwise, <c>false</c>.
        /// </value>
        bool IsDemocriteSystem { get; }

        /// <summary>
        /// Gets the silo information.
        /// </summary>
        NodeInfo SiloInfo { get; }

        #endregion
    }
}
