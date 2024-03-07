// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Inputs;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provider using a static collection declared by <see cref="DataSourceStaticCollectionDefinition{TInputType}"/> following mode <see cref="DataSourceStaticCollectionDefinition{TInputType}.PullMode"/>
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="IDataSourceProvider" />
    public sealed class DataSourceStaticCollectionProxyProvider<TInputType> : DataSourceBaseProvider<DataSourceStaticCollectionDefinition<TInputType>>, IDataSourceProvider
    {
        #region Fields

        private readonly IDataSourceProvider _inputProviderBaseOnPullMode;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceStaticCollectionProxyProvider{TInputType}"/> class.
        /// </summary>
        public DataSourceStaticCollectionProxyProvider(DataSourceStaticCollectionDefinition<TInputType> definition)
             : base(definition)
        {
            switch (definition.PullMode)
            {
                case PullModeEnum.Broadcast:
                    this._inputProviderBaseOnPullMode = new DataSourceStaticCollectionBroadcastSelector<TInputType>(definition);
                    break;

                case PullModeEnum.Random:
                    this._inputProviderBaseOnPullMode = new DataSourceStaticCollectionRandomSelector<TInputType>(definition);
                    break;

                case PullModeEnum.None:
                case PullModeEnum.Circling:
                default:
                    this._inputProviderBaseOnPullMode = new DataSourceStaticCollectionCirclingSelector<TInputType>(definition);
                    break;
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override bool UseState
        {
            get { return this._inputProviderBaseOnPullMode.UseState; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ValueTask<object?> GetNextAsync(object? sourceValue, CancellationToken token = default)
        {
            return this._inputProviderBaseOnPullMode.GetNextAsync(sourceValue, token);
        }

        /// <inheritdoc />
        public override object? GetState()
        {
            if (this.UseState)
                return this._inputProviderBaseOnPullMode.GetState();
            return null;
        }

        /// <inheritdoc />
        public override ValueTask<bool> IsStillValidAsync(DataSourceDefinition inputSourceDefinition, CancellationToken token = default)
        {
            return this._inputProviderBaseOnPullMode.IsStillValidAsync(inputSourceDefinition, token);
        }

        /// <inheritdoc />
        public override ValueTask RestoreStateAsync(object? state, CancellationToken token = default)
        {
            if (this._inputProviderBaseOnPullMode.UseState)
                return this._inputProviderBaseOnPullMode.RestoreStateAsync(state, token);
            return ValueTask.CompletedTask;
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
