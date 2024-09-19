// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Doors
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SignalNetworkBasePartDefinition" />
    [Immutable]
    [ImmutableObject(true)]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    public abstract class DoorDefinition : SignalNetworkBasePartDefinition
    {
        #region Fields

        public static readonly TimeSpan DEFAULT_RETENTION_MAX_DELAY = TimeSpan.FromDays(1);
        public static readonly uint DEFAULT_HISTORY_RETENTION = 0;
        public static readonly uint? DEFAULT_NOT_CONSUMED_RETENTION = null;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public DoorDefinition(Guid uid,
                              Uri refId,
                              string name,
                              string vgrainInterfaceFullName,
                              IEnumerable<SignalId>? signalSourceIds,
                              IEnumerable<DoorId>? doorSourceIds,
                              DefinitionMetaData? metaData,
                              TimeSpan? activeWindowInterval = null,
                              TimeSpan? retentionMaxDelay = null,
                              uint? historyMaxRetention = null,
                              uint? notConsumedMaxRetiention = null)
            : base(uid, refId, name, name, metaData)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            this.DoorId = new DoorId(uid, name);

            this.VGrainInterfaceFullName = vgrainInterfaceFullName;

            this.SignalSourceIds = signalSourceIds?.ToArray() ?? EnumerableHelper<SignalId>.ReadOnlyArray;
            this.DoorSourceIds = doorSourceIds?.ToArray() ?? EnumerableHelper<DoorId>.ReadOnlyArray;
            this.ActiveWindowInterval = activeWindowInterval;

            this.RetentionMaxDelay = retentionMaxDelay;
            this.HistoryMaxRetention = historyMaxRetention;
            this.NotConsumedMaxRetiention = notConsumedMaxRetiention;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the door identifier.
        /// </summary>
        [Id(0)]
        [IgnoreDataMember]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DoorId DoorId { get; }

        /// <summary>
        /// Gets the signal source ids.
        /// </summary>
        [Id(1)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public IReadOnlyCollection<SignalId> SignalSourceIds { get; }

        /// <summary>
        /// Gets the door source ids.
        /// </summary>
        [Id(2)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public IReadOnlyCollection<DoorId> DoorSourceIds { get; }

        /// <summary>
        /// Gets the activeWindowInterval when the condition are to be valid to fire the door signal.
        /// </summary>
        [Id(3)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public TimeSpan? ActiveWindowInterval { get; }

        /// <summary>
        /// Gets the full name of the vgrain interface.
        /// </summary>
        /// <remarks>
        ///     MUST be resolvable by ArtifactType.Get
        /// </remarks>
        [Id(4)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public string VGrainInterfaceFullName { get; }

        /// <summary>
        /// Gets quantity of message not consumed to retain; null means no limit.
        /// </summary>
        /// <remarks>
        ///     Default null <see cref="DEFAULT_NOT_CONSUMED_RETENTION"/>, Min 1
        /// </remarks>
        [Id(5)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public uint? NotConsumedMaxRetiention { get; }

        /// <summary>
        /// Gets quantity of history signal to retain; null means no limit; 0 prevent any history
        /// </summary>
        /// <remarks>
        ///     Default 0 <see cref="DEFAULT_HISTORY_RETENTION"/>
        /// </remarks>
        [Id(6)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public uint? HistoryMaxRetention { get; }

        /// <summary>
        /// Gets the retention maximum delay.
        /// </summary>
        /// <remarks>
        ///     Default 1 day <see cref="DEFAULT_RETENTION_MAX_DELAY"/>
        /// </remarks>
        [Id(7)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public TimeSpan? RetentionMaxDelay { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Validates this current instance of <see cref="DoorDefinition"/>
        /// </summary>
        protected sealed override bool OnValidate(ILogger logger, bool matchErrorAsWarning = false)
        {
            var isValid = true;
            var warningLevel = LogLevel.Warning;

            if (matchErrorAsWarning)
                warningLevel = LogLevel.Error;

            if (this.DoorId.Uid == Guid.Empty)
            {
                logger.OptiLog(LogLevel.Critical, "Door id MUST not be equals to Guid.Empty");
                isValid = false;
            }

            if (string.IsNullOrEmpty(this.VGrainInterfaceFullName))
            {
                logger.OptiLog(LogLevel.Critical, "VGrainInterfaceFullName MUST not be empty; it would result on a critical error at runtime.");
                isValid = false;
            }

            if (this.SignalSourceIds.Count == 0 && this.DoorSourceIds.Count == 0)
            {
                logger.OptiLog(warningLevel, "No signal or door are listen");

                if (matchErrorAsWarning)
                    isValid = false;
            }

            if (this.SignalSourceIds.Count == 0 && this.DoorSourceIds.Count == 0)
            {
                logger.OptiLog(warningLevel, "No signal or door are listen");

                if (matchErrorAsWarning)
                    isValid = false;
            }

            if (this.HistoryMaxRetention == null && this.RetentionMaxDelay == null)
            {
                logger.OptiLog(warningLevel, "No historical retention have been set, it will result of no automatic clean up and historical grow indefinetly");

                if (matchErrorAsWarning)
                    isValid = false;
            }

            if (this.NotConsumedMaxRetiention == null && this.RetentionMaxDelay == null)
            {
                logger.OptiLog(warningLevel, "No 'not consumed' retention have been set, it will result of no automatic clean up and historical grow indefinetly");

                if (matchErrorAsWarning)
                    isValid = false;
            }

            return isValid && OnDoorChildValidate(logger, matchErrorAsWarning);
        }

        /// <summary>
        /// Called to validate the current definition
        /// </summary>
        protected abstract bool OnDoorChildValidate(ILogger logger, bool matchErrorAsWarning);

        /// <inheritdoc />
        protected sealed override bool OnSignalEquals([NotNull] SignalNetworkBasePartDefinition other)
        {
            return other is DoorDefinition otherDoor &&
                   this.DisplayName == otherDoor.DisplayName &&
                   this.VGrainInterfaceFullName == otherDoor.VGrainInterfaceFullName &&
                   this.NotConsumedMaxRetiention == otherDoor.NotConsumedMaxRetiention &&
                   this.ActiveWindowInterval == otherDoor.ActiveWindowInterval &&
                   this.RetentionMaxDelay == otherDoor.RetentionMaxDelay &&
                   this.HistoryMaxRetention == otherDoor.HistoryMaxRetention &&
                   this.SignalSourceIds.SequenceEqual(otherDoor.SignalSourceIds) &&
                   this.DoorSourceIds.SequenceEqual(otherDoor.DoorSourceIds) &&
                   OnDoorEquals(otherDoor);
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        protected abstract bool OnDoorEquals(DoorDefinition otherDoor);

        /// <inheritdoc />
        protected sealed override int OnSignalGetHashCode()
        {
            return HashCode.Combine(this.SignalSourceIds.Aggregate(0, (acc, s) => s.GetHashCode() ^ acc) ^
                                    this.DoorSourceIds.Aggregate(0, (acc, s) => s.GetHashCode() ^ acc),

                                    this.ActiveWindowInterval,
                                    this.DisplayName,
                                    this.VGrainInterfaceFullName,

                                    HashCode.Combine(this.RetentionMaxDelay,
                                                     this.HistoryMaxRetention,
                                                     this.NotConsumedMaxRetiention),

                                    OnDoorGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract int OnDoorGetHashCode();

        #endregion
    }
}
