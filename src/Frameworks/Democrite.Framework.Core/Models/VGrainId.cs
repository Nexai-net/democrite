// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;

    /// <summary>
    /// Simple <see cref="IVGrainId"/>
    /// </summary>
    /// <seealso cref="IVGrainId" />
    internal sealed class VGrainId : IVGrainId
    {
        #region Fields

        private readonly object _primaryKey;
        private readonly string? _keyExtension;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainId"/> class.
        /// </summary>
        public VGrainId(Type vgrainType,
                       IdFormatTypeEnum idFormatType,
                       object primaryKey,
                       string? keyExtension = null)
        {
            this.VGrainType = vgrainType;
            this._primaryKey = primaryKey;
            this._keyExtension = keyExtension;
            this.IdFormatType = idFormatType;
        }
        #endregion

        #region Properties

        /// <inheritdoc />
        public IdFormatTypeEnum IdFormatType { get; }

        /// <inheritdoc />
        public Type VGrainType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string? GetVGrainIdExtensionParameter()
        {
            return this._keyExtension;
        }

        /// <inheritdoc />
        public TParam GetVGrainIdPrimaryValue<TParam>()
        {
            if (this._primaryKey is TParam primary)
                return primary;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            if (typeof(TParam) == typeof(string))
                return (TParam)(object)this._primaryKey?.ToString();
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            throw new InvalidCastException("Primary key type " + this._primaryKey.GetType() + " coul be cast to type " + typeof(TParam));
        }

        #endregion
    }
}
