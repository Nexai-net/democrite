// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes.MetaData
{
    using System;

    /// <summary>
    /// Tag a code element with a key used in external system 
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class VGrainMetaDataTagAttibute : Attribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMetaDataTagAttibute"/> class.
        /// </summary>
        public VGrainMetaDataTagAttibute(string tag)
        {
            this.Tag = tag;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the tag key.
        /// </summary>
        public string Tag { get; }

        #endregion
    }
}
