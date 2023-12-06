// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Global cluser node options
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ClusterNodeDiagnosticOptions : INodeOptions
    {
        #region Fields

        public const string ConfiguratioName = "Diagnostic";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ClusterNodeDiagnosticOptions"/> class.
        /// </summary>
        static ClusterNodeDiagnosticOptions()
        {
            Default = new ClusterNodeDiagnosticOptions(false, false, true, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeDiagnosticOptions"/> class.
        /// </summary>
        public ClusterNodeDiagnosticOptions()
            : this(Default.DisableVGrainExecutionTracing,
                   Default.TraceOrleanSystemVGrain,
                   Default.TraceDemocriteSystemVGrain,
                   Default.DisableVGrainInOutTracing)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeDiagnosticOptions"/> class.
        /// </summary>
        public ClusterNodeDiagnosticOptions(bool disableVGrainExecutionTracing,
                                            bool traceOrleanSystemVGrain,
                                            bool traceDemocriteSystemVGrain,
                                            bool disableVGrainInOutTracing)
        {
            this.DisableVGrainExecutionTracing = disableVGrainExecutionTracing;
            this.TraceOrleanSystemVGrain = traceOrleanSystemVGrain;
            this.TraceDemocriteSystemVGrain = traceDemocriteSystemVGrain;
            this.DisableVGrainInOutTracing = disableVGrainInOutTracing;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default <see cref="ClusterNodeDiagnosticOptions"/>
        /// </summary>
        public static ClusterNodeDiagnosticOptions Default { get; }

        /// <summary>
        /// Gets or sets a value indicating whether vgrain execution tracing MUST be disable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if vgrain execution tracing; otherwise <c>false</c> enable tracing.
        /// </value>
        public bool DisableVGrainExecutionTracing { get; }

        /// <summary>
        /// Gets a value indicating whether we must trace orlean system vgrain.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we must trace orlean system vgrain; otherwise <c>false</c>.
        /// </value>
        public bool TraceOrleanSystemVGrain { get; }

        /// <summary>
        /// Gets a value indicating whether we must trace democrite system vgrain.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we must trace democrite system vgrain; otherwise, <c>false</c>.
        /// </value>
        public bool TraceDemocriteSystemVGrain { get; }

        /// <summary>
        /// Gets or sets a value indicating whether vgrain input and output tracing MUST be disable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if vgrain intput and output tracing must be disabled; otherwise <c>false</c> enable tracing.
        /// </value>
        public bool DisableVGrainInOutTracing { get; }

        #endregion
    }
}
