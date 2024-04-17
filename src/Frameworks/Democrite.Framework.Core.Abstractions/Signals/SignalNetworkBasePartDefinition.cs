﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Elvex.Toolbox;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base Definition of any signal/signal
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract class SignalNetworkBasePartDefinition : IEquatable<SignalNetworkBasePartDefinition>, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNetworkBasePartDefinition"/> class.
        /// </summary>
        protected SignalNetworkBasePartDefinition(Guid uid,
                                                  string name,
                                                  string displayName,
                                                  string? group = null)
        {
            this.Uid = uid;
            this.Name = name;
            this.Group = group;
            this.DisplayName = displayName;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public Guid Uid { get; }

        /// <summary>
        /// Gets the unique name.
        /// </summary>
        [Id(1)]
        [DataMember]
        public string Name { get; }

        /// <inheritdoc />
        [Id(2)]
        [DataMember]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        [Id(3)]
        [DataMember]
        public string? Group { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            bool isValid = true;

            if (this.Uid == Guid.Empty)
            {
                logger.OptiLog(LogLevel.Critical, "Signal id MUST not be equals to Guid.Empty");
                isValid = false;
            }

            return isValid && OnValidate(logger, matchWarningAsError);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return GetType().Name + ":" + this.Name;
        }

        /// <inheritdoc cref="IDefinition.Validate(ILogger, bool)"/>
        protected abstract bool OnValidate(ILogger logger, bool matchWarningAsError = false);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.Name,
                                    this.DisplayName,
                                    this.Group,
                                    OnSignalGetHashCode());
        }

        /// <inheritdoc />
        public bool Equals(SignalNetworkBasePartDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.Uid == other.Uid &&
                   this.Name == other.Name &&
                   this.DisplayName == other.DisplayName &&
                   this.Group == other.Group &&
                   OnSignalEquals(other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is SignalNetworkBasePartDefinition other)
                return Equals(other);

            return false;
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        protected abstract bool OnSignalEquals([NotNull] SignalNetworkBasePartDefinition other);

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract int OnSignalGetHashCode();

        #endregion
    }
}
