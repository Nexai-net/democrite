// Copyright (c) Nexai.0
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Customizations;

    using System;

    [GenerateSerializer]
    internal struct SequenceExecutorStateSurrogate
    {
        /// <summary>
        /// Gets or sets the sequence definition identifier.
        /// </summary>
        [Id(0)]
        public Guid SequenceDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the sequence definition identifier.
        /// </summary>
        [Id(1)]
        public string SequenceDefinitionDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the flow uid.
        /// </summary>
        [Id(2)]
        public Guid FlowUid { get; set; }

        /// <summary>
        /// Gets or sets the instance identifier.
        /// </summary>
        [Id(3)]
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the sequence execution start.
        /// </summary>
        [Id(4)]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Gets or sets the main thread.
        /// </summary>
        [Id(5)]
        public SequenceExecutorExecThreadStateSurrogate? MainThread { get; set; }

        /// <summary>
        /// Gets or sets the execution customization.
        /// </summary>
        [Id(6)]
        public ExecutionCustomizationDescriptions? Customization { get; set; }
    }

    /// <summary>
    /// Converter used to serialize the <see cref="SequenceExecutorState"/> using <see cref="SequenceExecutorStateSurrogate"/> structure
    /// </summary>
    /// <seealso cref="IConverter{SequenceExecutorState, SequenceExecutorStateSurrogate}" />
    internal sealed class SequenceExecutorStateConverter : IConverter<SequenceExecutorState, SequenceExecutorStateSurrogate>
    {
        #region Fields
        
        private static readonly SequenceExecutorExecThreadStateConverter s_threadConverter;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SequenceExecutorStateConverter"/> class.
        /// </summary>
        static SequenceExecutorStateConverter()
        {
            s_threadConverter = new SequenceExecutorExecThreadStateConverter();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceExecutorState ConvertFromSurrogate(in SequenceExecutorStateSurrogate surrogate)
        {
            return new SequenceExecutorState(surrogate.SequenceDefinitionId,
                                             surrogate.SequenceDefinitionDisplayName,
                                             surrogate.FlowUid,
                                             surrogate.InstanceId,
                                             surrogate.MainThread != null ? s_threadConverter.ConvertFromSurrogate(surrogate.MainThread.Value) : null,
                                             surrogate.StartAt,
                                             surrogate.Customization);
        }

        /// <inheritdoc />
        public SequenceExecutorStateSurrogate ConvertToSurrogate(in SequenceExecutorState value)
        {
            return new SequenceExecutorStateSurrogate()
            {
                SequenceDefinitionId = value.SequenceDefinitionId,
                SequenceDefinitionDisplayName = value.SequenceDefinitionDisplayName,
                FlowUid = value.FlowUid,
                InstanceId = value.InstanceId,
                MainThread = value.MainThread != null
                                  ? s_threadConverter.ConvertToSurrogate(value.MainThread)
                                  : null,
                StartAt = value.StartAt,
                Customization = value.Customization
            };
        }

        #endregion
    }
}
