// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Door that relay a signal if a specific condition on it is respected
    /// </summary>
    /// <seealso cref="SignalNetworkBasePartDefinition" />
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class RelayFilterDoorDefinition : DoorDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayFilterDoorDefinition"/> class.
        /// </summary>
        public RelayFilterDoorDefinition(Guid uid,
                                         string name,
                                         string? group,
                                         string vgrainInterfaceFullName,
                                         IEnumerable<SignalId>? signalSourceIds,
                                         IEnumerable<DoorId>? doorSourceIds,
                                         ConditionExpressionDefinition filterCondition,
                                         bool dontRelaySignalContent,
                                         TimeSpan? activeWindowInterval = null,
                                         TimeSpan? retentionMaxDelay = null,
                                         uint? historyMaxRetention = null,
                                         uint? notConsumedMaxRetiention = null)
            : base(uid,
                   name,
                   group,
                   vgrainInterfaceFullName,
                   signalSourceIds,
                   doorSourceIds,
                   activeWindowInterval,
                   retentionMaxDelay,
                   historyMaxRetention,
                   notConsumedMaxRetiention)
        {
            ArgumentNullException.ThrowIfNull(filterCondition);

            this.FilterCondition = filterCondition;
            this.DontRelaySignalContent = dontRelaySignalContent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the filter condition.
        /// </summary>
        [Id(0)]
        public ConditionExpressionDefinition FilterCondition { get; }

        /// <summary>
        /// Gets a value indicating whether the door signal must not transmit the content of the sign relayed.
        /// </summary>
        [Id(1)]
        public bool DontRelaySignalContent { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnDoorChildValidate(ILogger logger, bool matchErrorAsWarning)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            if (this.FilterCondition == null)
            {
                logger.OptiLog(LogLevel.Critical, "FilterCondition MUST not be null");
                return false;
            }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            return true;
        }

        #endregion
    }
}
