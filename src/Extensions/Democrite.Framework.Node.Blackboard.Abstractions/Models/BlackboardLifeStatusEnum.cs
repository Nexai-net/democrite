// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    /// <summary>
    /// 
    /// </summary>
    public enum BlackboardLifeStatusEnum
    {
        /// <summary>
        /// The blackboard board will be automatically initialized
        /// </summary>
        None,

        /// <summary>
        /// Wait for an external initialization, all action except initialization will be refused.
        /// </summary>
        WaitingInitialization,

        /// <summary>
        /// In processing
        /// </summary>
        Running,

        /// <summary>
        /// Define the blackboard objectif have been reached.
        /// </summary>
        Done,

        /// <summary>
        /// Blackboard sealed keep the requested information but refused any modification
        /// </summary>
        Sealed,
    }
}
