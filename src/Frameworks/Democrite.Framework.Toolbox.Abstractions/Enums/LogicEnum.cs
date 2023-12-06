// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Enums
{
    using Democrite.Framework.Toolbox.Abstractions.Attributes;

    /// <summary>
    /// Enum expositing logical relation
    /// </summary>
    public enum LogicEnum
    {
        None,

        [DescriptionWithAlias('&')]
        And,

        [DescriptionWithAlias('|')]
        Or,

        [DescriptionWithAlias('^')]
        ExclusiveOr,

        [DescriptionWithAlias('!')]
        Not
    }
}
