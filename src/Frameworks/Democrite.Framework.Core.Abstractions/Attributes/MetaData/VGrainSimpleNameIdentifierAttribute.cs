﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes.MetaData
{
    using System;

    /// <summary>
    /// Tag to provide information to easy item identification
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public class VGrainSimpleNameIdentifierAttribute : Attribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainSimpleNameIdentifierAttribute"/> class.
        /// </summary>
        public VGrainSimpleNameIdentifierAttribute(string simpleNameIdentifier, string? namespaceIdentifier = null)
        {
            this.SimpleNameIdentifier = simpleNameIdentifier;
            this.NamespaceIdentifier = namespaceIdentifier;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the simple name identifier.
        /// </summary>
        public string SimpleNameIdentifier { get; }

        /// <summary>
        /// Gets the namespace identifier.
        /// </summary>
        public string? NamespaceIdentifier { get; }

        #endregion
    }
}