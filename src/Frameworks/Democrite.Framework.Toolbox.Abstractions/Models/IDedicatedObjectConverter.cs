// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Define a dedicated converter
    /// </summary>
    public interface IDedicatedObjectConverter
    {
        #region Properties

        /// <summary>
        /// Gets the source types managed
        /// </summary>
        IReadOnlyCollection<Type> ManagedSourceTypes { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to convert from <paramref name="obj"/> to type <paramref name="targetType"/>
        /// </summary>
        /// <returns>
        ///     <c>True</c> if the convertion successed
        /// </returns>
        bool TryConvert(object obj, Type targetType, out object? result);

        #endregion
    }
}
