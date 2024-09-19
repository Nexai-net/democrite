// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes.MetaData
{
    using Democrite.Framework.Core.Abstractions.References;

    using System;

    /// <summary>
    /// Assembly attribute charge to populate the registry with assembly information
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public abstract class DemocriteReferenceProviderAttribute : Attribute
    {
        /// <summary>
        /// Populates the specified registry with assemblies references
        /// </summary>
        public abstract void Populate(DemocriteReferenceRegistry registry);
    }
}
