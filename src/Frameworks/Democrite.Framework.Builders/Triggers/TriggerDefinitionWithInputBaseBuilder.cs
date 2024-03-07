// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Implementations.Triggers
{
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;

    /// <summary>
    /// Get trigger definition
    /// </summary>
    /// <typeparam name="TOutputMesage">The type of the output mesage.</typeparam>
    /// <seealso cref="ITriggerDefinitionBuilder" />
    internal abstract class TriggerDefinitionWithInputBaseBuilder : TriggerDefinitionBaseBuilder<ITriggerDefinitionFinalizeBuilder>, ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder>, ITriggerDefinitionFinalizeBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionWithInputBaseBuilder"/> class.
        /// </summary>
        protected TriggerDefinitionWithInputBaseBuilder(TriggerTypeEnum triggerType,
                                                        string displayName,
                                                        Guid? fixUid = null)
            : base(triggerType, displayName, fixUid)
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
        public ITriggerDefinitionFinalizeBuilder SetOutput(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> outputBuilders)
        {
            var builder = new TriggerOutputBuilder();
            ArgumentNullException.ThrowIfNull(nameof(outputBuilders));
            this.TriggerGlobalOutputDefinition = outputBuilders(builder).Build();

            return this;
        }

        #endregion
    }
}
