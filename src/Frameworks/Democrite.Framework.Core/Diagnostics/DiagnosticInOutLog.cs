// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Diagnostics
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Elvex.Toolbox.Abstractions.Models;

    using System;
    using System.ComponentModel;
    using System.Text;

    /// <summary>
    /// Diagnostic log about before and after a virtual grain execution
    /// </summary>
    /// <seealso cref="DiagnosticBaseLog" />
    /// <seealso cref="IDiagnosticInOutLog" />
    [Immutable]
    [ImmutableObject(true)]
    [Serializable]
    [GenerateSerializer]
    public sealed class DiagnosticInOutLog : DiagnosticBaseLog, IDiagnosticInOutLog
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticInOutLog"/> class.
        /// </summary>
        public DiagnosticInOutLog(Guid? flowUID,
                                  Guid? currentExecutionId,
                                  Guid? callerId,
                                  OrientationEnum orientation,
                                  DateTime createOn,
                                  TypedArgument? inOut,
                                  string? error = null)
            : base(DiagnosticLogTypeEnum.InOutContext, flowUID, currentExecutionId, callerId, orientation, createOn)
        {
            this.InOut = inOut;
            this.Error = error;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the in out data in parameters or out result
        /// </summary>
        [Id(0)]
        public TypedArgument? InOut { get; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        [Id(1)]
        public string? Error { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Called to give more inline information
        /// </summary>
        protected override string OnDebugDisplayName()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(this.Error))
                builder.Append(string.Format("[Error] {0} ", this.Error));

            if (this.InOut != null)
                builder.Append(string.Format("[{1} [{0}] ", string.Join(", ", this.InOut.Flattern()), base.Orientation == OrientationEnum.In ? "Arg" : "Return"));

            return builder.ToString();
        }

        #endregion
    }
}
