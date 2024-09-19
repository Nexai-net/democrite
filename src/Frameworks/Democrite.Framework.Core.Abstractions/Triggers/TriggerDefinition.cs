// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base definition of trigger definition
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]

    [KnownType(typeof(CronTriggerDefinition))]
    [KnownType(typeof(SignalTriggerDefinition))]
    [KnownType(typeof(StreamTriggerDefinition))]
    
    public abstract class TriggerDefinition : IEquatable<TriggerDefinition>, IDefinition, IRefDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinition"/> class.
        /// </summary>
        protected TriggerDefinition(Guid uid,
                                    Uri refId,
                                    string displayName,        
                                    TriggerTypeEnum triggerType,
                                    IEnumerable<TriggerTargetDefinition> targets,
                                    bool enabled,
                                    DefinitionMetaData? metaData,
                                    DataSourceDefinition? triggerGlobalOutputDefinition = null)
        {
            this.Uid = uid;
            this.MetaData = metaData;
            this.DisplayName = displayName;
            this.Enabled = enabled;
            this.TriggerType = triggerType;
            this.Targets = targets?.ToArray() ?? EnumerableHelper<TriggerTargetDefinition>.ReadOnlyArray;
            this.TriggerGlobalOutputDefinition = triggerGlobalOutputDefinition;
            this.RefId = refId;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public Guid Uid { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        [Id(1)]
        [DataMember]
        public string DisplayName { get; }

        /// <inheritdoc />
        [Id(2)]
        [DataMember]
        public bool Enabled { get; }

        /// <inheritdoc />
        [Id(3)]
        [DataMember]
        public TriggerTypeEnum TriggerType { get; }

        /// <inheritdoc />
        [Id(4)]
        [DataMember]
        public IReadOnlyCollection<TriggerTargetDefinition> Targets { get; }

        /// <inheritdoc />
        [Id(5)]
        [DataMember]
        public DataSourceDefinition? TriggerGlobalOutputDefinition { get; }

        /// <inheritdoc />
        [Id(6)]
        [DataMember]
        public DefinitionMetaData? MetaData { get; }

        /// <inheritdoc />
        [Id(7)]
        [DataMember]
        public Uri RefId { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(TriggerDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.Uid == other.Uid &&
                   this.Enabled == other.Enabled &&
                   this.TriggerType == other.TriggerType &&
                   this.MetaData == other.MetaData &&
                   (this.TriggerGlobalOutputDefinition?.Equals(other.TriggerGlobalOutputDefinition) ?? other.TriggerGlobalOutputDefinition is null) &&
                   this.Targets.SequenceEqual(other.Targets);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is TriggerDefinition trigger)
                return Equals(trigger);

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.TriggerType,
                                    this.MetaData,
                                    this.TriggerGlobalOutputDefinition);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return "[Id: " + this.Uid + "][Trigger: " + this.TriggerType + "]" + OnDebugDisplayName();
        }

        /// <summary>
        /// Called when [debug display name].
        /// </summary>
        protected virtual string OnDebugDisplayName()
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            bool valid = true;

            if (this.Targets is null || this.Targets.Count == 0)
            {
                logger.OptiLog(LogLevel.Error, "At least one target is required");
                valid = false;
            }

            valid &= RefIdHelper.ValidateRefId(this.RefId, logger);
            valid &= OnValidate(logger, matchWarningAsError);
            return valid;
        }

        protected abstract bool OnValidate(ILogger logger, bool matchWarningAsError = false);

        #endregion
    }
}
