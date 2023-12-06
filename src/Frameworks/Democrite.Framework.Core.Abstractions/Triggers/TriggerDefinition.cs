// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Helpers;

    using System.ComponentModel;

    /// <summary>
    /// Base definition of trigger definition
    /// </summary>
    [Serializable]
    [Immutable]
    [ImmutableObject(true)]
    public abstract class TriggerDefinition : ITriggerDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinition"/> class.
        /// </summary>
        protected TriggerDefinition(Guid uid,
                                    TriggerTypeEnum triggerType,
                                    IEnumerable<Guid> targetSequenceIds,
                                    IEnumerable<SignalId> targetSignalIds,
                                    bool enabled,
                                    InputSourceDefinition? inputSourceDefinition = null)
        {
            this.Uid = uid;
            this.Enabled = enabled;
            this.TriggerType = triggerType;
            this.TargetSequenceIds = targetSequenceIds?.ToArray() ?? EnumerableHelper<Guid>.ReadOnlyArray;
            this.TargetSignalIds = targetSignalIds?.ToArray() ?? EnumerableHelper<SignalId>.ReadOnlyArray;
            this.InputSourceDefinition = inputSourceDefinition;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Guid Uid { get; }

        /// <inheritdoc />
        public bool Enabled { get; }

        /// <inheritdoc />
        public TriggerTypeEnum TriggerType { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<Guid> TargetSequenceIds { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<SignalId> TargetSignalIds { get; }

        /// <inheritdoc />
        public InputSourceDefinition? InputSourceDefinition { get; }

        #endregion
    }
}
