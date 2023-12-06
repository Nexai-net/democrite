// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Express information about configuration that need to be pass through the <see cref="IExecutionContext{TConfiguration}"/>
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ExecutionConfigurationValidatorAttribute<TConfig> : Attribute, IExecutionContextConfigurationValidator<TConfig>
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether default value is allowed
        /// </summary>
        public bool AllowDefault { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Validate(TConfig? config, MethodInfo info)
        {
            if (DefaultValueCheck<TConfig>(config, info))
                return;

            OnValidate(config!, info);
        }

        /// <inheritdoc />
        public virtual void Validate(object? config, MethodInfo info)
        {
            if (DefaultValueCheck<object>(config, info))
                return;

            if (config is TConfig cfg)
            {
                Validate(cfg, info);
                return;
            }

            throw new InvalidCastException(string.Format("Invalid configuration type received '{0}' expect '{1}'", config!.GetType(), typeof(TConfig)));
        }

        #region Tools

        /// <summary>
        /// Defaults the value check.
        /// </summary>
        /// <exception cref="InvalidDataException"><paramref name="info"/> Method Default configuration value is not allowed</exception>
        private bool DefaultValueCheck<TCheckType>(TCheckType? config, MethodInfo info)
        {
            if (EqualityComparer<TCheckType>.Default.Equals(config, default))
            {
                if (!this.AllowDefault)
                    throw new InvalidDataException(info.Name + ": Default configuration value is not allowed");
                return true;
            }
            return false;
        }

        /// <inheritdoc cref="IExecutionContextConfigurationValidator{TConfig}.Validate(TConfig?, MethodInfo)" />
        protected abstract void OnValidate(TConfig config, MethodInfo info);

        #endregion

        #endregion
    }
}
