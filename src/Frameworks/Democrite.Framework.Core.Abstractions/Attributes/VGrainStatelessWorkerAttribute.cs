// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Define the grain as stateless worker used by orlean and democrite to optimize the usage
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class VGrainStatelessWorkerAttribute : Attribute
    {
    }
}
