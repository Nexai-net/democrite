// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    [GenerateSerializer]
    public class DefaultBlackboardControllerState
    {
        /// <summary>
        /// Gets or sets the option.
        /// </summary>
        [Id(0)]
        public DefaultControllerOptions? Option { get; set; }   
    }
}
