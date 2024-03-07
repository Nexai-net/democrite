// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.DataSources
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Build a source from a collection of data
    /// </summary>
    /// <typeparam name="TData">The type of the output.</typeparam>
    /// <seealso cref="IDataSourceStaticCollectionBuilder{TData}" />
    internal sealed class DataSourceStaticCollectionBuilder<TData> : IDataSourceStaticCollectionBuilder<TData>
    {
        #region Fields

        private readonly IReadOnlyCollection<TData> _staticCollection;
        private readonly Guid _uid;

        private PullModeEnum? _pullMode;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceStaticCollectionBuilder{TData}"/> class.
        /// </summary>
        public DataSourceStaticCollectionBuilder(IEnumerable<TData> staticCollection, Guid? fixUid)
        {
            this._uid = fixUid ?? Guid.NewGuid();
            this._staticCollection = staticCollection?.ToArray() ?? EnumerableHelper<TData>.ReadOnlyArray;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public DataSourceDefinition Build()
        {
            return new DataSourceStaticCollectionDefinition<TData>(this._uid,
                                                                   this._staticCollection,
                                                                   this._pullMode ?? PullModeEnum.Circling);
        }

        /// <inheritdoc />
        public IDataSourceStaticCollectionBuilder<TData> PullMode(PullModeEnum mode)
        {
            this._pullMode = mode;
            return this;
        }

        #endregion
    }
}
