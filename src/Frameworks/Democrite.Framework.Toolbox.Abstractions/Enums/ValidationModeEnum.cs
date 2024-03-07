// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Enums
{
    /// <summary>
    /// Define the type of logical validation between validator of the same group collection
    /// </summary>
    public enum ValidationModeEnum
    {
        None = 0,
        All,
        AtLeastOne,
        NoOne
    }
}
