// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// Define mode to select values from a collection
    /// </summary>
    public enum PullModeEnum
    {
        None = 0,

        [Description("Each request pick the next one in infin loop")]
        Circling,

        [Description("At each request all values are use")]
        Broadcast,

        [Description("At each request a random value in the collection is pulled")]
        Random,
    }
}
