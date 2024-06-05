// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBlackboardTemplateOptionsBuilder
    {
        /// <summary>
        /// Initializations is required.
        /// </summary>
        IBlackboardTemplateOptionsBuilder InitializationRequired();

        /// <summary>
        /// Tolerate only one record by logical type that match the configuration
        /// </summary>
        /// <param name="replace">Allow replacement if id differ</param>
        IBlackboardTemplateOptionsBuilder AllLogicalTypeUnique(bool replace = false);
    }
}
