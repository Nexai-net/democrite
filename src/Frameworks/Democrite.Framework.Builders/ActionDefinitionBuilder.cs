// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Core.Abstractions;

    using System;

    /// <summary>
    /// Relay <typeparamref name="TDefinition"/> build to a external delegate
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    /// <seealso cref="Democrite.Framework.Builders.IDefinitionBaseBuilder&lt;TDefinition&gt;" />
    public sealed class ActionDefinitionBuilder<TDefinition> : IDefinitionBaseBuilder<TDefinition>
        where TDefinition : IDefinition
    {
        #region Fields
        
        private readonly Func<TDefinition> _builder;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDefinitionBuilder{TDefinition}"/> class.
        /// </summary>
        public ActionDefinitionBuilder(Func<TDefinition> builder)
        {
            this._builder = builder;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TDefinition Build()
        {
            return this._builder();
        }

        #endregion
    }
}
