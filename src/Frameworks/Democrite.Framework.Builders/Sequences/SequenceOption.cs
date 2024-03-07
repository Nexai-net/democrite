// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Sequence option builder
    /// </summary>
    public sealed class SequenceOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOption"/> class.
        /// </summary>
        public SequenceOption([AllowNull] Guid? uid = null, SequenceOptionDefinition? savedOptions = null)
        {
            this.Uid = uid ?? Guid.NewGuid();
            this.Diagnostic = new SequenceOptionDiagnostic(savedOptions?.Diagnostic ?? SequenceDiagnosticOptionDefinition.Default);
        }

        /// <summary>
        /// Gets or sets the uniqie sequence.
        /// </summary>
        [NotNull]
        public Guid Uid { get; private set; }

        /// <summary>
        /// Gets the diagnostic.
        /// </summary>
        public SequenceOptionDiagnostic Diagnostic { get; }

        /// <summary>
        /// Froms the definition.
        /// </summary>
        public void FromDefinition(SequenceOptionDefinition option)
        {
            this.Diagnostic.FromDefintion(option.Diagnostic);
        }

        /// <summary>
        /// Converts to definition.
        /// </summary>
        public SequenceOptionDefinition ToDefinition()
        {
            return new SequenceOptionDefinition(this.Diagnostic.ToDefinition());
        }
    }
}
