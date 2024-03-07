// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.DataSources
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Build a data source provider
    /// </summary>
    /// <remarks>
    ///     Data source type builders are split in different interface to allow chosing the source allowed
    /// </remarks>
    internal class DataSourceBuilder : IDataSourceBuilder
    {
        #region Methods

        /// <inheritdoc />
        public IDataSourceConvertValueBuilder<TInput, TData> Convert<TInput, TData>(Expression<Func<TInput, TData>> dataProvider, Guid? fixUid = null)
        {
            return new DataSourceConvertValueBuilder<TInput, TData>(dataProvider, fixUid);  
        }

        /// <inheritdoc />
        public IDataSourceValueBuilder<TData> FixValue<TData>(TData data, Guid? fixUid = null)
        {
            return new DataSourceFixValueBuilder<TData>(data, fixUid);
        }

        /// <inheritdoc />
        public IDataSourceStaticCollectionBuilder<TData> StaticCollection<TData>(IEnumerable<TData> collection, Guid? fixUid = null)
        {
            return new DataSourceStaticCollectionBuilder<TData>(collection, fixUid);
        }

        #endregion
    }
}
