// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;
    using Democrite.Framework.Node.Blackboard.Models.Surrogates;

    using Elvex.Toolbox.Models;

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class BlackboardDeferredQueryState : IEquatable<BlackboardDeferredQueryState>
    {
        #region Fields

        private readonly IDemocriteSerializer _democriteSerializer;

        private BlackboardQueryRequest? _queryRequest;
        private byte[]? _serializeQuery;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardDeferredQueryState"/> class.
        /// </summary>
        public BlackboardDeferredQueryState(DeferredId deferredId,
                                            ConcretBaseType responseType,
                                            byte[] serializeQuery,
                                            IDemocriteSerializer democriteSerializer)
            : this(deferredId, responseType, (BlackboardQueryRequest?)null, democriteSerializer)
        {
            this._serializeQuery = serializeQuery;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardDeferredQueryState"/> class.
        /// </summary>
        private BlackboardDeferredQueryState(DeferredId deferredId,
                                             ConcretBaseType responseType,
                                             BlackboardQueryRequest? queryRequest,
                                             IDemocriteSerializer democriteSerializer)
        {
            this._democriteSerializer = democriteSerializer;

            this.DeferredId = deferredId;
            this.ResponseType = responseType;
            this._queryRequest = queryRequest;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the deferred identifier.
        /// </summary>
        public DeferredId DeferredId { get; }

        /// <summary>
        /// Gets the type of the response.
        /// </summary>
        public ConcretBaseType ResponseType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the request.
        /// </summary>
        public BlackboardQueryRequest GetRequest()
        {
            if (this._queryRequest is null)
            {
                Debug.Assert(this._serializeQuery is not null && this._serializeQuery.Any());
                var rawBytes = Convert.FromBase64String(Encoding.UTF8.GetString(this._serializeQuery));
                this._queryRequest = this._democriteSerializer.Deserialize<BlackboardQueryRequest>(rawBytes);
            }

            return this._queryRequest;
        }

        /// <summary>
        /// Gets the request serialize bytes.
        /// </summary>
        private byte[] GetRequestSerializeBytes()
        {
            if (this._serializeQuery is null)
            {
                Debug.Assert(this._democriteSerializer is not null);
                var rawQuery = this._democriteSerializer.SerializeToBinary(this._democriteSerializer).ToArray();
                this._serializeQuery = Encoding.UTF8.GetBytes(Convert.ToBase64String(rawQuery));
            }
            return this._serializeQuery;
        }

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        internal BlackboardDeferredQueryStateSurrogate ToSurrogate()
        {
            return new BlackboardDeferredQueryStateSurrogate(this.DeferredId,
                                                             ConcretBaseTypeConverter.ConvertToSurrogate(this.ResponseType),
                                                             GetRequestSerializeBytes());
        }

        /// <summary>
        /// Creates the specified deferred state.
        /// </summary>
        public static BlackboardDeferredQueryState Create(BlackboardDeferredQueryStateSurrogate surrogate, IDemocriteSerializer democriteSerializer)
        {
            return new BlackboardDeferredQueryState(surrogate.DeferredId,
                                                    ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.ResponseType),
                                                    surrogate.SerializeQuery,
                                                    democriteSerializer);
        }

        /// <summary>
        /// Creates the specified deferred state.
        /// </summary>
        public static BlackboardDeferredQueryState Create<TResponseType>(DeferredId deferredId,
                                                                         BlackboardQueryRequest request,
                                                                         IDemocriteSerializer democriteSerializer)
        {
            return new BlackboardDeferredQueryState(deferredId,
                                                    (ConcretBaseType)typeof(TResponseType).GetAbstractType(),
                                                    request,
                                                    democriteSerializer);
        }

        /// <inheritdoc />
        public bool Equals(BlackboardDeferredQueryState? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.DeferredId == other.DeferredId &&
                   this.ResponseType.Equals(other.ResponseType) &&
                   this.GetRequestSerializeBytes().SequenceEqual(other.GetRequestSerializeBytes());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is BlackboardDeferredQueryState state)
                return Equals(state);

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.DeferredId, this.ResponseType, GetRequestSerializeBytes());
        }

        #endregion
    }
}
