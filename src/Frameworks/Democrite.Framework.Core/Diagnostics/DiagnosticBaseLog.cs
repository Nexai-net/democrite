// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System.ComponentModel;

    /// <summary>
    /// Base class of all diagnostic log
    /// </summary>
    [Immutable]
    [ImmutableObject(true)]
    [Serializable]
    [GenerateSerializer]
    public abstract class DiagnosticBaseLog : IDiagnosticLog
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticCallLog"/> class.
        /// </summary>
        public DiagnosticBaseLog(DiagnosticLogTypeEnum type,
                                 Guid? flowUid,
                                 Guid? currentExecutionId,
                                 Guid? callerId,
                                 OrientationEnum orientation,
                                 DateTime createOn)
        {
            this.Type = type;
            this.FlowUID = flowUid ?? Guid.Empty;
            this.CurrentExecutionId = currentExecutionId ?? Guid.Empty;
            this.CallerId = callerId ?? Guid.Empty;
            this.CreateOn = createOn;
            this.Orientation = orientation;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        public DiagnosticLogTypeEnum Type { get; }

        /// <inheritdoc />
        [Id(1)]
        public Guid FlowUID { get; }

        /// <inheritdoc />
        [Id(2)]
        public Guid CurrentExecutionId { get; }

        /// <inheritdoc />
        [Id(3)]
        public OrientationEnum Orientation { get; }

        /// <inheritdoc />
        [Id(4)]
        public Guid? CallerId { get; }

        /// <inheritdoc />
        [Id(5)]
        public DateTime CreateOn { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return string.Format("[{0}][{1}][Flow '{2}'] - [Caller = {5}, Current = {3}]  - {4}",
                                 this.Type,
                                 this.Orientation,
                                 this.FlowUID,
                                 this.CurrentExecutionId,
                                 OnDebugDisplayName(),
                                 this.CallerId);
        }

        /// <summary>
        /// Called to give more inline information
        /// </summary>
        protected abstract string OnDebugDisplayName();

        /// <inheritdoc />
        public override string ToString()
        {
            return ToDebugDisplayName();
        }

        #endregion
    }
}
