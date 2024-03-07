// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;

    /// <summary>
    /// Specific how vgrain id must be formated
    /// </summary>
    /// <remarks>
    ///     Template : <br />
    ///     - "<b>{new}</b>" : Generate unique value from Guid.NewUid() <br />
    ///     - "<b>{input.Property}</b>" : extract value exposed by input object; if null use fallback value <br />
    ///     - "<b>{executionContext.Property}</b>" : extract value exposed by execution context object; if null use fallback value <br />
    ///     - "<b>{executionContext.Configuration.Property}</b>" : extract value exposed by configuration contains in execution context object; if null use fallback value <br />
    ///     - "<b>FIX_VALUE</b>" : apply fix value<br />
    ///     <br />
    ///     If no template are provide then the template "{new}" is used by <c>Defaultt.</c>
    /// </remarks>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class VGrainIdFormatAttribute : VGrainIdBaseFormatorAttribute
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainIdFormatAttribute"/> class.
        /// </summary>
        static VGrainIdFormatAttribute()
        {
            Default = new VGrainIdFormatAttribute(IdFormatTypeEnum.Guid)
            {
                FirstParameterTemplate = "{new}"
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdFormatAttribute"/> class.
        /// </summary>
        public VGrainIdFormatAttribute(IdFormatTypeEnum formatType)
            : base(formatType)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default <see cref="IdFormatTypeEnum.Guid"/> - '{new}'
        /// </summary>
        public static VGrainIdFormatAttribute Default { get; }

        /// <summary>
        /// Gets the first parameter template.
        /// </summary>
        public string? FirstParameterTemplate { get; init; }

        /// <summary>
        /// Gets the first parameter fallback.
        /// </summary>
        public string? FirstParameterFallback { get; init; }

        /// <summary>
        /// Gets the second parameter template.
        /// </summary>
        public string? SecondParameterTemplate { get; init; }

        /// <summary>
        /// Gets the second parameter fallback.
        /// </summary>
        public string? SecondParameterFallback { get; init; }

        #endregion
    }
}
