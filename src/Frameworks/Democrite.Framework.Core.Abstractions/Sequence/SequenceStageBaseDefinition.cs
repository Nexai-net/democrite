// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Define information needed to setup, run and diagnostic a stage
    /// </summary>
    public abstract class SequenceStageBaseDefinition : Equatable<ISequenceStageDefinition>,
                                                        ISupportDebugDisplayName,
                                                        ISequenceStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ISequenceStageDefinition"/> class.
        /// </summary>
        public SequenceStageBaseDefinition(StageTypeEnum type,
                                           Type? input,
                                           Type? output,
                                           SequenceOptionStageDefinition? options = null,
                                           bool preventReturn = false,
                                           Guid? uid = null)
        {
            this.Uid = uid ?? options?.StageId ?? Guid.NewGuid();

            if (this.Uid == Guid.Empty)
                this.Uid = Guid.NewGuid();

            this.Type = type;
            this.Options = options ?? SequenceOptionStageDefinition.Default;

            this.PreventReturn = preventReturn;

            this.Input = input;
            if (!preventReturn)
                this.Output = output;
        }

        #endregion

        #region Properties     

        /// <summary>
        /// Gets stage unique id.
        /// </summary>
        public Guid Uid { get; }

        /// <summary>
        /// Gets a value indicating whether any return is prevent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prevent type return]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventReturn { get; }

        /// <summary>
        /// Gets the input first stage input.
        /// </summary>
        public Type? Input { get; }

        /// <summary>
        /// Gets the input first stage ouytput.
        /// </summary>
        public Type? Output { get; }

        /// <summary>
        /// Gets the stage type.
        /// </summary>
        public StageTypeEnum Type { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        public SequenceOptionStageDefinition Options { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected sealed override bool OnEquals([NotNull] ISequenceStageDefinition other)
        {
            return this.Type == other.Type &&
                   this.Input == other.Input &&
                   this.Output == other.Output &&
                   this.PreventReturn == other.PreventReturn &&
                   this.Options == other.Options &&
                   OnStageEquals(other);
        }

        /// <inheritdoc />
        protected sealed override int OnGetHashCode()
        {
            return this.Type.GetHashCode() ^
                   (this.Input?.GetHashCode() ?? 0) ^
                   (this.Output?.GetHashCode() ?? 0) ^
                   this.PreventReturn.GetHashCode() ^
                   this.Options.GetHashCode() ^
                   OnStageGetHashCode();
        }

        /// <summary>
        /// Called to get children hash code
        /// </summary>
        protected abstract int OnStageGetHashCode();

        /// <summary>
        /// Called when to check quality
        /// </summary>
        protected abstract bool OnStageEquals(ISequenceStageDefinition other);

        /// <inheritdoc />
        public virtual string ToDebugDisplayName()
        {
            return ToString() ?? typeof(ISequenceStageDefinition).Name;
        }

        #endregion
    }
}
