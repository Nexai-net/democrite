// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Sequences
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Diagnostics;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Group all the stage execution logs
    /// </summary>
    public sealed class FlowStage
    {
        #region Fields

        private readonly List<FlowStage> _children;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowStage"/> class.
        /// </summary>
        private FlowStage(Guid currentExecId,
                          Guid? parentStageId,
                          IEnumerable<IDiagnosticLog> logs)
        {
            this.CurrentExecId = currentExecId;
            this.ParentStageId = parentStageId;

            foreach (var log in logs)
            {
                if (log is DiagnosticInOutLog inOut)
                {
                    if (inOut.Orientation == OrientationEnum.In)
                        this.Parameters = inOut;
                    else
                        this.ReturnArg = inOut;
                    continue;
                }

                if (log is DiagnosticCallLog call)
                {
                    if (call.Orientation == OrientationEnum.In)
                        this.Call = call;
                    else
                        this.Outgoing = call;
                    continue;
                }

                if (log is ExecutionCursorDiagnosticLog cursor)
                {
                    this.DefinitionCursorId = cursor.Cursor;
                    continue;
                }
            }

            this.AllLogs = logs?.ToReadOnly() ?? EnumerableHelper<IDiagnosticLog>.ReadOnly;

            this._children = new List<FlowStage>();
            this.Children = new ReadOnlyCollection<FlowStage>(this._children);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current execute identifier.
        /// </summary>
        public Guid CurrentExecId { get; }

        /// <summary>
        /// Gets the parent stage identifier.
        /// </summary>
        public Guid? ParentStageId { get; }

        /// <summary>
        /// Gets the definition stage identifier.
        /// </summary>
        public Guid? DefinitionCursorId { get; }

        /// <summary>
        /// Gets the call parameters.
        /// </summary>
        public DiagnosticInOutLog? Parameters { get; }

        /// <summary>
        /// Gets the call information.
        /// </summary>
        public DiagnosticCallLog? Call { get; }

        /// <summary>
        /// Gets the return argument.
        /// </summary>
        public DiagnosticInOutLog? ReturnArg { get; }

        /// <summary>
        /// Gets the outgoing.
        /// </summary>
        public DiagnosticCallLog? Outgoing { get; }

        /// <summary>
        /// Gets the other logs.
        /// </summary>
        public IReadOnlyCollection<IDiagnosticLog> AllLogs { get; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public FlowStage? Parent { get; private set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        public IReadOnlyCollection<FlowStage> Children { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a <see cref="FlowStage"/> from <paramref name="logs"/>
        /// </summary>
        public static IReadOnlyCollection<FlowStage> Create(IReadOnlyCollection<IDiagnosticLog> logs)
        {
            return logs.OrderBy(l => l.Orientation)
                       .ThenBy(l => l.CreateOn)
                       .GroupBy(l => l.CallerId ?? Guid.Empty)
                       .SelectMany(vv => vv.GroupBy(v => v.CurrentExecutionId)
                                           .Select(v => new FlowStage(v.Key, vv.Key, v.ToReadOnly())))
                       .ToReadOnly();
        }

        /// <summary>
        /// Adds new children stage
        /// </summary>
        internal void AddChildren(FlowStage newStage)
        {
            ArgumentNullException.ThrowIfNull(newStage);

            newStage.Parent = this;
            this._children.Add(newStage);
        }

        #endregion
    }
}
