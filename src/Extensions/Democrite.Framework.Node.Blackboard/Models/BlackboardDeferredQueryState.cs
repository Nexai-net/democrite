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

        private BlackboardBaseQuery? _queryRequest;
        private byte[]? _serializeQuery;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardDeferredQueryState"/> class.
        /// </summary>
        public BlackboardDeferredQueryState(DeferredId deferredId,
                                            ConcretBaseType? responseType,
                                            byte[] serializeQuery,
                                            BlackboardQueryTypeEnum type,
                                            IDemocriteSerializer democriteSerializer)
            : this(deferredId, responseType, (BlackboardBaseQuery?)null, democriteSerializer, type)
        {
            this._serializeQuery = serializeQuery;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardDeferredQueryState"/> class.
        /// </summary>
        private BlackboardDeferredQueryState(DeferredId deferredId,
                                             ConcretBaseType? responseType,
                                             BlackboardBaseQuery? queryRequest,
                                             IDemocriteSerializer democriteSerializer,
                                             BlackboardQueryTypeEnum type)
        {
            this._democriteSerializer = democriteSerializer;

            this.Type = type;
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
        public ConcretBaseType? ResponseType { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public BlackboardQueryTypeEnum Type { get; }    

        #endregion

        #region Methods

        /// <summary>
        /// Gets the request.
        /// </summary>
        public BlackboardBaseQuery GetRequest()
        {
            if (this._queryRequest is null)
            {
                Debug.Assert(this._serializeQuery is not null && this._serializeQuery.Any());
                var rawBytes = Convert.FromBase64String(Encoding.UTF8.GetString(this._serializeQuery));
                this._queryRequest = this._democriteSerializer.Deserialize<BlackboardBaseQuery>(rawBytes);
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
                                                             this.ResponseType is null ? null : ConcretBaseTypeConverter.ConvertToSurrogate(this.ResponseType),
                                                             this.Type,
                                                             GetRequestSerializeBytes());
        }

        /// <summary>
        /// Creates the specified deferred state.
        /// </summary>
        public static BlackboardDeferredQueryState Create(BlackboardDeferredQueryStateSurrogate surrogate, IDemocriteSerializer democriteSerializer)
        {
            return new BlackboardDeferredQueryState(surrogate.DeferredId,
                                                    surrogate.ResponseType is null ? null : ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.ResponseType),
                                                    surrogate.SerializeQuery,
                                                    surrogate.Type,
                                                    democriteSerializer);
        }

        /// <summary>
        /// Creates the specified deferred state.
        /// </summary>
        public static BlackboardDeferredQueryState Create(DeferredId deferredId,
                                                          BlackboardBaseQuery query,
                                                          IDemocriteSerializer democriteSerializer)
        {
            return new BlackboardDeferredQueryState(deferredId,
                                                    query.ExpectedResponseType,
                                                    query,
                                                    democriteSerializer,
                                                    query.Type);
        }

        /// <inheritdoc />
        public bool Equals(BlackboardDeferredQueryState? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.DeferredId == other.DeferredId &&
                   (this.ResponseType?.Equals(other.ResponseType) ?? other.ResponseType is null)  &&
                   this.Type == other.Type &&
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
