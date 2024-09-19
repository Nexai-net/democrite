// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Implementations.Triggers
{
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;

    /// <summary>
    /// Get trigger definition
    /// </summary>
    /// <typeparam name="TOutputMesage">The type of the output mesage.</typeparam>
    /// <seealso cref="ITriggerDefinitionBuilder" />
    internal abstract class TriggerDefinitionWithInputBaseBuilder : TriggerDefinitionBaseBuilder, ITriggerDefinitionBuilder, ITriggerDefinitionFinalizeBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionWithInputBaseBuilder"/> class.
        /// </summary>
        protected TriggerDefinitionWithInputBaseBuilder(TriggerTypeEnum triggerType,
                                                        string simpleNameIdentifier,
                                                        string displayName,
                                                        Guid? fixUid,
                                                        Action<IDefinitionMetaDataBuilder>? metadataBuilder)
            : base(triggerType, simpleNameIdentifier, displayName, fixUid, metadataBuilder)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the trigger output definition, this will be used by any target that doesn't have a dedicated output provider
        /// </summary>
        protected DataSourceDefinition? TriggerGlobalOutputDefinition { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ITriggerDefinitionBuilder SetOutput(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> outputBuilders)
        {
            var builder = new TriggerOutputBuilder();
            ArgumentNullException.ThrowIfNull(nameof(outputBuilders));
            this.TriggerGlobalOutputDefinition = outputBuilders(builder).Build();

            return this;
        }

        /// <inheritdoc />
        public ITriggerDefinitionBuilder SetOutput(DataSourceDefinition outputdef)
        {
            this.TriggerGlobalOutputDefinition = outputdef;
            return this;
        }

        #endregion
    }
}
