// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Inputs
{
    using Democrite.Framework.Node.Inputs;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation using cycling index to provide input
    /// </summary>
    /// <seealso cref="InputSourceStaticCollectionBaseSelector{TInputType}" />
    internal sealed class InputSourceStaticCollectionCirclingSelector<TInputType> : InputSourceStaticCollectionBaseSelector<TInputType>
    {
        #region Fields

        private readonly ReaderWriterLock _overflowLocker;

        private int _readIndex = -1;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceStaticCollectionCirclingSelector{TInputType}"/> class.
        /// </summary>
        public InputSourceStaticCollectionCirclingSelector(InputSourceStaticCollectionDefinition<TInputType> definition)
            : base(definition)
        {
            this._overflowLocker = new ReaderWriterLock();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Use cycling index to navigate onto the static collection
        /// </summary>
        public override Task<object?> GetNextAsync(CancellationToken token = default)
        {
            object? result = null;
            var collection = this.Definition.Collection;

            if (collection.Count > 0)
            {
                var indx = 0;

                // Lock each time to mange the overflow and a parfaite cycling
                // Interlock doesn't managed the modulo to prevent overflow
                lock (this._overflowLocker)
                {
                    if (this._readIndex > 65536)
                        this._readIndex %= collection.Count;

                    this._readIndex++;

                    indx = this._readIndex % collection.Count;
                }

                result = collection[indx];
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Provide current read index as state
        /// </summary>
        public override object? GetState()
        {
            lock (this._overflowLocker)
            {
                return this._readIndex;
            }
        }

        /// <summary>
        ///     Restore read index based on state provide
        /// </summary>
        public override ValueTask RestoreStateAsync(object? state, CancellationToken token = default)
        {
            if (state is int indxState)
            {
                lock (this._overflowLocker)
                {
                    this._readIndex = indxState;
                }
            }

            return base.RestoreStateAsync(state, token);
        }

        #endregion
    }
}
