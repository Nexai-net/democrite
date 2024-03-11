// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Define information needed to setup, run and diagnostic a stage
    /// </summary>
    public interface ISequenceStageDefinition
    {
        #region Properties     

        /// <summary>
        /// Gets stage unique id.
        /// </summary>
        Guid Uid { get; }

        /// <summary>
        /// Gets a value indicating whether any return is prevent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prevent type return]; otherwise, <c>false</c>.
        /// </value>
        bool PreventReturn { get; }

        /// <summary>
        /// Gets the input first stage input.
        /// </summary>
        AbstractType? Input { get; }

        /// <summary>
        /// Gets the input first stage ouytput.
        /// </summary>
        AbstractType? Output { get; }

        /// <summary>
        /// Gets the stage type.
        /// </summary>
        StageTypeEnum Type { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        SequenceOptionStageDefinition Options { get; }

        #endregion
    }
}
