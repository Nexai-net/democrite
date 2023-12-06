// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Define a key that all assembly auto configurator define by <see cref="AutoConfiguratorAttribute"/> are attached
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AutoConfiguratorKeyAttribute : Attribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoConfiguratorKeyAttribute"/> class.
        /// </summary>
        public AutoConfiguratorKeyAttribute(string key)
        {
            this.Key = key;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the auto configuration key associate to this assembly.
        /// </summary>
        public string Key { get; }

        #endregion
    }
}
