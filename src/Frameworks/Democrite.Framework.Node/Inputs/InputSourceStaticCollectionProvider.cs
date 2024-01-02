// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provider using a static collection declared by <see cref="InputSourceStaticCollectionDefinition{TInputType}"/> following mode <see cref="InputSourceStaticCollectionDefinition{TInputType}.PullMode"/>
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="IInputProvider" />
    public sealed class InputSourceStaticCollectionProvider<TInputType> : SafeDisposable, IInputProvider
    {
        #region Fields

        private readonly InputSourceStaticCollectionDefinition<TInputType> _definition;
        private readonly IInputProvider _inputProviderBaseOnPullMode;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceStaticCollectionProvider{TInputType}"/> class.
        /// </summary>
        public InputSourceStaticCollectionProvider(InputSourceStaticCollectionDefinition<TInputType> definition)
        {
            this._definition = definition;

            switch (definition.PullMode)
            {
                case PullModeEnum.Broadcast:
                    this._inputProviderBaseOnPullMode = new InputSourceStaticCollectionBroadcastSelector<TInputType>(definition);
                    break;

                case PullModeEnum.Random:
                    this._inputProviderBaseOnPullMode = new InputSourceStaticCollectionRandomSelector<TInputType>(definition);
                    break;

                case PullModeEnum.None:
                case PullModeEnum.Circling:
                default:
                    this._inputProviderBaseOnPullMode = new InputSourceStaticCollectionCirclingSelector<TInputType>(definition);
                    break;
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public AbstractType InputType
        {
            get { return this._definition.InputType; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<object?> GetNextAsync(CancellationToken token = default)
        {
            return this._inputProviderBaseOnPullMode.GetNextAsync(token);
        }

        /// <inheritdoc />
        public object? GetState()
        {
            return this._inputProviderBaseOnPullMode.GetState();
        }

        /// <inheritdoc />
        public ValueTask<bool> IsStillValidAsync(InputSourceDefinition inputSourceDefinition, CancellationToken token = default)
        {
            return this._inputProviderBaseOnPullMode.IsStillValidAsync(inputSourceDefinition, token);
        }

        /// <inheritdoc />
        public ValueTask RestoreStateAsync(object? state, CancellationToken token = default)
        {
            return this._inputProviderBaseOnPullMode.RestoreStateAsync(state, token);
        }

        /// <summary>
        /// Dispose pull mode implementation is case needed
        /// </summary>
        protected override void DisposeBegin()
        {
            if (this._inputProviderBaseOnPullMode is IDisposable disposable)
                disposable.Dispose();

            base.DisposeBegin();
        }

        #endregion
    }
}
