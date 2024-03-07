// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.DataSources
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    /// <summary>
    /// Build a source from a collection of data
    /// </summary>
    /// <typeparam name="TData">The type of the output.</typeparam>
    /// <seealso cref="IDataSourceStaticCollectionBuilder{TData}" />
    internal sealed class DataSourceFixValueBuilder<TData> : IDataSourceValueBuilder<TData>
    {
        #region Fields

        private readonly TData? _value;
        private readonly Guid _uid;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceStaticCollectionBuilder{TData}"/> class.
        /// </summary>
        public DataSourceFixValueBuilder(TData value, Guid? fixUid)
        {
            this._uid = fixUid ?? Guid.NewGuid();
            this._value = value;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public DataSourceDefinition Build()
        {
            return new DataSourceFixValueDefinition<TData>(this._uid, this._value);
        }

        #endregion
    }
}
