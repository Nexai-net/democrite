// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Define all the sequence information
    /// </summary>

    [Immutable]
    [Serializable]
    public sealed class SequenceDefinition : Equatable<SequenceDefinition>
    {
        #region Fields

        private readonly IReadOnlyDictionary<Guid, ISequenceStageDefinition> _indexedStages;

        // Index current Stahe Id to next stage id
        private readonly IReadOnlyDictionary<Guid, Guid?> _indexedNextStages;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinition"/> class.
        /// </summary>
        public SequenceDefinition(Guid uid,
                                  string? displayName,
                                  SequenceOptionDefinition options,
                                  IEnumerable<ISequenceStageDefinition> stages)
        {
            this.Uid = uid;
            this.DisplayName = displayName ?? uid.ToString();
            this.Options = options ?? SequenceOptionDefinition.Default;

            var arrayStages = stages?.ToArray() ?? EnumerableHelper<ISequenceStageDefinition>.ReadOnlyArray;
            this.Stages = arrayStages;

            this._indexedStages = this.Stages.ToDictionary(k => k.Uid);

            var indexedNextStages = new Dictionary<Guid, Guid?>();
            for (int indx = 0; indx < this.Stages.Count; indx++)
            {
                var current = arrayStages[indx];
                var next = indx + 1 < this.Stages.Count ? arrayStages[indx + 1] : (ISequenceStageDefinition?)null;

                indexedNextStages.Add(current.Uid, next?.Uid);
            }

            this._indexedNextStages = indexedNextStages;

            var inputStage = stages?.FirstOrDefault();
            var outputStage = stages?.LastOrDefault();

            this.Input = inputStage?.Input;
            this.Output = outputStage?.Output;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the uid.
        /// </summary>
        public Guid Uid { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the sequence options.
        /// </summary>
        [NotNull]
        public SequenceOptionDefinition Options { get; }

        /// <summary>
        /// Gets sequence's stages.
        /// </summary>
        public IReadOnlyCollection<ISequenceStageDefinition> Stages { get; }

        /// <summary>
        /// Gets the sequence input.
        /// </summary>
        public Type? Input { get; }

        /// <summary>
        /// Gets the sequence output.
        /// </summary>
        public Type? Output { get; }

        /// <summary>
        /// Gets the <see cref="Nullable{SequenceStageDefinition}"/> with the specified stage identifier.
        /// </summary>
        public ISequenceStageDefinition? this[Guid? stageId]
        {
            get
            {
                if (stageId == null)
                    return this.Stages.FirstOrDefault();

                if (this._indexedStages.TryGetValue(stageId.Value, out var stage))
                    return stage;

                return null;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] SequenceDefinition other)
        {
            return this.Uid == other.Uid &&
                   string.Equals(this.DisplayName, other.DisplayName) &&
                   this.Options.Equals(other.Options) &&
                   this.Stages.SequenceEqual(other.Stages);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Uid.GetHashCode() ^
                   (this.DisplayName?.GetHashCode() ?? 0) ^
                   this.Options.GetHashCode() ^
                   this.Stages.Aggregate(0, (acc, stg) => acc ^ stg.GetHashCode());
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(this.DisplayName);
            builder.Append(" - ");
            builder.Append(this.Uid);
            builder.Append(" : (");
            builder.Append(this.Input);
            builder.Append(") -> ");
            builder.AppendLine();

            // Trigger
            foreach (var stage in this.Stages)
                builder.AppendLine(stage.ToString());

            builder.Append("(");
            builder.Append(this.Output);
            builder.Append(") <- ");

            return builder.ToString();
        }

        /// <summary>
        /// Gets the next stage Uid.
        /// </summary>
        public Guid? GetNextStage(Guid? currentStageId)
        {
            if (currentStageId == null || currentStageId == Guid.Empty)
                return null;

            if (this._indexedNextStages.TryGetValue(currentStageId.Value, out var nextStageId))
                return nextStageId;

            return null;
        }

        #endregion
    }
}
