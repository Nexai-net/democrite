// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Attribute used to tag needed vgrain by democrite to work
    /// </summary>
    /// <remarks>
    ///     Use to protected the vgrain from removing on vgrain declaration customization <see cref="Configurations.ClusterNodeVGrainBuilder"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public sealed class DemocriteSystemVGrainAttribute : Attribute
    {
    }
}
