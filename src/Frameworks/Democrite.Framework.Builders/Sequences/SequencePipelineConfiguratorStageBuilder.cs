// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;

    /// <inheritdoc />
    internal sealed class SequencePipelineConfiguratorStageBuilder : ISequencePipelineStageConfigurator //<TInput>, ISequencePipelineStageConfigurator
    {
        #region Fields

        private Guid _customStageId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineConfiguratorStageBuilder"/> class.
        /// </summary>
        public SequencePipelineConfiguratorStageBuilder()
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISequencePipelineStageConfigurator ContextParameter<TParameterType>(string parameterName, TParameterType value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISequencePipelineStageConfigurator Options<TParameterType>(Func<TParameterType> optionReturned)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISequencePipelineStageConfigurator Options<TParameterType>(Action<TParameterType> optionSetup) where TParameterType : new()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISequencePipelineStageConfigurator Uid(Guid uid)
        {
            this._customStageId = uid;
            return this;
        }

        /// <summary>
        /// Converts to definition.
        /// </summary>
        internal SequenceOptionStageDefinition? ToDefinition()
        {
            return new SequenceOptionStageDefinition(this._customStageId);
        }

        #endregion
    }
}
