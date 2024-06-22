// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;

    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define information needed to setup, run and diagnostic a stage
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]

    [KnownType(typeof(SequenceStageCallDefinition))]
    [KnownType(typeof(SequenceStageFilterDefinition))]
    [KnownType(typeof(SequenceStageForeachDefinition))]
    [KnownType(typeof(SequenceStagePushToContextDefinition))]
    [KnownType(typeof(SequenceStageFireSignalDefinition))]

    public abstract class SequenceStageDefinition : IEquatable<SequenceStageDefinition>,
                                                    ISupportDebugDisplayName,
                                                    IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ISequenceStageDefinition"/> class.
        /// </summary>
        protected SequenceStageDefinition(Guid uid,
                                          string displayName,
                                          StageTypeEnum type,
                                          AbstractType? input,
                                          AbstractType? output,
                                          DefinitionMetaData? metaData,
                                          bool preventReturn = false)
        {
            this.Uid = uid;
            if (this.Uid == Guid.Empty)
                this.Uid = Guid.NewGuid();

            this.DisplayName = displayName;
            this.MetaData = metaData;

            this.Type = type;
            this.PreventReturn = preventReturn;

            this.Input = input;
            if (!preventReturn)
                this.Output = output;
        }

        #endregion

        #region Properties     

        /// <summary>
        /// Gets stage unique id.
        /// </summary>
        [DataMember]
        [Id(0)]
        public Guid Uid { get; }

        /// <summary>
        /// Gets a value indicating whether any return is prevent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prevent type return]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [Id(1)]
        public bool PreventReturn { get; }

        /// <summary>
        /// Gets the input first stage input.
        /// </summary>
        [DataMember]
        [Id(2)]
        public AbstractType? Input { get; }

        /// <summary>
        /// Gets the input first stage ouytput.
        /// </summary>
        [DataMember]
        [Id(3)]
        public AbstractType? Output { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(4)]
        public string DisplayName { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(5)]
        public DefinitionMetaData? MetaData { get; }

        /// <summary>
        /// Gets the stage type.
        /// </summary>
        [DataMember]
        [Id(6)]
        public StageTypeEnum Type { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(SequenceStageDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.Type == other.Type &&
                   this.Input == other.Input &&
                   this.Output == other.Output &&
                   this.PreventReturn == other.PreventReturn &&
                   this.MetaData == other.MetaData &&
                   this.DisplayName == other.DisplayName &&
                   OnStageEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is SequenceStageDefinition stage)
                return Equals(stage);
            return false;   
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.Type,
                                    this.Input,
                                    this.Output,
                                    this.PreventReturn,
                                    this.MetaData,
                                    this.DisplayName,
                                    OnStageGetHashCode());
        }

        /// <summary>
        /// Called to get children hash code
        /// </summary>
        protected abstract int OnStageGetHashCode();

        /// <summary>
        /// Called when to check quality
        /// </summary>
        protected abstract bool OnStageEquals(SequenceStageDefinition other);

        /// <inheritdoc />
        public virtual string ToDebugDisplayName()
        {
            return ToString() ?? typeof(SequenceStageDefinition).Name;
        }

        /// <inheritdoc />
        public virtual bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            // TODO : Sequence stage validation
            return true;
        }

        #endregion
    }
}
