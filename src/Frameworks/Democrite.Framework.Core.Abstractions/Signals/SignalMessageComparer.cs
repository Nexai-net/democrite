// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Compare <see cref="SignalMessage"/>
    /// </summary>
    /// <seealso cref="IEqualityComparer{SignalMessage}" />
    public sealed class SignalMessageComparer : IEqualityComparer<SignalMessage>, IEqualityComparer
    {
        #region Fields

        private readonly bool _compareOnlyDefinition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessageComparer"/> class.
        /// </summary>
        static SignalMessageComparer()
        {
            Default = new SignalMessageComparer(false);
            OnlyDefinition = new SignalMessageComparer(true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessageComparer"/> class.
        /// </summary>
        private SignalMessageComparer(bool compareOnlyDefinition)
        {
            this._compareOnlyDefinition = compareOnlyDefinition;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static SignalMessageComparer Default { get; }

        /// <summary>
        /// Gets a comparer that match only the <see cref="SignalMessage.From"/> definition.
        /// </summary>
        public static SignalMessageComparer OnlyDefinition { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(SignalMessage? x, SignalMessage? y)
        {
            if (!this._compareOnlyDefinition)
                return x == y;

            if (x is null && y is null)
                return true;

            if (object.ReferenceEquals(x, y))
                return true;

            return x is not null &&
                   y is not null &&
                   x.From.SourceDefinitionId == y.From.SourceDefinitionId &&
                   x.From.SourceDefinitionName == y.From.SourceDefinitionName;
        }

        /// <inheritdoc />
        public new bool Equals(object? x, object? y)
        {
            return Equals(x as SignalMessage, y as SignalMessage);
        }

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] SignalMessage obj)
        {
            if (!this._compareOnlyDefinition)
                return obj.GetHashCode();

            return HashCode.Combine(obj.From.SourceDefinitionId, obj.From.SourceDefinitionName);
        }

        /// <inheritdoc />
        public int GetHashCode(object obj)
        {
            if (obj is SignalMessage signalMessage)
                return GetHashCode(signalMessage);

            throw new InvalidCastException(nameof(SignalMessageComparer) + " only managed " + nameof(SignalMessage) + " equality comparaison");
        }

        #endregion
    }
}
