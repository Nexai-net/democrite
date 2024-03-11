// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Elvex.Toolbox.Models;

    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of prodive input using static collection a source.
    /// PullMode define the child type
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="IDataSourceProvider" />
    internal abstract class DataSourceStaticCollectionBaseSelector<TInputType> : DataSourceBaseProvider<DataSourceStaticCollectionDefinition<TInputType>>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceStaticCollectionBaseSelector{TInputType}"/> class.
        /// </summary>
        protected DataSourceStaticCollectionBaseSelector(DataSourceStaticCollectionDefinition<TInputType> definition)
            : base(definition)
        {
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="DataSourceStaticCollectionDefinition{TInputType}.PullMode" />
        public PullModeEnum PullMode
        {
            get { return this.Definition.PullMode; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected abstract ValueTask<object?> OnGetNextAsync(CancellationToken token = default);

        /// <inheritdoc />
        public override ValueTask<object?> GetNextAsync(object? sourceValue, CancellationToken token = default)
        {
            return OnGetNextAsync(token);
        }

        #endregion
    }
}
