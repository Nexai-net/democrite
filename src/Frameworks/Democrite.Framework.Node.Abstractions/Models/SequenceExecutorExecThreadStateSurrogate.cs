// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models
{
    using Elvex.Toolbox.Helpers;

    [GenerateSerializer]
    public struct SequenceExecutorExecThreadStateSurrogate
    {
        /// <summary>
        /// Gets or sets the flow uid.
        /// </summary>
        [Id(0)]
        public Guid FlowUid { get; set; }

        /// <summary>
        /// Gets or sets the flow definition identifier.
        /// </summary>
        [Id(1)]
        public Guid FlowDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the current stage execute identifier.
        /// </summary>
        [Id(2)]
        public Guid CurrentStageExecId { get; set; }

        /// <summary>
        /// Gets or sets the parent stage execute identifier.
        /// </summary>
        [Id(3)]
        public Guid? ParentStageExecId { get; set; }

        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        [Id(4)]
        public Guid? Cursor { get; set; }

        /// <summary>
        /// Gets or sets the thread input.
        /// </summary>
        [Id(5)]
        public object? ThreadInput { get; set; }

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        [Id(6)]
        public object? Output { get; set; }

        /// <summary>
        /// Gets or sets the inner threads.
        /// </summary>
        [Id(7)]
        public IEnumerable<SequenceExecutorExecThreadStateSurrogate>? InnerThreads { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        [Id(8)]
        public Exception? Exception { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SequenceExecutorExecThreadStateSurrogate"/> is started.
        /// </summary>
        [Id(9)]
        public bool Started { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SequenceExecutorExecThreadStateSurrogate"/> is done.
        /// </summary>
        [Id(10)]
        public bool Done { get; set; }
    }

    /// <summary>
    /// Converter 
    /// </summary>
    /// <seealso cref="IConverter{SequenceExecutorExecThreadState, SequenceExecutorExecThreadStateSurrogate}" />
    internal sealed class SequenceExecutorExecThreadStateConverter : IConverter<SequenceExecutorExecThreadState, SequenceExecutorExecThreadStateSurrogate>
    {
        /// <inheritdoc />
        public SequenceExecutorExecThreadState ConvertFromSurrogate(in SequenceExecutorExecThreadStateSurrogate surrogate)
        {
            return new SequenceExecutorExecThreadState(surrogate.FlowUid,
                                                       surrogate.FlowDefinitionId,
                                                       surrogate.CurrentStageExecId,
                                                       surrogate.ParentStageExecId,
                                                       surrogate.Cursor,
                                                       surrogate.ThreadInput,
                                                       surrogate.Output,
                                                       surrogate.InnerThreads
                                                               ?.Select(i => ConvertFromSurrogate(i))
                                                                .ToArray(),
                                                       surrogate.Exception,
                                                       surrogate.Started,
                                                       surrogate.Done);
        }

        /// <inheritdoc />
        public SequenceExecutorExecThreadStateSurrogate ConvertToSurrogate(in SequenceExecutorExecThreadState value)
        {
            return new SequenceExecutorExecThreadStateSurrogate()
            {
                FlowUid = value.FlowUid,
                FlowDefinitionId = value.FlowDefinitionId,
                CurrentStageExecId = value.CurrentStageExecId,
                ParentStageExecId = value.ParentStageExecId,
                Cursor = value.Cursor,
                ThreadInput = value.ThreadInput,
                Output = value.Output,
                InnerThreads = value.InnerThreads
                                   ?.Select(s => ConvertToSurrogate(s))
                                    .ToArray() ?? EnumerableHelper<SequenceExecutorExecThreadStateSurrogate>.ReadOnlyArray,
                Exception = value.Exception,
                Started = value.Started,
                Done = value.JobDone
            };
        }
    }
}
