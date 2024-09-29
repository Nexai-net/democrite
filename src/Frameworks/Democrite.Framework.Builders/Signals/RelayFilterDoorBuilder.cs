// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Builders.Doors;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.Abstractions.Conditions;

    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Implementation of <see cref="IRelayFilterDoorBuilder"/> to construct <see cref="RelayFilterDoorDefinition"/>
    /// </summary>
    /// <seealso cref="IRelayFilterDoorBuilder" />
    /// <seealso cref="IDefinitionBaseBuilder{RelayFilterDoorDefinition}" />
    internal sealed class RelayFilterDoorBuilder : IRelayFilterDoorBuilder, IDefinitionBaseBuilder<DoorDefinition>
    {
        #region Fields
        
        private readonly IDoorWithListenerBuilder _rootDoorBuilder;
        private ConditionExpressionDefinition? _serializedExpression;
        private bool _dontRelaySignalContent;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayFilterDoorBuilder"/> class.
        /// </summary>
        public RelayFilterDoorBuilder(IDoorWithListenerBuilder rootDoorBuilder)
        {
            this._rootDoorBuilder = rootDoorBuilder;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDefinitionBaseBuilder<DoorDefinition> Condition<TContent>(Expression<Func<TContent, SignalMessage, bool>> filterExpression) 
            where TContent : struct
        {
            this._serializedExpression = filterExpression.Serialize();
            return this;
        }

        /// <inheritdoc />
        public IDefinitionBaseBuilder<DoorDefinition> Condition(Expression<Func<SignalMessage, bool>> filterExpression)
        {
            this._serializedExpression = filterExpression.Serialize();
            return this;
        }

        /// <inheritdoc />
        public IRelayFilterDoorBuilder DontRelaySignalContent()
        {
            this._dontRelaySignalContent = true;
            return this;
        }

        /// <inheritdoc />
        public DoorDefinition Build()
        {
            ArgumentNullException.ThrowIfNull(this._serializedExpression);

            return new RelayFilterDoorDefinition(this._rootDoorBuilder.Uid,
                                                 RefIdHelper.Generate(RefTypeEnum.Door, this._rootDoorBuilder.SimpleNameIdentifier, this._rootDoorBuilder.DefinitionMetaData?.NamespaceIdentifier),
                                                 this._rootDoorBuilder.DisplayName ?? this._rootDoorBuilder.SimpleNameIdentifier,
                                                 typeof(IRelayFilterVGrain).AssemblyQualifiedName!,
                                                 this._rootDoorBuilder.SignalIds,
                                                 this._rootDoorBuilder.DoorIds,
                                                 this._serializedExpression,
                                                 this._dontRelaySignalContent,
                                                 this._rootDoorBuilder.DefinitionMetaData,
                                                 null,
                                                 this._rootDoorBuilder.RetentionMaxPeriod,
                                                 this._rootDoorBuilder.HistoryMaxRetention,
                                                 this._rootDoorBuilder.NotConsumedMaxRetiention);
        }

        #endregion
    }
}
