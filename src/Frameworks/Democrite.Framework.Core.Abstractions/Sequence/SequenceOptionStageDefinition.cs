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
    /// Define all the stage options
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class SequenceOptionStageDefinition : Equatable<SequenceOptionStageDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SequenceOptionStageDefinition"/> class.
        /// </summary>
        static SequenceOptionStageDefinition()
        {
            Default = new SequenceOptionStageDefinition(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOptionStageDefinition"/> class.
        /// </summary>
        public SequenceOptionStageDefinition(Guid? stageId)
        {
            this.StageId = stageId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static SequenceOptionStageDefinition Default { get; }

        /// <summary>
        /// Gets the desired stage identifier.
        /// </summary>
        [DataMember]
        public Guid? StageId { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected sealed override bool OnEquals([NotNull] SequenceOptionStageDefinition other)
        {
            return this.StageId == other.StageId;
        }

        /// <inheritdoc />
        protected sealed override int OnGetHashCode()
        {
            return this.StageId?.GetHashCode() ?? 0;
        }

        #endregion
    }
}
