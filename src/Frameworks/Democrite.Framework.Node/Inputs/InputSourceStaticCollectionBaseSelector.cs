// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Inputs;

    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of prodive input using static collection a source.
    /// PullMode define the child type
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="IInputProvider" />
    internal abstract class InputSourceStaticCollectionBaseSelector<TInputType> : IInputProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceStaticCollectionBaseSelector{TInputType}"/> class.
        /// </summary>
        public InputSourceStaticCollectionBaseSelector(InputSourceStaticCollectionDefinition<TInputType> definition)
        {
            this.Definition = definition;
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="InputSourceStaticCollectionDefinition{TInputType}.PullMode" />
        public PullModeEnum PullMode
        {
            get { return this.Definition.PullMode; }
        }

        /// <inheritdoc />
        public Type InputType
        {
            get { return this.Definition.InputType; }
        }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        protected InputSourceStaticCollectionDefinition<TInputType> Definition { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public abstract Task<object?> GetNextAsync(CancellationToken token = default);

        /// <inheritdoc />
        public virtual object? GetState()
        {
            return null;
        }

        /// <inheritdoc />
        public ValueTask<bool> IsStillValidAsync(InputSourceDefinition inputSourceDefinition, CancellationToken token = default)
        {
            bool result = object.ReferenceEquals(this.Definition, inputSourceDefinition) ||

                          (inputSourceDefinition is InputSourceStaticCollectionDefinition<TInputType> otherDefinition &&
                           this.PullMode == otherDefinition.PullMode &&
                           this.Definition.Collection.SequenceEqual(otherDefinition.Collection));

            return ValueTask.FromResult(result);
        }

        /// <inheritdoc />
        public virtual ValueTask RestoreStateAsync(object? state, CancellationToken token = default)
        {
            return ValueTask.CompletedTask;
        }

        #endregion
    }
}
