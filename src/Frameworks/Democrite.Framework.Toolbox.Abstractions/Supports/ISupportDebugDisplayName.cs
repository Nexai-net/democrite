// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Supports
{
    /// <summary>
    /// Support converting this instance into debuggable string
    /// </summary>
    public interface ISupportDebugDisplayName
    {
        /// <summary>
        /// Convert instance to string with debug information
        /// </summary>
        string ToDebugDisplayName();
    }
}
