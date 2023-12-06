// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Generate surrogate struct to be able to manage serialization and restoration by CTOR
    /// </summary>
    /// <remarks>
    ///     Auto-Gen surrogate class through code generator Democrite.Framework.Generator if surrogate class doesn't exists
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AutoGenerateSerializerByCtorAttribute : Attribute
    {
    }
}
