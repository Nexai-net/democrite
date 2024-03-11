// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Builders.Doors;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Elvex.Toolbox.Abstractions.Enums;
    using Elvex.Toolbox.Helpers;

    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Extensions method used to customize the door behavior desired
    /// </summary>
    public static class DoorHandlerExtensions
    {
        #region Methods

        /// <summary>
        /// Uses a relay filter door definition. <br/>
        /// A relay filter door relay signal that valid the setup conditions
        /// </summary>
        /// <typeparam name="TContent">Token carray by the signal</typeparam>
        public static IDefinitionBaseBuilder<DoorDefinition> UseRelayFilter<TContent>(this IDoorWithListenerBuilder doorWithListenerBuilder,
                                                                                      Expression<Func<TContent, SignalMessage, bool>> filterExpression)
            where TContent : struct
        {
            return UseRelayFilter(doorWithListenerBuilder , (IRelayFilterDoorBuilder r) => r.Condition<TContent>(filterExpression));
        }

        /// <summary>
        /// Uses a relay filter door definition. <br/>
        /// A relay filter door relay signal that valid the setup conditions
        /// </summary>
        public static IDefinitionBaseBuilder<DoorDefinition> UseRelayFilter(this IDoorWithListenerBuilder doorWithListenerBuilder,
                                                                            Expression<Func<SignalMessage, bool>> filterExpression)
        {
            return UseRelayFilter(doorWithListenerBuilder , (IRelayFilterDoorBuilder r) => r.Condition(filterExpression));
        }

        /// <summary>
        /// Uses a relay filter door definition. <br/>
        /// A relay filter door relay signal that valid the setup conditions
        /// </summary>
        public static IDefinitionBaseBuilder<DoorDefinition> UseRelayFilter(this IDoorWithListenerBuilder doorWithListenerBuilder,
                                                                            Func<IRelayFilterDoorBuilder, IDefinitionBaseBuilder<DoorDefinition>> builder)
        {
            ArgumentNullException.ThrowIfNull(doorWithListenerBuilder);
            ArgumentNullException.ThrowIfNull(builder);

            return builder(new RelayFilterDoorBuilder(doorWithListenerBuilder));
        }

        /// <summary>
        /// Uses the logical aggregator.
        /// </summary>
        public static IDefinitionBaseBuilder<DoorDefinition> UseLogicalAggregator(this IDoorWithListenerBuilder doorWithListenerBuilder,
                                                                                  Func<ILogicalDoorBuilder, IDefinitionBaseBuilder<DoorDefinition>> builder)
        {
            ArgumentNullException.ThrowIfNull(doorWithListenerBuilder);
            ArgumentNullException.ThrowIfNull(builder);

            return builder(new LogicalDoorBuilder(doorWithListenerBuilder));
        }

        /// <summary>
        /// Uses the logical aggregator.
        /// </summary>
        /// <param name="doorWithListenerBuilder">The door with listener builder.</param>
        /// <param name="aggregationCondition">The aggregation condition, example with received 2 signals on 3 listen, <see cref="LogicEnum.And"/> will not trigger, <see cref="LogicEnum.Or"/> and <see cref="LogicEnum.ExclusiveOr"/> will.</param>
        /// <param name="interval">The interval use to interpret the signal. Default 1s window</param>
        /// <param name="onlyOneByInterval">Allow only one door firering by interval</param>
        public static IDefinitionBaseBuilder<DoorDefinition> UseLogicalAggregator(this IDoorWithListenerBuilder doorWithListenerBuilder,
                                                                                  LogicEnum aggregationCondition = LogicEnum.And,
                                                                                  TimeSpan? interval = null,
                                                                                  bool onlyOneByInterval = false)
        {
            var fixedInterval = interval ?? TimeSpan.FromSeconds(1);

            return UseLogicalAggregator(doorWithListenerBuilder, b =>
            {
                var builder = b.ActiveWindowInterval(fixedInterval!);

                var variables = new List<string>(doorWithListenerBuilder.SignalIds.Count + doorWithListenerBuilder.DoorIds.Count);
                uint indx = 0;
                ushort nbVarDigits = (ushort)(((doorWithListenerBuilder.SignalIds.Count + doorWithListenerBuilder.DoorIds.Count) / 26) + 1);

                foreach (var signal in doorWithListenerBuilder.SignalIds)
                {
                    var variable = FormulaHelper.GenerateVariableName(indx, nbVarDigits);
                    variables.Add(variable);

                    builder.AssignVariableName(variable, signal);
                    indx++;
                }

                foreach (var door in doorWithListenerBuilder.DoorIds)
                {
                    var variable = FormulaHelper.GenerateVariableName(indx, nbVarDigits);
                    variables.Add(variable);

                    builder.AssignVariableName(variable, door);
                    indx++;
                }

                var operatorChar = LogicHelper.GetSymbolFrom(aggregationCondition);
                var formula = string.Join(" " + operatorChar + " ", variables);

                if (onlyOneByInterval)
                    formula = "(" + formula + ") & !this";

                return builder.Formula(formula);
            });
        }

        /// <summary>
        /// Fire at any signal source insignal
        /// </summary>
        public static IDefinitionBaseBuilder<DoorDefinition> Relay(this IDoorWithListenerBuilder doorWithListenerBuilder)
        {
            // ActiveWindowInterval small used to try caching only source element
            var fixedInterval = TimeSpan.FromMilliseconds(100);
            return UseLogicalAggregator(doorWithListenerBuilder, LogicEnum.Or, fixedInterval, onlyOneByInterval: false);
        }

        /// <summary>
        /// Uses be able to setup any TargetVGrain <typeparamref name="THandler"/> as door handler.
        /// </summary>
        /// <typeparam name="THandler">Must be the Interface VGrain handler that implement <see cref="IDoorVGrain"/></typeparam>
        /// <remarks>
        ///     We recommand that <typeparamref name="THandler"/> implementation inherite from <see cref="Framework.Node.Signals.Doors.DoorBaseVGrain{THandler, TDoorDef}"/> from extension <see cref="Democrite.Framework.Node.Signals"/>
        /// </remarks>
        public static IDefinitionBaseBuilder<DoorDefinition> UseCustomHandler<THandler>(this IDoorWithListenerBuilder doorWithListenerBuilder, Func<IDoorWithListenerBuilder, IDefinitionBaseBuilder<DoorDefinition>> doorDefinitionBuilder)
            where THandler : IDoorVGrain
        {
            if (typeof(THandler).IsInterface == false)
                throw new ArgumentException(nameof(THandler) + " must be an interface that implement IDoorVGrain");

            return doorDefinitionBuilder?.Invoke(doorWithListenerBuilder) ?? throw new InvalidOperationException("Invalid null build");
        }

        #endregion
    }
}
