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
    internal sealed class TriggerDefinitionStreamBuilder : TriggerDefinitionBaseBuilder<ITriggerDefinitionStreamFinalizeBuilder>, ITriggerDefinitionStreamFinalizeBuilder
    {
        #region Fields
        
        private readonly Guid _streamSourceDefinitionUid;
        private uint _maxConcurrentProcess;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionStreamBuilder"/> class.
        /// </summary>
        public TriggerDefinitionStreamBuilder(Guid streamSourceDefinitionUid, string displayName, Guid? fixUid)
            : base(TriggerTypeEnum.Stream, displayName, fixUid)
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
                                               this.DisplayName,
                                               this.Targets,
                                               true,
                                               this._maxConcurrentProcess,
                                               this._streamSourceDefinitionUid,
                                               base.DefinitionMetaData);
        }

        /// <inheritdoc />
        public IDefinitionBaseBuilder<TriggerDefinition> MaxConcurrentProcess(uint maxConcurrent)
        {
            this._maxConcurrentProcess = Math.Max(1, maxConcurrent);
            return this;
        }

        #endregion

    }
}
