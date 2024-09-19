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
    /// Cron trigger builder
    /// </summary>
    internal sealed class TriggerDefinitionCronBuilder : TriggerDefinitionWithInputBaseBuilder, ITriggerDefinitionFinalizeBuilder, ITriggerDefinitionCronBuilder
    {
        #region Fields

        private readonly string _cronExpression;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionBaseBuilder"/> class.
        /// </summary>
        public TriggerDefinitionCronBuilder(string cronExpression,
                                            string simpleNameIdentifier, 
                                            string displayName,
                                            Guid? fixUid = null,
                                            Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
            : base(TriggerTypeEnum.Cron, simpleNameIdentifier, displayName, fixUid, metadataBuilder)
        {
            this._cronExpression = cronExpression;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override TriggerDefinition Build()
        {
            int nbRefPart = 0;
            CronTriggerDefinition.ValidateCronExpression(ref nbRefPart, this._cronExpression);

            return new CronTriggerDefinition(this.Uid,
                                             GetRefId(),
                                             this.DisplayName,       
                                             this.Targets,
                                             true,
                                             this._cronExpression,
                                             nbRefPart == 6,
                                             this.DefinitionMetaData,
                                             base.TriggerGlobalOutputDefinition);
        }

        #endregion
    }
}
