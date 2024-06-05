// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Elvex.Toolbox;

    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define all the sequence options
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class SequenceOptionDefinition : Equatable<SequenceOptionDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SequenceOptionDefinition"/> class.
        /// </summary>
        static SequenceOptionDefinition()
        {
            Default = new SequenceOptionDefinition(SequenceDiagnosticOptionDefinition.Default, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOptionDefinition"/> class.
        /// </summary>
        public SequenceOptionDefinition([AllowNull] SequenceDiagnosticOptionDefinition diagnostic, bool preventSequenceExecutorStateStorage)
        {
            this.Diagnostic = diagnostic ?? SequenceDiagnosticOptionDefinition.Default;
            this.PreventSequenceExecutorStateStorage = preventSequenceExecutorStateStorage;
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
        [DataMember]
        public SequenceDiagnosticOptionDefinition Diagnostic { get; }

        /// <summary>
        /// Gets a value indicating whether [prevent state storage].
        /// </summary>
        [DataMember]
        public bool PreventSequenceExecutorStateStorage { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected sealed override bool OnEquals([NotNull] SequenceOptionDefinition other)
        {
            return other.Diagnostic.Equals(this.Diagnostic) &&
                   this.PreventSequenceExecutorStateStorage == other.PreventSequenceExecutorStateStorage;
        }

        /// <inheritdoc />
        protected sealed override int OnGetHashCode()
        {
            return HashCode.Combine(this.Diagnostic, this.PreventSequenceExecutorStateStorage);
        }

        #endregion
    }
}
