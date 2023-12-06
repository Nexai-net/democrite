// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Enums;

    /// <summary>
    /// Unique virtual grain identifier used to create vgrain instance, reuse them or/and create persistant context
    /// </summary>
    /// <remarks>
    ///     Produce by <see cref="IVGrainIdFactory"/> following format descripte by <see cref="ExpectedVGrainIdFormatAttribute"/>
    /// </remarks>
    public interface IVGrainId
    {
        #region Properties

        /// <summary>
        /// Gets the identifier format type enum.
        /// </summary>
        IdFormatTypeEnum IdFormatType { get; }

        /// <summary>
        /// Gets the type of the vgrain.
        /// </summary>
        Type VGrainType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the vgrain identifier primary value.
        /// </summary>
        TParam GetVGrainIdPrimaryValue<TParam>();

        /// <summary>
        /// Gets the vgrain identifier extension parameter.
        /// </summary>
        string? GetVGrainIdExtensionParameter();

        #endregion
    }
}
