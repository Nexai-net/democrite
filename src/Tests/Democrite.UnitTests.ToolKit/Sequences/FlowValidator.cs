// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Sequences
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Extensions;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Container of all the step information about flow
    /// </summary>
    public sealed class FlowValidator
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowValidator"/> class.
        /// </summary>
        private FlowValidator(IReadOnlyCollection<FlowStage> stages)
        {
            this.Roots = stages?.Where(s => s.Parent == null).ToArray() ?? EnumerableHelper<FlowStage>.ReadOnlyArray;
            this.Stages = stages?.ToArray() ?? EnumerableHelper<FlowStage>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets stage roots.
        /// </summary>
        public IReadOnlyCollection<FlowStage> Roots { get; }

        /// <summary>
        /// Gets all stages.
        /// </summary>
        public IReadOnlyCollection<FlowStage> Stages { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Analyze logs and provide a <see cref="FlowValidator"/> by flow
        /// </summary>
        public static IReadOnlyCollection<FlowValidator> From(IReadOnlyCollection<IDiagnosticLog>? logs)
        {
            if (logs == null)
                return EnumerableHelper<FlowValidator>.ReadOnly;

#if DEBUG
            var logDebugString = logs.ToDebugDisplayName();
            Debug.WriteLine(logDebugString);
#endif

            var indexByExeId = logs.GroupBy(l => l.FlowUID)
                                   .ToDictionary(k => k.Key,
                                                 l => FlowStage.Create(l.ToReadOnly()).ToReadOnly());

            return indexByExeId.Select(kvFlow => FlowHierachy(kvFlow.Value)).ToArray();
        }

        /// <summary>
        /// Get a dedicated stage hierarchy
        /// </summary>
        private static FlowValidator FlowHierachy(IReadOnlyCollection<FlowStage> stages)
        {
            var indexedChildrenByCaller = stages.GroupBy(s => s.ParentStageId ?? Guid.Empty)
                                                .ToDictionary(k => k.Key, v => v.ToReadOnly());

            FlowHierachy(null, indexedChildrenByCaller);

            return new FlowValidator(stages);
        }

        /// <summary>
        /// Get a dedicated stage hierarchy
        /// </summary>
        private static void FlowHierachy(FlowStage? parent,
                                         IReadOnlyDictionary<Guid, IReadOnlyCollection<FlowStage>> indexedChildrenByParent)
        {
            if (indexedChildrenByParent.TryGetValue(parent?.CurrentExecId ?? Guid.Empty, out var children))
            {
                foreach (var child in children)
                {
                    parent?.AddChildren(child);
                    FlowHierachy(child, indexedChildrenByParent);
                }
            }
        }

        #endregion
    }
}
