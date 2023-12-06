// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Contain all information used to trace vgrain input and vgrain output
    /// </summary>
    [Immutable]
    [Serializable]
    [ImmutableObject(true)]
    [GenerateSerializer]
    public class DiagnosticCallLog : DiagnosticBaseLog, IDiagnosticCallLog
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticCallLog"/> class.
        /// </summary>
        public DiagnosticCallLog(Guid? flowUid,
                                 Guid? currentExecutionId,
                                 Guid? callerId,
                                 string? sourceInstanceId,
                                 string? targetInstanceId,
                                 string? targetVGrainType,
                                 string? targetMethod,
                                 OrientationEnum orientation,
                                 DateTime createOn,
                                 bool isOrleanSystem,
                                 bool isDemocriteSystem,
                                 NodeInfo siloInfo)
            : base(DiagnosticLogTypeEnum.Call, flowUid, currentExecutionId, callerId, orientation, createOn)
        {
            this.SourceInstanceId = sourceInstanceId;

            this.TargetInstanceId = targetInstanceId;
            this.TargetVGrainType = targetVGrainType;

            this.TargetMethod = targetMethod;
            this.IsOrleanSystem = isOrleanSystem;
            this.IsDemocriteSystem = isDemocriteSystem;
            this.SiloInfo = siloInfo;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the full source instance identifier seialized.
        /// </summary>
        [Id(0)]
        public string? SourceInstanceId { get; }

        /// <summary>
        /// Gets the full target instance identifier seialized.
        /// </summary>
        [Id(1)]
        public string? TargetInstanceId { get; }

        /// <summary>
        /// Gets the type of the target vgrain.
        /// </summary>
        [Id(2)]
        public string? TargetVGrainType { get; }

        /// <summary>
        /// Gets the target method.
        /// </summary>
        [Id(3)]
        public string? TargetMethod { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is use by orlean system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is orlean system; otherwise, <c>false</c>.
        /// </value>
        [Id(4)]
        public bool IsOrleanSystem { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is use by democrite system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is democrite system; otherwise, <c>false</c>.
        /// </value>
        [Id(5)]
        public bool IsDemocriteSystem { get; }

        /// <summary>
        /// Gets the silo information.
        /// </summary>
        [Id(6)]
        public NodeInfo SiloInfo { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Called to give more inline information
        /// </summary>
        protected override string OnDebugDisplayName()
        {
            return string.Format("[{0}][{1}] - ({3}) {2}", this.IsOrleanSystem, this.IsDemocriteSystem, this.TargetMethod, this.TargetVGrainType);
        }

        #endregion
    }
}
