// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Attributes
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;

    using System;

    /// <summary>
    /// Link an auto configurator process
    /// </summary>
    public abstract class AutoConfiguratorAttribute : Attribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoConfiguratorAttribute"/> class.
        /// </summary>
        protected AutoConfiguratorAttribute(Type autoConfigService, Type implementation)
        {
            this.AutoConfigService = autoConfigService;
            this.Implementation = implementation;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the automatic configuration service.
        /// </summary>
        public Type AutoConfigService { get; }

        /// <summary>
        /// Gets the implementation.
        /// </summary>
        public Type Implementation { get; }

        #endregion
    }

    /// <summary>
    /// Link an auto configurator process, defined by <typeparamref name="TAutoConfiguratorService"/>, to a local implementation <typeparamref name="TImplementation"/>
    /// </summary>
    /// <typeparam name="TAutoConfiguratorService">The type of the automatic configurator service.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AutoConfiguratorAttribute<TAutoConfiguratorService, TImplementation> : AutoConfiguratorAttribute
        where TAutoConfiguratorService : IAutoConfigurator
        where TImplementation : class, TAutoConfiguratorService, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoConfiguratorAttribute{TAutoConfiguratorService, TImplementation}"/> class.
        /// </summary>
        public AutoConfiguratorAttribute()
            : base(typeof(TAutoConfiguratorService), typeof(TImplementation))
        {

        }
    }
}
