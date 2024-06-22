// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence
{
    using Elvex.Toolbox;

    using Microsoft.Extensions.Logging;

    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Sequence option realted to diagnostic
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceDiagnosticOptionDefinition : IEquatable<SequenceDiagnosticOptionDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SequenceDiagnosticOptionDefinition"/> class.
        /// </summary>
        static SequenceDiagnosticOptionDefinition()
        {
            Default = new SequenceDiagnosticOptionDefinition(Debugger.IsAttached, LogLevel.Information);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDiagnosticOptionDefinition"/> class.
        /// </summary>
        public SequenceDiagnosticOptionDefinition(bool saveAllStageInfo, LogLevel minLogLevel)
        {
            this.SaveAllStageInfo = saveAllStageInfo;
            this.MinLogLevel = minLogLevel;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static SequenceDiagnosticOptionDefinition Default { get; }

        /// <summary>
        /// Gets or sets a value indicating whether save all stage information will be save for diagnostic purpose.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [save all stage information]; otherwise, <c>false</c>.
        /// </value>
        [Id(0)]
        [DataMember]
        public bool SaveAllStageInfo { get; }

        /// <summary>
        /// Gets the minimum log level.
        /// </summary>
        [Id(1)]
        [DataMember]
        public LogLevel MinLogLevel { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(SequenceDiagnosticOptionDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.SaveAllStageInfo == other.SaveAllStageInfo &&
                   this.MinLogLevel == other.MinLogLevel;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is SequenceDiagnosticOptionDefinition def)
                return Equals(def);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.SaveAllStageInfo, this.MinLogLevel);
        }

        #endregion
    }
}
