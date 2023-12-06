// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System.Reflection;

    /// <summary>
    /// Validator used to ensure the configuration provide in the execution context is correct
    /// </summary>
    public interface IExecutionContextConfigurationValidator
    {
        /// <summary>
        /// Validates the specified configuration.
        /// </summary>
        /// <exception cref="InvalidDataException">Raised if configurationrule is not respected.</exception>
        void Validate(object? config, MethodInfo info);
    }

    /// <summary>
    /// Validator used to ensure the configuration provide in the execution context is correct
    /// </summary>
    public interface IExecutionContextConfigurationValidator<TConfig> : IExecutionContextConfigurationValidator
    {
        /// <summary>
        /// Validates the specified configuration.
        /// </summary>
        /// <exception cref="InvalidDataException">Raised if configurationrule is not respected.</exception>
        void Validate(TConfig? config, MethodInfo info);
    }
}
