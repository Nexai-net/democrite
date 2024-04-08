// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Deferred
{
    using Democrite.Framework.Core.Abstractions;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DeferredHandlerState
    {
        #region Fields

        private readonly Dictionary<Guid, DeferredWork> _indexedWork;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredHandlerState"/> class.
        /// </summary>
        public DeferredHandlerState(IEnumerable<DeferredWork>? works)
        {
            this._indexedWork = works?.ToDictionary(k => k.DeferredId.Uid) ?? new Dictionary<Guid, DeferredWork>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the deferred work by identifier.
        /// </summary>
        internal DeferredWork? GetDeferredWorkById(Guid deferredId)
        {
            if (this._indexedWork.TryGetValue(deferredId, out var work))
                return work;

            return null;
        }

        /// <summary>
        /// Gets the deferred work by source identifier.
        /// </summary>
        internal IReadOnlyCollection<DeferredWork> GetDeferredWorkBySourceId(Guid sourceId, IIdentityCard? identityCardFilter)
        {
            // TODO : Use identity card as filter
            return this._indexedWork.Values.Where(v => v.DeferredId.SourceId == sourceId)
                                           .ToArray();
        }

        /// <summary>
        /// Removes the specified work.
        /// </summary>
        internal void Remove(DeferredWork work)
        {
            this._indexedWork.Remove(work.DeferredId.Uid);
        }

        /// <summary>
        /// Adds the specified new work.
        /// </summary>
        internal void Add(DeferredWork newWork)
        {
            this._indexedWork.Add(newWork.DeferredId.Uid, newWork);
        }

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        internal DeferredHandlerStateSurrogate ToSurrogate()
        {
            return new DeferredHandlerStateSurrogate(this._indexedWork.Values.Select(v => DeferredWorkConverter.Default.ConvertToSurrogate(v)).ToReadOnly());
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal record struct DeferredHandlerStateSurrogate(IReadOnlyCollection<DeferredWorkSurrogate> Works);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IConverter{DeferredHandlerState, DeferredHandlerStateSurrogate}" />
    [RegisterConverter]
    internal sealed class DeferredHandlerStateConverter : IConverter<DeferredHandlerState, DeferredHandlerStateSurrogate>
    {
        /// <inheritdoc />
        public DeferredHandlerState ConvertFromSurrogate(in DeferredHandlerStateSurrogate surrogate)
        {
            return new DeferredHandlerState(surrogate.Works?.Select(w => DeferredWorkConverter.Default.ConvertFromSurrogate(w)));
        }

        /// <inheritdoc />
        public DeferredHandlerStateSurrogate ConvertToSurrogate(in DeferredHandlerState value)
        {
            return value.ToSurrogate();
        }
    }
}