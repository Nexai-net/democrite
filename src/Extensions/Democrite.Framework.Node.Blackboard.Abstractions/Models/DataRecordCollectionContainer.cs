// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates;

    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Read only collection used to easy the data access through a sequence
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <seealso cref="IReadOnlyCollection{DataRecordContainer{TData}}" />
    public sealed class DataRecordCollectionContainer<TData> : IReadOnlyCollection<DataRecordContainer<TData>>
    {
        #region Fields

        private readonly DataRecordContainer<TData>[] _data;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRecordCollectionContainer{TData}"/> class.
        /// </summary>
        public DataRecordCollectionContainer(IEnumerable<DataRecordContainer<TData>> data)
        {
            this._data = data?.ToArray() ?? EnumerableHelper<DataRecordContainer<TData>>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IReadOnlyCollection<DataRecordContainer<TData>> Data
        {
            get { return this._data; }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return this._data.Length; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <remarks>
        ///     Create a new collection
        /// </remarks>
        public IReadOnlyCollection<TData?> GetDatas()
        {
            return this._data.GroupBy(d => d.Uid)
                             .Select(grp => grp.First().Data)
                             .ToReadOnly(); 
        }

        /// <inheritdoc />
        public IEnumerator<DataRecordContainer<TData>> GetEnumerator()
        {
            return ((IReadOnlyCollection<DataRecordContainer<TData>>)this._data).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._data.GetEnumerator();
        }

        #endregion
    }

    [GenerateSerializer]
    public record struct DataRecordCollectionContainerSurrogate<TData>(DataRecordContainerSurrogate<TData>[] DataRecordContainerSurrogates);

    [RegisterConverter]
    public sealed class DataRecordCollectionContainerConverter<TData> : IConverter<DataRecordCollectionContainer<TData>, DataRecordCollectionContainerSurrogate<TData>>
    {
        /// <inheritdoc />
        public DataRecordCollectionContainer<TData> ConvertFromSurrogate(in DataRecordCollectionContainerSurrogate<TData> surrogate)
        {
            return new DataRecordCollectionContainer<TData>(surrogate.DataRecordContainerSurrogates
                                                                     .Select(d => DataRecordContainerConvert<TData>.Default.ConvertFromSurrogate(d)).ToArray());
        }

        /// <inheritdoc />
        public DataRecordCollectionContainerSurrogate<TData> ConvertToSurrogate(in DataRecordCollectionContainer<TData> value)
        {
            return new DataRecordCollectionContainerSurrogate<TData>(value.Data.Select(d => DataRecordContainerConvert<TData>.Default.ConvertToSurrogate(d)).ToArray());
        }
    }
}
