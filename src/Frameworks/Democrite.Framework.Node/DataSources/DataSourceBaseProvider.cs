// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Models;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class for all the data source provider
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="IDataSourceProvider" />
    public abstract class DataSourceBaseProvider<TDefinition> : SafeDisposable, IDataSourceProvider
        where TDefinition : DataSourceDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceStaticCollectionProvider{TInputType}"/> class.
        /// </summary>
        protected DataSourceBaseProvider(TDefinition definition)
        {
            this.Definition = definition;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public AbstractType DataType
        {
            get { return this.Definition.DataType; }
        }

        /// <inheritdoc />
        public virtual bool UseState
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        protected TDefinition Definition { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public abstract ValueTask<object?> GetNextAsync(object? sourceValue, CancellationToken token = default);

        /// <inheritdoc />
        public virtual object? GetState()
        {
            return null;
        }

        /// <inheritdoc />
        public virtual ValueTask<bool> IsStillValidAsync(DataSourceDefinition dataSourceDefinition, CancellationToken token = default)
        {
            return ValueTask.FromResult(dataSourceDefinition is not null && this.Definition.Equals(dataSourceDefinition));
        }

        /// <inheritdoc />
        public virtual ValueTask RestoreStateAsync(object? state, CancellationToken token = default)
        {
            return ValueTask.CompletedTask;
        }

        #endregion
    }
}
