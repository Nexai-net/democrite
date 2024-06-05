// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    using Microsoft.Extensions.Logging;

    using System.Diagnostics;

    /// <summary>
    /// Sequence option builder
    /// </summary>
    internal sealed class SequenceOptionBuilder : ISequenceOptionBuilder, IDefinitionBaseBuilder<SequenceOptionDefinition>
    {
        #region Fields

        private LogLevel _minimalLogLevel = LogLevel.Information;
        private bool _preventSequenceExecutorStateStorage;

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISequenceOptionBuilder MinimalLogLevel(LogLevel logLevel)
        {
            this._minimalLogLevel = logLevel;
            return this;
        }

        /// <inheritdoc />
        public ISequenceOptionBuilder PreventSequenceExecutorStateStorage()
        {
            this._preventSequenceExecutorStateStorage = true;
            return this;
        }

        /// <inheritdoc />
        public SequenceOptionDefinition Build()
        {
            return new SequenceOptionDefinition(new SequenceDiagnosticOptionDefinition(Debugger.IsAttached, this._minimalLogLevel),
                                                this._preventSequenceExecutorStateStorage);
        }

        #endregion
    }
}
