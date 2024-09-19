// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    using Democrite.Framework.Builders.Implementations.Triggers;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;

    /// <summary>
    /// Define a trigger based on stream incmming message
    /// </summary>
    /// <seealso cref="TriggerDefinitionBaseBuilder" />
    /// <seealso cref="ITriggerDefinitionFinalizeBuilder" />
    internal sealed class TriggerDefinitionStreamBuilder : TriggerDefinitionBaseBuilder, ITriggerDefinitionStreamBuilder
    {
        #region Fields
        
        private readonly Guid _streamSourceDefinitionUid;
        private uint? _fixedMaxConcurrentProcess;
        private uint? _relativeMaxConcurrentProcessFactor;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionStreamBuilder"/> class.
        /// </summary>
        public TriggerDefinitionStreamBuilder(Guid streamSourceDefinitionUid,
                                              string simpleNameIdentifier,
                                              string displayName,
                                              Guid? fixUid,
                                              Action<IDefinitionMetaDataBuilder>? metadataBuilder)
            : base(TriggerTypeEnum.Stream, simpleNameIdentifier, displayName, fixUid, metadataBuilder)
        {
            this._streamSourceDefinitionUid = streamSourceDefinitionUid;
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        public override TriggerDefinition Build()
        {
            return new StreamTriggerDefinition(this.Uid,
                                               GetRefId(),
                                               this.DisplayName,
                                               this.Targets,
                                               true,
                                               this._fixedMaxConcurrentProcess ?? StreamTriggerDefinition.DEFAULT_FIX_CONCURRENT_PROCESS,
                                               this._relativeMaxConcurrentProcessFactor,
                                               this._streamSourceDefinitionUid,
                                               base.DefinitionMetaData);
        }

        /// <inheritdoc />
        public ITriggerDefinitionBuilder MaxConcurrentProcess(uint maxConcurrent)
        {
            this._fixedMaxConcurrentProcess = Math.Max(1, maxConcurrent);
            return this;
        }

        /// <inheritdoc />
        public ITriggerDefinitionBuilder MaxConcurrentFactorClusterRelativeProcess(uint maxConcurrent)
        {
            this._relativeMaxConcurrentProcessFactor = Math.Max(1, maxConcurrent);
            return this;
        }

        #endregion

    }
}
