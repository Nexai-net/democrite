// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Memories
{
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Saftly contains <typeparamref name="TItem"/>
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class SafeContainer<TItem> : IDisposable
    {
        #region Fields

        private readonly Dictionary<Guid, TItem> _items;
        private readonly bool _disposeItems;

        private long _disposeCounter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeContainer{TItem}"/> class.
        /// </summary>
        public SafeContainer(bool disposeItems = false)
        {
            this._disposeItems = disposeItems;
            this._items = new Dictionary<Guid, TItem>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SafeContainer{TItem}"/> class.
        /// </summary>
        ~SafeContainer()
        {
            Dispose(true);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get { return Interlocked.Read(ref this._disposeCounter) > 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a new <paramref name="item"/>, return a unique id to easily remove this item if needed
        /// </summary>
        public Guid Register(TItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (this.IsDisposed)
                return Guid.Empty;

            lock (this._items)
            {
                var key = Guid.NewGuid();
                this._items.Add(key, item);
                return key;
            }
        }

        /// <summary>
        /// Removes the <see cref="TItem"/> by is register id
        /// </summary>
        public void Remove(Guid id)
        {
            if (this.IsDisposed)
                return;

            lock (this._items)
            {
                this._items.Remove(id);
            }
        }

        /// <summary>
        /// Removes the <see cref="TItem"/>.Work only of item that support equal comparaison
        /// </summary>
        public void Remove(TItem item)
        {
            if (this.IsDisposed)
                return;

            lock (this._items)
            {
                var items = this._items.Where(v => EqualityComparer<TItem>.Default.Equals(v.Value, item)).ToReadOnly();

                foreach (var it in items)
                    this._items.Remove(it.Key);
            }
        }

        /// <summary>
        /// Gets a copy of item contains
        /// </summary>
        public IReadOnlyCollection<TItem> GetContainerCopy()
        {
            if (this.IsDisposed)
                return EnumerableHelper<TItem>.ReadOnlyArray;

            lock (this._items)
            {
                return this._items.Values.ToArray();
            }
        }

        /// <summary>
        /// Clears contains
        /// </summary>
        public void Clear()
        {
            if (this.IsDisposed)
                return;

            lock (this._items)
            {
                this._items.Clear();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(false);
        }

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        private void Dispose(bool fromFinalizer)
        {
            if (Interlocked.Increment(ref this._disposeCounter) > 1)
                return;

            if (this._disposeItems)
            {
                KeyValuePair<Guid, TItem>[] items;
                lock (this._items)
                {
                    items = this._items.ToArray();
                }

                foreach (var item in items.OfType<IDisposable>())
                    item.Dispose();
            }

            Clear();
        }

        #endregion
    }
}
