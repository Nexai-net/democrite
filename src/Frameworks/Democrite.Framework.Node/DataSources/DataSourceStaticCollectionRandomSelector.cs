// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="DataSourceStaticCollectionBaseSelector{TInputType}" />
    internal sealed class DataSourceStaticCollectionRandomSelector<TInputType> : DataSourceStaticCollectionBaseSelector<TInputType>
    {
        #region Fields

        private readonly Random _random;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceStaticCollectionRandomSelector{TInputType}"/> class.
        /// </summary>
        public DataSourceStaticCollectionRandomSelector(DataSourceStaticCollectionDefinition<TInputType> definition)
            : base(definition)
        {
            this._random = new Random();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override ValueTask<object?> OnGetNextAsync(CancellationToken token = default)
        {
            object? result = null;
            var collection = this.Definition.Collection;

            if (collection.Count > 0)
                result = this.Definition.Collection[this._random.Next(0, collection.Count)];

            return ValueTask.FromResult(result);
        }

        #endregion
    }
}
