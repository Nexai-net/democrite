// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define all the sequence options
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceOptionDefinition : IEquatable<SequenceOptionDefinition>
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
        [IgnoreDataMember]
        public static SequenceOptionDefinition Default { get; }

        /// <summary>
        /// Gets the diagnostic options
        /// </summary>
        [NotNull]
        [Id(0)]
        [DataMember]
        public SequenceDiagnosticOptionDefinition Diagnostic { get; }

        /// <summary>
        /// Gets a value indicating whether [prevent state storage].
        /// </summary>
        [Id(1)]
        [DataMember]
        public bool PreventSequenceExecutorStateStorage { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(SequenceOptionDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return other.Diagnostic.Equals(this.Diagnostic) &&
                   this.PreventSequenceExecutorStateStorage == other.PreventSequenceExecutorStateStorage;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is SequenceOptionDefinition option)
                return Equals(option);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Diagnostic, this.PreventSequenceExecutorStateStorage);
        }

        #endregion
    }
}
