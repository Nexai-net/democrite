// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Express information about configuration that need to be pass through the <see cref="IExecutionContext{TConfiguration}"/>
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ExecutionConfigurationInfoAttribute : Attribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionConfigurationInfoAttribute"/> class.
        /// </summary>
        public ExecutionConfigurationInfoAttribute(string description)
        {
            this.Description = description;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        #endregion
    }
}
