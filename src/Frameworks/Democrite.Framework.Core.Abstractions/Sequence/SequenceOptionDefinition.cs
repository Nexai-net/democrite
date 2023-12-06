// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Democrite.Framework.Toolbox;

    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Define all the sequence options
    /// </summary>
    public sealed class SequenceOptionDefinition : Equatable<SequenceOptionDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SequenceOptionDefinition"/> class.
        /// </summary>
        static SequenceOptionDefinition()
        {
            Default = new SequenceOptionDefinition(SequenceDiagnosticOptionDefinition.Default);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOptionDefinition"/> class.
        /// </summary>
        public SequenceOptionDefinition([AllowNull] SequenceDiagnosticOptionDefinition diagnostic)
        {
            this.Diagnostic = diagnostic ?? SequenceDiagnosticOptionDefinition.Default;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default option.
        /// </summary>
        public static SequenceOptionDefinition Default { get; }

        /// <summary>
        /// Gets the diagnostic options
        /// </summary>
        [NotNull]
        public SequenceDiagnosticOptionDefinition Diagnostic { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected sealed override bool OnEquals([NotNull] SequenceOptionDefinition other)
        {
            return other.Diagnostic.Equals(this.Diagnostic);
        }

        /// <inheritdoc />
        protected sealed override int OnGetHashCode()
        {
            return this.Diagnostic.GetHashCode();
        }

        #endregion
    }
}
