// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Builders.Exceptions;
    using Democrite.Framework.Builders.Resources;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Statements;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Create a <see cref="LogicalAggregatorDoorDefinition"/>
    /// </summary>
    /// <seealso cref="ILogicalDoorBuilder" />
    internal sealed class LogicalDoorBuilder : ILogicalDoorBuilder, IDefinitionBaseBuilder<DoorDefinition>, ILogicalDoorBuilderWithInterval
    {
        #region Fields

        private static readonly Regex s_formulaCharAllowed = new Regex(@"^[a-zA-Z!&|^\(\)\s]+$");
        private static readonly Regex s_variableCharAllowed = new Regex(@"^[a-zA-Z]+$");

        private readonly IDoorWithListenerBuilder _doorBuilder;
        private readonly Dictionary<Guid, string> _variableNames;

        private string? _logicalFormula;
        private TimeSpan? _interval;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalDoorBuilder"/> class.
        /// </summary>
        public LogicalDoorBuilder(IDoorWithListenerBuilder doorBuilder)
        {
            this._doorBuilder = doorBuilder;
            this._variableNames = new Dictionary<Guid, string>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ILogicalDoorBuilderWithInterval ListenWindowMode(ListenWindowModeEnum windowModeEnum)
        {
            throw new NotSupportedException("Feature to yet implement and supported");
        }

        /// <inheritdoc />
        public ILogicalDoorBuilderWithInterval AssignVariableName(string variableName, SignalId signalId)
        {
            ArgumentException.ThrowIfNullOrEmpty(variableName);

            if (!s_variableCharAllowed.IsMatch(variableName))
            {
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.VariableNames),
                                                    BuildErrorSR.VariableOnlyCharAllowed);
            }

            this._variableNames.Add(signalId.Uid, variableName);
            return this;
        }

        /// <inheritdoc />
        public ILogicalDoorBuilderWithInterval AssignVariableName(string variableName, DoorId doorId)
        {
            ArgumentException.ThrowIfNullOrEmpty(variableName);

            if (!s_variableCharAllowed.IsMatch(variableName))
            {
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.VariableNames),
                                                    BuildErrorSR.VariableOnlyCharAllowed);
            }

            this._variableNames.Add(doorId.Uid, variableName);
            return this;
        }

        /// <inheritdoc />
        public ILogicalDoorBuilderWithInterval AssignVariableName(string variableName, SignalDefinition signalId)
        {
            return AssignVariableName(variableName, signalId.SignalId);
        }

        /// <inheritdoc />
        public ILogicalDoorBuilderWithInterval AssignVariableName(string variableName, DoorDefinition doorId)
        {
            return AssignVariableName(variableName, doorId.DoorId);
        }

        /// <inheritdoc />
        public ILogicalDoorBuilderWithInterval UseVariableThis()
        {
            this._variableNames.Add(this._doorBuilder.Uid, "this");
            return this;
        }

        /// <inheritdoc />
        public IDefinitionBaseBuilder<DoorDefinition> Formula(string formula)
        {
            ArgumentException.ThrowIfNullOrEmpty(formula);
            this._logicalFormula = formula;
            return this;
        }

        /// <inheritdoc />
        public ILogicalDoorBuilderWithInterval Interval(TimeSpan interval)
        {
            this._interval = interval;
            return this;
        }

        /// <inheritdoc />
        public DoorDefinition Build()
        {
            if (this._interval == null)
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.Interval), BuildErrorSR.MandatoryValueMissing);

            if (string.IsNullOrEmpty(this._logicalFormula))
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.LogicalFormula), BuildErrorSR.MandatoryValueMissing);

            var allVariablesIds = this._doorBuilder.SignalIds
                                                      .Select(p => (p.Uid, p.Name))
                                                      .Concat(this._doorBuilder.DoorIds.Select(s => (s.Uid, s.Name)))
                                                      .Distinct()
                                                      .ToArray();

            if (allVariablesIds.Length == 0)
            {
                throw new InvalidParameterException(nameof(this._doorBuilder.SignalIds) + "-" + nameof(this._doorBuilder.DoorIds),
                                                    BuildErrorSR.MandatoryValueMissing);
            }

            if (allVariablesIds.Length != this._variableNames.Count(g => g.Key != this._doorBuilder.Uid))
            {
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.VariableNames),
                                                    BuildErrorSR.VariableNamesIsNotAlignWIthSources);
            }

            if (string.IsNullOrEmpty(this._logicalFormula))
            {
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.LogicalFormula),
                                                    BuildErrorSR.MandatoryValueMissing);
            }

            var missingVariable = this._variableNames.Values
                                                     .Where(vr => this._logicalFormula!.Contains(vr, StringComparison.OrdinalIgnoreCase) == false)
                                                     .ToArray();

            if (missingVariable.Length > 0)
            {
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.LogicalFormula),
                                                    BuildErrorSR.VariableNameIsMissingFromFormula.WithArguments(string.Join(", ", missingVariable)));
            }

            if (!ExpressionBuilder.CanBuildBoolLogicStatement(this._logicalFormula, this._variableNames.Values.ToArray(), out var reason))
                throw new InvalidParameterException(nameof(LogicalAggregatorDoorDefinition.LogicalFormula), reason);

            return new LogicalAggregatorDoorDefinition(this._doorBuilder.Uid,
                                                          this._doorBuilder.Name,
                                                          this._doorBuilder.GroupName,
                                                          this._logicalFormula,
                                                          typeof(ILogicalDoorVGrain).AssemblyQualifiedName!,
                                                          this._doorBuilder.SignalIds,
                                                          this._doorBuilder.DoorIds,
                                                          this._interval.Value,
                                                          this._variableNames.ToDictionary(kv => kv.Value, kv => kv.Key),
                                                          this._variableNames.ContainsKey(this._doorBuilder.Uid));
        }

        #endregion
    }
}
