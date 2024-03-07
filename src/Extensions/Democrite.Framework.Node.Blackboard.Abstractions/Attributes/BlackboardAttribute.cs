// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Define an attribute to inject a <see cref="IBlackboardRef"/> base on provided key
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="IFacetMetadata" />
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class BlackboardAttribute : Attribute, IFacetMetadata
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardAttribute"/> class.
        /// </summary>
        public BlackboardAttribute(string boardName, string? boardTemplateConfigurationKey = null) 
        {
            this.BoardName = boardName;
            this.BoardTemplateConfigurationKey = string.IsNullOrEmpty(boardTemplateConfigurationKey)
                                                        ? BlackboardConstants.DefaultBoardTemplateConfigurationKey
                                                        : boardTemplateConfigurationKey;        
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the board name identifier.
        /// </summary>
        public string BoardName { get; }

        /// <summary>
        /// Gets the board template configuration name.
        /// </summary>
        public string BoardTemplateConfigurationKey { get; }

        #endregion
    }
}
