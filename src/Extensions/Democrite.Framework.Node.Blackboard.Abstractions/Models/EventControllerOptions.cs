// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ControllerBaseOptions" />
    /// <seealso cref="IControllerEventOptions" />
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    public class EventControllerOptions : ControllerBaseOptions, IControllerEventOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventControllerOptions"/> class.
        /// </summary>
        internal protected EventControllerOptions(IEnumerable<SignalId>? listenSignals, IEnumerable<DoorId>? listenDoors)
        {
            this.ListenSignals = listenSignals?.ToArray() ?? EnumerableHelper<SignalId>.ReadOnlyArray;
            this.ListenDoors = listenDoors?.ToArray() ?? EnumerableHelper<DoorId>.ReadOnlyArray;
        }

        #endregion

        #region Propertie

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public IReadOnlyCollection<SignalId> ListenSignals { get; }

        /// <inheritdoc />
        [Id(1)]
        [DataMember]
        public IReadOnlyCollection<DoorId> ListenDoors { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified builder.
        /// </summary>
        public static EventControllerOptions Create(Action<IEventControllerOptionsBuilder> builderFunc)
        {
            var builder = new EventControllerOptionsBuilder();
            builderFunc(builder);
            return builder.Build();
        }

        /// <inheritdoc />
        protected sealed override bool OnEquals([NotNull] ControllerBaseOptions other)
        {
            return other is EventControllerOptions evt &&
                   this.ListenSignals.SequenceEqual(evt.ListenSignals) &&
                   this.ListenDoors.SequenceEqual(evt.ListenDoors) &&
                   OnEventControllerEquals(evt);
        }

        /// <inheritdoc />
        protected sealed override int OnGetHashCode()
        {
            return this.ListenDoors.Aggregate(0, (acc, d) => acc ^ d.GetHashCode()) ^
                   this.ListenSignals.Aggregate(0, (acc, d) => acc ^ d.GetHashCode()) ^
                   OnEventControllerHashCode();
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        protected virtual bool OnEventControllerEquals(EventControllerOptions evt)
        {
            return true;
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected virtual int OnEventControllerHashCode()
        {
            return 0;
        }

        #endregion
    }
}
