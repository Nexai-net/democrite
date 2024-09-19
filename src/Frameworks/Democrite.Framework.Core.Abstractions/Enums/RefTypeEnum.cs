// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    public enum RefTypeEnum
    {
        None = 0,

        [Description("Sequence definition")]
        Sequence = 1,

        [Description("VGrain interface, inherite from IVGrain")]
        VGrain = 2,
        VGrainImplementation = 3,
        Trigger = 4,
        Door = 5,
        Signal = 6,
        Artifact = 7,
        StreamQueue = 8,
        BlackboardTemplate = 9,
        BlackboardController = 10,
        Type = 11,
        Method = 12,
        Other = 0xFFFFFF
    }
}
