// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed record class BlackboardTemplateConfigurationDefinition(bool InitializationRequired)
    {
        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static BlackboardTemplateConfigurationDefinition()
        {
            DefaultConfiguration = new BlackboardTemplateConfigurationDefinition(false);
        }

        /// <summary>
        /// Gets the default configuration.
        /// </summary>
        public static BlackboardTemplateConfigurationDefinition DefaultConfiguration { get; }
    }
}
