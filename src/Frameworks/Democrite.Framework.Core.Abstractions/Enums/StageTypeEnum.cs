// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Enums
{
    /// <summary>
    /// Stage type
    /// </summary>
    public enum StageTypeEnum
    {
        None,
        Call,
        Foreach,
        Filter,
        Convert,
        PushToContext,
        FireSignal,
        Select,
        NestedSequenceCall
    }
}
