// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Helpers;

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

    public abstract class TriggerDefinition : IEquatable<TriggerDefinition>, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinition"/> class.
        /// </summary>
        protected TriggerDefinition(Guid uid,
                                    TriggerTypeEnum triggerType,
                                    IEnumerable<Guid> targetSequenceIds,
                                    IEnumerable<SignalId> targetSignalIds,
                                    bool enabled,
                                    InputSourceDefinition? inputSourceDefinition = null)
        {
            this.Uid = uid;
            this.Enabled = enabled;
            this.TriggerType = triggerType;
            this.TargetSequenceIds = targetSequenceIds?.ToArray() ?? EnumerableHelper<Guid>.ReadOnlyArray;
            this.TargetSignalIds = targetSignalIds?.ToArray() ?? EnumerableHelper<SignalId>.ReadOnlyArray;
            this.InputSourceDefinition = inputSourceDefinition;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public Guid Uid { get; }

        /// <inheritdoc />
        [Id(1)]
        [DataMember]
        public bool Enabled { get; }

        /// <inheritdoc />
        [Id(2)]
        [DataMember]
        public TriggerTypeEnum TriggerType { get; }

        /// <inheritdoc />
        [Id(3)]
        [DataMember]
        public IReadOnlyCollection<Guid> TargetSequenceIds { get; }

        /// <inheritdoc />
        [Id(4)]
        [DataMember]
        public IReadOnlyCollection<SignalId> TargetSignalIds { get; }

        /// <inheritdoc />
        [Id(5)]
        [DataMember]
        public InputSourceDefinition? InputSourceDefinition { get; }

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
                   (this.InputSourceDefinition?.Equals(other.InputSourceDefinition) ?? other.InputSourceDefinition is null) &&
                   this.TargetSignalIds.SequenceEqual(other.TargetSignalIds) &&
                   this.TargetSequenceIds.SequenceEqual(this.TargetSequenceIds);
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
                                    this.InputSourceDefinition);
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
