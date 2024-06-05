// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Sequence option relative to diagnostic
    /// </summary>
    public sealed class SequenceOptionDiagnostic
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOptionDiagnostic"/> class.
        /// </summary>
        public SequenceOptionDiagnostic(SequenceDiagnosticOptionDefinition? sequenceStageDefinition = null)
        {
            this.SaveAllStageInfo = sequenceStageDefinition?.SaveAllStageInfo ?? SequenceDiagnosticOptionDefinition.Default.SaveAllStageInfo;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether save all stage information will be save for diagnostic purpose.
        /// </summary>
        public bool SaveAllStageInfo { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize from <see cref="SequenceDiagnosticOptionDefinition"/>
        /// </summary>
        internal void FromDefintion(SequenceDiagnosticOptionDefinition diagnostic)
        {
            this.SaveAllStageInfo = diagnostic.SaveAllStageInfo;
        }

        /// <summary>
        /// Converts to definition.
        /// </summary>
        public SequenceDiagnosticOptionDefinition ToDefinition()
        {
            return new SequenceDiagnosticOptionDefinition(this.SaveAllStageInfo, Microsoft.Extensions.Logging.LogLevel.Information);
        }

        #endregion
    }
}
