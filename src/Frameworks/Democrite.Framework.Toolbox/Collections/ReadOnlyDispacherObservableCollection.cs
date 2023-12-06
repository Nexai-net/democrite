// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Collections
{
    using Democrite.Framework.Toolbox.Abstractions.Proxies;

    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// Observable Read only dictionary that relay event <see cref="INotifyCollectionChanged"/> into <see cref="IDispatcherProxy"/> pass by ctor
    /// </summary>
    public sealed class ReadOnlyDispacherObservableCollection<T> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Fields

        private readonly IReadOnlyCollection<T> _source;
        private readonly IDispatcherProxy _dispatcherProxy;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDispacherObservableCollection{TKey, TValue}"/> class.
        /// </summary>
        public ReadOnlyDispacherObservableCollection(IReadOnlyCollection<T> source, IDispatcherProxy dispatcherProxy)
        {
            this._dispatcherProxy = dispatcherProxy;
            this._source = source;

            if (this._source is INotifyCollectionChanged collectionChanged)
                collectionChanged.CollectionChanged += OnCollectionChanged;

            if (this._source is INotifyPropertyChanged propertyChanged)
                propertyChanged.PropertyChanged += OnPropertyChanged;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public int Count
        {
            get { return this._source.Count; }
        }

        #endregion

        #region Event

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._source.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this._source.GetEnumerator();
        }

        #region Tools

        /// <summary>
        /// Relay event <see cref="INotifyPropertyChanged.PropertyChanged"/> in dispatcher
        /// </summary>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this._dispatcherProxy.Send(() => PropertyChanged?.Invoke(sender, e));
        }

        /// <summary>
        /// Relay event <see cref="INotifyCollectionChanged.CollectionChanged"/> in dispatcher
        /// </summary>
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            this._dispatcherProxy.Send(() => CollectionChanged?.Invoke(sender, e));
        }

        #endregion

        #endregion
    }
}
