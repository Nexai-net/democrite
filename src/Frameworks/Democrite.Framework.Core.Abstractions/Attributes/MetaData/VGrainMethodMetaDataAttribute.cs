// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes.MetaData
{
    using System;

    /// <summary>
    /// Attribute used to flag a VGrain method and describe is main goal.
    /// This description will be used as meta-data in the analytics algorithm and wysiwyg interface (like SyDE studio)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class VGrainMetaDataMethodAttribute : RefSimpleNameIdentifierAttribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMetaDataAttribute"/> class.
        /// </summary>
        public VGrainMetaDataMethodAttribute(string simpleNameIdentifier, string? displayName = null, string? description = null)
            : base(simpleNameIdentifier, null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(simpleNameIdentifier);

            this.DisplayName = displayName;
            this.Description = description;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string? Description { get; }

        #endregion
    }
}
