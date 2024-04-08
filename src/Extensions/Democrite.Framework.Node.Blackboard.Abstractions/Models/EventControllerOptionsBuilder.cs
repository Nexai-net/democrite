// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EventControllerOptionsBuilder<TBuilder> : IEventControllerOptionsBuilder<TBuilder>
        where TBuilder : IEventControllerOptionsBuilder<TBuilder>
    {
        #region Fields
        
        private readonly HashSet<SignalId> _listenSignals;
        private readonly HashSet<DoorId> _listenDoors;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventControllerOptionsBuilder{TBuilder}"/> class.
        /// </summary>
        public EventControllerOptionsBuilder()
        {
            this._listenDoors = new HashSet<DoorId>();
            this._listenSignals = new HashSet<SignalId>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the listen signals.
        /// </summary>
        protected IReadOnlyCollection<SignalId> Signals
        {
            get { return this._listenSignals; }
        }

        /// <summary>
        /// Gets the listen signals.
        /// </summary>
        protected IReadOnlyCollection<DoorId> Doors
        {
            get { return this._listenDoors; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual EventControllerOptions Build()
        {
            return new EventControllerOptions(this._listenSignals, this._listenDoors);  
        }

        /// <inheritdoc />
        public virtual TBuilder ListenDoors(params Guid[] doorIds)
        {
            return ListenDoors(doorIds.Select(d => new DoorId(d, string.Empty)).ToArray());
        }

        /// <inheritdoc />
        public virtual TBuilder ListenDoors(params DoorId[] doorIds)
        {
            foreach (var door in doorIds)
                this._listenDoors.Add(door);
            return GetBuilder();
        }

        /// <inheritdoc />
        public virtual TBuilder ListenSignals(params Guid[] signalIds)
        {
            return ListenSignals(signalIds.Select(d => new SignalId(d, string.Empty)).ToArray());
        }

        /// <inheritdoc />
        public virtual TBuilder ListenSignals(params SignalId[] signalIds)
        {
            foreach (var door in signalIds)
                this._listenSignals.Add(door);
            return GetBuilder();
        }

        #region Tools

        /// <summary>
        /// Gets the builder.
        /// </summary>
        protected virtual TBuilder GetBuilder()
        {
            if (this is TBuilder builder)
                return builder;

            throw new NotSupportedException("GetBuilder MUST BE overrided for type " + typeof(TBuilder));
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Default event option builder
    /// </summary>
    /// <seealso cref="IEventControllerOptionsBuilder{TBuilder}" />
    public sealed class EventControllerOptionsBuilder : EventControllerOptionsBuilder<IEventControllerOptionsBuilder>, IEventControllerOptionsBuilder
    {
    }
}
