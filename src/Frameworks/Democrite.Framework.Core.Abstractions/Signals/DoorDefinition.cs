// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SignalNetworkBasePartDefinition" />
    [Immutable]
    [ImmutableObject(true)]
    [Serializable]
    [GenerateSerializer]
    public abstract class DoorDefinition : SignalNetworkBasePartDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorDefinition"/> class.
        /// </summary>
        public DoorDefinition(Guid uid,
                                string name,
                                string? group,
                                string vgrainInterfaceFullName,
                                IEnumerable<SignalId>? signalSourceIds,
                                IEnumerable<DoorId>? doorSourceIds,
                                TimeSpan interval)
            : base(uid, name, group)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(name);

            this.DoorId = new DoorId(uid, name);

            this.VGrainInterfaceFullName = vgrainInterfaceFullName;

            this.SignalSourceIds = signalSourceIds?.ToArray() ?? EnumerableHelper<SignalId>.ReadOnlyArray;
            this.DoorSourceIds = doorSourceIds?.ToArray() ?? EnumerableHelper<DoorId>.ReadOnlyArray;
            this.Interval = interval;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the door identifier.
        /// </summary>
        [Id(0)]
        public DoorId DoorId { get; }

        /// <summary>
        /// Gets the signal source ids.
        /// </summary>
        [Id(1)]
        public IReadOnlyCollection<SignalId> SignalSourceIds { get; }

        /// <summary>
        /// Gets the door source ids.
        /// </summary>
        [Id(2)]
        public IReadOnlyCollection<DoorId> DoorSourceIds { get; }

        /// <summary>
        /// Gets the interval when the condition are to be valid to fire the door signal.
        /// </summary>
        [Id(3)]
        public TimeSpan Interval { get; }

        /// <summary>
        /// Gets the full name of the vgrain interface.
        /// </summary>
        /// <remarks>
        ///     MUST be resolvable by Type.Create
        /// </remarks>
        [Id(4)]
        public string VGrainInterfaceFullName { get; }

        #endregion
    }
}
