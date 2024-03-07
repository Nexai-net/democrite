// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using Microsoft.Extensions.Logging;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base definition of trigger definition
    /// </summary>
    [Serializable]
    [Immutable]
    [ImmutableObject(true)]
    [DataContract]

    [KnownType(typeof(CronTriggerDefinition))]
    [KnownType(typeof(SignalTriggerDefinition))]
    [KnownType(typeof(StreamTriggerDefinition))]
    
    public abstract class TriggerDefinition : IEquatable<TriggerDefinition>, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinition"/> class.
        /// </summary>
        protected TriggerDefinition(Guid uid,
                                    string displayName,        
                                    TriggerTypeEnum triggerType,
                                    IEnumerable<TriggerTargetDefinition> targets,
                                    bool enabled,
                                    DataSourceDefinition? triggerGlobalOutputDefinition = null)
        {
            this.Uid = uid;
            this.DisplayName = displayName;
            this.Enabled = enabled;
            this.TriggerType = triggerType;
            this.Targets = targets?.ToArray() ?? EnumerableHelper<TriggerTargetDefinition>.ReadOnlyArray;
            this.TriggerGlobalOutputDefinition = triggerGlobalOutputDefinition;
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
        public bool Equals(TriggerDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.Uid == other.Uid &&
                   this.Enabled == other.Enabled &&
                   this.TriggerType == other.TriggerType &&
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
                                    this.TriggerGlobalOutputDefinition);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return "[Id: " + this.Uid + "][Trigger: " + this.TriggerType + "]";
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
