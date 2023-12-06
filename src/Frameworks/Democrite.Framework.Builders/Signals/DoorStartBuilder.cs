// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Start door builder used to carry all default door information
    /// </summary>
    /// <seealso cref="IDoorBuilder" />
    internal sealed class DoorStartBuilder : SignalNetworkBasePartBuilder<IDoorBuilder>, IDoorBuilder, IDoorWithListenerBuilder
    {
        #region Fields

        private readonly HashSet<SignalId> _signalIds;
        private readonly HashSet<DoorId> _doorIds;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorStartBuilder"/> class.
        /// </summary>
        public DoorStartBuilder(string name, Guid? uid = null)
            : base(name, uid)
        {
            this._signalIds = new HashSet<SignalId>();
            this._doorIds = new HashSet<DoorId>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal ids.
        /// </summary>
        public IReadOnlyCollection<SignalId> SignalIds
        {
            get { return this._signalIds; }
        }

        /// <summary>
        /// Gets the door ids.
        /// </summary>
        public IReadOnlyCollection<DoorId> DoorIds
        {
            get { return this._doorIds; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params SignalDefinition[] signalDefinition)
        {
            return Listen(signalDefinition.Select(p => p.SignalId).ToArray());
        }

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params SignalId[] signalIds)
        {
            foreach (var signalId in signalIds)
                this._signalIds.Add(signalId);
            return this;
        }

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params DoorDefinition[] signalDefinition)
        {
            return Listen(signalDefinition.Select(p => p.DoorId).ToArray());
        }

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params DoorId[] doorIds)
        {
            foreach (var doorId in doorIds)
                this._doorIds.Add(doorId);
            return this;
        }

        #endregion
    }
}
