// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Define a signal used to send signal and trigger other logical part.
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SignalMessage : IEquatable<SignalMessage>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessage"/> class.
        /// </summary>
        public SignalMessage(Guid uid,
                             DateTime sendUtcTime,
                             SignalSource from)
        {
            this.Uid = uid;
            this.From = from;
            this.SendUtcTime = sendUtcTime;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets unique identifier used to trace the events
        /// </summary>
        [Id(0)]
        public Guid Uid { get; }

        /// <summary>
        /// Gets information about the vgrain that fire the signal
        /// </summary>
        [Id(1)]
        public SignalSource From { get; }

        /// <summary>
        /// Gets UTC time when the signal have been emit
        /// </summary>
        [Id(2)]
        public DateTime SendUtcTime { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(SignalMessage? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.Uid == other.Uid &&
                   this.From == other.From &&
                   this.SendUtcTime == other.SendUtcTime;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.From,
                                    this.SendUtcTime);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is SignalMessage otherSignal)
                return Equals(otherSignal);

            return false;
        }

        /// <inheritdoc />
        public static bool operator ==(SignalMessage? lhs, SignalMessage? rhs)
        {
            return lhs?.Equals(rhs) ?? lhs is null;
        }

        /// <inheritdoc />
        public static bool operator !=(SignalMessage? lhs, SignalMessage? rhs)
        {
            return !(lhs == rhs);
        }

        #endregion
    }
}
