// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Toolbox.Helpers;

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
    public sealed class LogicalAggregatorDoorDefinition : DoorDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalAggregatorDoorDefinition"/> class.
        /// </summary>
        public LogicalAggregatorDoorDefinition(Guid uid,
                                                  string name,
                                                  string? group,
                                                  string logicalFormula,
                                                  string vgrainInterfaceFullName,
                                                  IEnumerable<SignalId>? signalSourceIds,
                                                  IEnumerable<DoorId>? doorSourceIds,
                                                  TimeSpan interval,
                                                  Dictionary<string, Guid> variableNames,
                                                  bool useCurrentDoorStatus)
            : base(uid, name, group, vgrainInterfaceFullName, signalSourceIds, doorSourceIds, interval)
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
    }
}
