// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Elvex.Toolbox.Abstractions.Supports;

    /// <summary>
    /// Define a diagnostic log
    /// </summary>
    public interface IDiagnosticLog : ISupportDebugDisplayName
    {
        /// <summary>
        /// Gets the flow uid.
        /// </summary>
        Guid FlowUID { get; }

        /// <summary>
        /// Gets the current execution identifier.
        /// </summary>
        Guid CurrentExecutionId { get; }

        /// <summary>
        /// Gets the caller identifier.
        /// </summary>
        public Guid? CallerId { get; }

        /// <summary>
        /// Gets the diagnostic type.
        /// </summary>
        DiagnosticLogTypeEnum Type { get; }

        /// <summary>
        /// Gets the record orientation vgrain input or output.
        /// </summary>
        OrientationEnum Orientation { get; }

        /// <summary>
        /// Gets log creation.
        /// </summary>
        DateTime CreateOn { get; }
    }
}
