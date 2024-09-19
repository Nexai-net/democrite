// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Define all the sequence information
    /// </summary>

    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceDefinition : IEquatable<SequenceDefinition>, IDefinition, IRefDefinition
    {
        #region Fields

        private readonly IReadOnlyDictionary<Guid, SequenceStageDefinition> _indexedStages;

        // Index current Stage Id to next stage id
        private readonly IReadOnlyDictionary<Guid, Guid?> _indexedNextStages;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public SequenceDefinition(Guid uid,
                                  Uri refId,
                                  string? displayName,
                                  SequenceOptionDefinition options,
                                  IEnumerable<SequenceStageDefinition> stages,
                                  DefinitionMetaData? metadata)
        {
            this.Uid = uid;
            this.DisplayName = displayName ?? uid.ToString();
            this.Options = options ?? SequenceOptionDefinition.Default;
            this.MetaData = metadata;

            var arrayStages = stages?.ToArray() ?? EnumerableHelper<SequenceStageDefinition>.ReadOnlyArray;
            this.Stages = arrayStages;

            this._indexedStages = this.Stages.ToDictionary(k => k.Uid);

            var indexedNextStages = new Dictionary<Guid, Guid?>();
            for (int indx = 0; indx < this.Stages.Count; indx++)
            {
                var current = arrayStages[indx];
                var next = indx + 1 < this.Stages.Count ? arrayStages[indx + 1] : (SequenceStageDefinition?)null;

                indexedNextStages.Add(current.Uid, next?.Uid);
            }

            this._indexedNextStages = indexedNextStages;

            var inputStage = stages?.FirstOrDefault();
            var outputStage = stages?.LastOrDefault();

            this.Input = inputStage?.Input;
            this.Output = outputStage?.Output;
            this.RefId = refId;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(1)]
        [DataMember]
        public Guid Uid { get; }

        /// <inheritdoc />
        [Id(2)]
        [DataMember]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the sequence options.
        /// </summary>
        [NotNull]
        [Id(3)]
        [DataMember]
        public SequenceOptionDefinition Options { get; }

        /// <summary>
        /// Gets sequence's stages.
        /// </summary>
        [Id(4)]
        [DataMember]
        public IReadOnlyCollection<SequenceStageDefinition> Stages { get; }

        /// <summary>
        /// Gets the sequence input.
        /// </summary>
        [Id(5)]
        [DataMember]
        public AbstractType? Input { get; }

        /// <summary>
        /// Gets the sequence output.
        /// </summary>
        [Id(6)]
        [DataMember]
        public AbstractType? Output { get; }

        /// <inheritdoc />
        [Id(7)]
        [DataMember]
        public DefinitionMetaData? MetaData { get; }

        /// <summary>
        /// Gets the <see cref="Nullable{SequenceStageDefinition}"/> with the specified stage identifier.
        /// </summary>
        [IgnoreDataMember]
        public SequenceStageDefinition? this[Guid? stageId]
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

        /// <inheritdoc />
        [Id(8)]
        [DataMember]
        public Uri RefId { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(SequenceDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.Uid == other.Uid &&
                   string.Equals(this.DisplayName, other.DisplayName) &&
                   this.Options.Equals(other.Options) &&
                   this.MetaData == other.MetaData &&
                   this.Stages.SequenceEqual(other.Stages);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is SequenceDefinition sequence)
                return Equals(sequence);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.DisplayName,
                                    this.Options,
                                    this.MetaData,
                                    this.Stages.Aggregate(0, (acc, stg) => acc ^ stg.GetHashCode()));
        }

        /// <summary>
        /// Gets the next stage DeferredId.
        /// </summary>
        public Guid? GetNextStage(Guid? currentStageId)
        {
            if (currentStageId == null || currentStageId == Guid.Empty)
                return null;

            if (this._indexedNextStages.TryGetValue(currentStageId.Value, out var nextStageId))
                return nextStageId;

            return null;
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            bool valid = true;

            var input = this.Input;
            var index = 0;
            foreach (var stage in this.Stages)
            {
                if (stage.Input?.Equals(input) == false)
                {
                    logger.OptiLog(LogLevel.Error, "Stages input -> output chain doesn't match at stage {stage} index {index}", stage, index);
                    valid = false;
                    break;
                }
                index++;
                input = stage.Output;
            }

            foreach (var stage in this.Stages)
                valid &= stage.Validate(logger, matchWarningAsError);

            return valid;
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            var builder = new StringBuilder();

            builder.Append(this.DisplayName);
            builder.Append(" - ");
            builder.Append(this.Uid);
            builder.Append(" : (");
            builder.Append(this.Input);
            builder.Append(") -> ");

            if (this.MetaData is not null)
            {
                builder.AppendLine();
                builder.Append(' ', 4);
                builder.Append(nameof(this.MetaData.CategoryPath));
                builder.Append(": ");
                builder.AppendLine(this.MetaData.CategoryPath);

                builder.Append(' ', 4);
                builder.Append(nameof(this.MetaData.Description));
                builder.Append(": ");
                builder.AppendLine(this.MetaData.Description);

                builder.Append(' ', 4);
                builder.Append(nameof(this.MetaData.Tags));
                builder.Append(": ");

                bool first = true;
                foreach (var tag in this.MetaData.Tags ?? EnumerableHelper<string>.ReadOnly)
                {
                    if (!first)
                        builder.Append(", ");
                    builder.Append(tag);
                    first = false;
                }

                builder.AppendLine();
            }

            builder.AppendLine();

            // Trigger
            foreach (var stage in this.Stages)
                builder.AppendLine(stage.ToString());

            builder.Append("(");
            builder.Append(this.Output);
            builder.Append(") <- ");

            return builder.ToString();
        }

        /// <inheritdoc />
        public static bool operator ==(SequenceDefinition? lhs, SequenceDefinition? rhs)
        {
            return lhs?.Equals(rhs) ?? lhs is null;
        }

        /// <inheritdoc />
        public static bool operator !=(SequenceDefinition? lhs, SequenceDefinition? rhs)
        {
            return !(lhs == rhs);
        }

        #endregion
    }
}
