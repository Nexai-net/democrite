// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;

    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    [DebuggerDisplay("Target {DisplayName}")]
    public sealed class TriggerTargetDefinition : IEquatable<TriggerTargetDefinition>, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerTargetDefinition"/> class.
        /// </summary>
        public TriggerTargetDefinition(Guid uid, TargetTypeEnum type, DataSourceDefinition? dedicatedDataProvider)
        {
            this.Uid = uid;
            this.Type = type;
            this.DedicatedDataProvider = dedicatedDataProvider;

            this.DisplayName = $"{this.Type}:{this.Uid}";
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        public string DisplayName { get; }

        /// <inheritdoc />
        [DataMember]
        public Guid Uid { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [DataMember]
        public TargetTypeEnum Type { get; }

        /// <summary>
        /// Gets the dedicated data provider.
        /// </summary>
        [DataMember]
        public DataSourceDefinition? DedicatedDataProvider { get; }

        #endregion

        #region Methods

        public bool Equals(TriggerTargetDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return other.Uid == this.Uid &&
                   other.Type == this.Type &&
                   (other.DedicatedDataProvider?.Equals(this.DedicatedDataProvider) ?? this.DedicatedDataProvider is null);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TriggerTargetDefinition);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Uid, this.Type, this.DedicatedDataProvider);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return this.DisplayName;
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
