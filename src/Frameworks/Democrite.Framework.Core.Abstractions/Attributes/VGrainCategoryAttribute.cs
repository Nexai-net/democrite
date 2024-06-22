// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// CategoryPath value used to enhanced the grain meta-data
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public sealed class VGrainCategoryAttribute : Attribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainCategoryAttribute"/> class.
        /// </summary>
        public VGrainCategoryAttribute(string category)
        {
            this.Category = category;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the category.
        /// </summary>
        public string Category { get; }

        #endregion
    }
}
