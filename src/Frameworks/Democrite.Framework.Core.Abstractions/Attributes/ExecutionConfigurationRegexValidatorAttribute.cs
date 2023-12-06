// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Used regex to validate a configuration
    /// </summary>
    /// <seealso cref="ExecutionConfigurationValidatorAttribute{string}" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ExecutionConfigurationRegexValidatorAttribute : ExecutionConfigurationValidatorAttribute<string>
    {
        #region Fields

        private readonly Regex _regex;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionConfigurationRegexValidatorAttribute"/> class.
        /// </summary>
        public ExecutionConfigurationRegexValidatorAttribute(string regexPattern)
        {
            this._regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void OnValidate(string config, MethodInfo _)
        {
            if (!this._regex.IsMatch(config))
                throw new InvalidDataException("configuration '" + config + "' doesn't follow the regex rule " + this._regex.ToString());
        }

        #endregion
    }
}
