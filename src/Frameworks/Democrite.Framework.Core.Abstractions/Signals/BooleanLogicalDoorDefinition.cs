// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Aggreagtor door that use a logical formula to define if synpase fire or not
    /// </summary>
    /// <seealso cref="DoorDefinition" />
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class BooleanLogicalDoorDefinition : DoorDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanLogicalDoorDefinition"/> class.
        /// </summary>
        public BooleanLogicalDoorDefinition(Guid uid,
                                            string name,
                                            string? group,
                                            string logicalFormula,
                                            string vgrainInterfaceFullName,
                                            IEnumerable<SignalId>? signalSourceIds,
                                            IEnumerable<DoorId>? doorSourceIds,
                                            Dictionary<string, Guid> variableNames,
                                            bool useCurrentDoorStatus,
                                            TimeSpan? activeWindowInterval = null,
                                            TimeSpan? retentionMaxDelay = null,
                                            uint? historyMaxRetention = null,
                                            uint? notConsumedMaxRetiention = null)
            : base(uid, name, group, vgrainInterfaceFullName, signalSourceIds, doorSourceIds, activeWindowInterval, retentionMaxDelay, historyMaxRetention, notConsumedMaxRetiention)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(logicalFormula);

            this.LogicalFormula = logicalFormula;
            this.UseCurrentDoorStatus = useCurrentDoorStatus;
            this.VariableNames = variableNames ?? DictionaryHelper<string, Guid>.ReadOnly;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logical formula.
        /// </summary>
        [Id(0)]
        public string LogicalFormula { get; }

        /// <summary>
        /// Gets the variable names.
        /// </summary>
        [Id(1)]
        public IReadOnlyDictionary<string, Guid> VariableNames { get; }

        /// <summary>
        /// Gets a value indicating whether use current door status.
        /// </summary>
        [Id(2)]
        public bool UseCurrentDoorStatus { get; }

        #endregion

        #region Methods

        /// <inheritdoc />>
        protected override bool OnDoorChildValidate(ILogger logger, bool matchErrorAsWarning)
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(this.LogicalFormula))
            {
                logger.OptiLog(LogLevel.Critical, "LogicalFormula MUST not be null or empty");
                isValid = false;
            }

            return isValid;
        }

        #endregion
    }
}
