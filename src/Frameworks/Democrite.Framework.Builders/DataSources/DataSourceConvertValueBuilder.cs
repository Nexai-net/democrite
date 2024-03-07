// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.DataSources
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System.Linq.Expressions;

    /// <summary>
    /// Build a source from a collection of data
    /// </summary>
    /// <typeparam name="TData">The type of the output.</typeparam>
    /// <seealso cref="IDataSourceStaticCollectionBuilder{TData}" />
    internal sealed class DataSourceConvertValueBuilder<TInput, TData> : IDataSourceConvertValueBuilder<TInput, TData>
    {
        #region Fields

        private readonly Expression<Func<TInput, TData>> _convertExpression;
        private readonly Guid _uid;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceConvertValueBuilder{TInput, TData}"/> class.
        /// </summary>
        public DataSourceConvertValueBuilder(Expression<Func<TInput, TData>> convertExpression, Guid? fixUid)
        {
            this._uid = fixUid ?? Guid.NewGuid();
            this._convertExpression = convertExpression;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public DataSourceDefinition Build()
        {
            return new DataSourceConvertValueDefinition<TData>(this._uid, this._convertExpression.CreateAccess());
        }

        #endregion
    }
}
