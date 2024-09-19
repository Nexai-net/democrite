// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Doors
{
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Exceptions;
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
        public DoorStartBuilder(string simpleNameIdentifier, string? displayName = null, Guid? uid = null, Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
            : base(simpleNameIdentifier, displayName, uid, metaDataBuilder)
        {
            this._signalIds = new HashSet<SignalId>();
            this._doorIds = new HashSet<DoorId>();

            this.RetentionMaxPeriod = DoorDefinition.DEFAULT_RETENTION_MAX_DELAY;
            this.HistoryMaxRetention = DoorDefinition.DEFAULT_HISTORY_RETENTION;
            this.NotConsumedMaxRetiention = DoorDefinition.DEFAULT_NOT_CONSUMED_RETENTION;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public TimeSpan? RetentionMaxPeriod { get; private set; }

        /// <inheritdoc />
        public uint? HistoryMaxRetention { get; private set; }

        /// <inheritdoc />
        public uint? NotConsumedMaxRetiention { get; private set; }

        /// <inheritdoc />
        public IReadOnlyCollection<SignalId> SignalIds
        {
            get { return this._signalIds; }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<DoorId> DoorIds
        {
            get { return this._doorIds; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDoorBuilder SetSignalRetention(TimeSpan? retentionMaxPeriod)
        {
            this.RetentionMaxPeriod = retentionMaxPeriod;
            return this;
        }

        /// <inheritdoc />
        public IDoorBuilder SetSignalRetention(uint? history, uint? notConsumed)
        {
            this.HistoryMaxRetention = history;

            if (notConsumed != null && notConsumed < 1)
            {
                throw new InvalidDefinitionPropertyValueException(typeof(DoorDefinition),
                                                                  nameof(DoorDefinition.NotConsumedMaxRetiention),
                                                                  notConsumed?.ToString() ?? string.Empty,
                                                                  "Couldn't be less than 1");
            }

            this.HistoryMaxRetention = history;
            return this;
        }

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
