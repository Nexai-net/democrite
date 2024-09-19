// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes.MetaData
{
    using System;

    /// <summary>
    /// Attribute used to flag the VGrain and describe is main goal.
    /// This description will be used as meta-data in the analytics algorithm and wysiwyg interface (like SyDE studio)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class VGrainMetaDataAttribute : RefSimpleNameIdentifierAttribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMetaDataAttribute"/> class.
        /// </summary>
        public VGrainMetaDataAttribute(string Uid,
                                       string simpleNameIdentifier,            
                                       string? namespaceIdentifier = null,
                                       string? displayName = null,
                                       string? description = null,
                                       string? categoryPath = null)
            : base(simpleNameIdentifier, namespaceIdentifier)
        {
            if (Guid.TryParse(Uid, out var guid))
                this.Uid = guid;
            else
                throw new Exception("The id of VGrainMetaDataAttribute must be a GUID");

            ArgumentNullException.ThrowIfNullOrWhiteSpace(simpleNameIdentifier);
            
            this.DisplayName = displayName;
            this.Description = description;
            this.CategoryPath = categoryPath;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public Guid Uid { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the categoryPath path.
        /// </summary>
        public string? CategoryPath { get; }    

        #endregion
    }
}
