// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Collections
{
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Synchronize a collection from <typeparamref name="TSource"/> to <typeparamref name="T"/>
    /// </summary>
    public sealed class ReadOnlyObservableTranslationCollection<T, TSource> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Fields

        private readonly IReadOnlyCollection<TSource> _sources;
        private readonly Func<TSource, T> _translate;
        private readonly List<T> _translatedItems;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyObservableTranslationCollection{T, TSource}"/> class.
        /// </summary>
        public ReadOnlyObservableTranslationCollection(IReadOnlyCollection<TSource> sources, Func<TSource, T> translate)
        {
            ArgumentNullException.ThrowIfNull(sources);
            ArgumentNullException.ThrowIfNull(translate);

            this._translatedItems = new List<T>(sources.Select(s => translate(s)) ?? EnumerableHelper<T>.ReadOnly);
            this._translate = translate;
            this._sources = sources;

            if (sources is INotifyCollectionChanged collectionChanged)
                collectionChanged.CollectionChanged += OnCollectionChanged;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public int Count
        {
            get { return this._sources.Count; }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this._translatedItems.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._translatedItems.GetEnumerator();
        }

        /// <summary>
        /// Called when the source Collections the changed.
        /// </summary>
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newItems = e.NewItems?.OfType<TSource>()
                                .Select(n => this._translate(n))
                                .ToArray();

                if (newItems != null && newItems.Length > 0)
                {
                    this._translatedItems.AddRange(newItems);

                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
                }
            }
        }

        #endregion
    }
}
