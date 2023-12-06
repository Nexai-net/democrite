// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Democrite.Framework.Toolbox;

    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Sequence option realted to diagnostic
    /// </summary>
    public sealed class SequenceDiagnosticOptionDefinition : Equatable<SequenceDiagnosticOptionDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SequenceDiagnosticOptionDefinition"/> class.
        /// </summary>
        static SequenceDiagnosticOptionDefinition()
        {
            Default = new SequenceDiagnosticOptionDefinition(Debugger.IsAttached);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDiagnosticOptionDefinition"/> class.
        /// </summary>
        public SequenceDiagnosticOptionDefinition(bool saveAllStageInfo)
        {
            this.SaveAllStageInfo = saveAllStageInfo;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static SequenceDiagnosticOptionDefinition Default { get; }

        /// <summary>
        /// Gets or sets a value indicating whether save all stage information will be save for diagnostic purpose.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [save all stage information]; otherwise, <c>false</c>.
        /// </value>
        public bool SaveAllStageInfo { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] SequenceDiagnosticOptionDefinition other)
        {
            return this.SaveAllStageInfo == other.SaveAllStageInfo;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.SaveAllStageInfo.GetHashCode();
        }

        #endregion
    }
}
