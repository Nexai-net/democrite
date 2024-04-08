﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries
{
    using Democrite.Framework.Core.Abstractions.Deferred;

    using Elvex.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    public interface IBlackboardQueryResponse
    {
        QueryReponseTypeEnum Type { get; }
    }

    /// <summary>
    /// Base class of every query response
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public abstract record class BlackboardQueryResponse<TReponseData>(QueryReponseTypeEnum Type) : ISupportDebugDisplayName, IBlackboardQueryResponse
    {
        /// <inheritdoc />
        public abstract string ToDebugDisplayName();
    }

    /// <summary>
    /// Reponse used to reject any query 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed record class BlackboardQueryRejectedResponse<TResponseData>(string? Detail) : BlackboardQueryResponse<TResponseData>(QueryReponseTypeEnum.Rejected)
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static BlackboardQueryRejectedResponse()
        {
            NoController = new BlackboardQueryRejectedResponse<TResponseData>("No Event Controller setup in the template");
            Default = new BlackboardQueryRejectedResponse<TResponseData>("Controller provide no response DuringQueryProcessing");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets reject reponse due to no controller available to process requests
        /// </summary>
        public static BlackboardQueryRejectedResponse<TResponseData> NoController { get; }

        /// <summary>
        /// Gets reject reponse due to controller null reponse.
        /// </summary>
        public static BlackboardQueryRejectedResponse<TResponseData> Default { get; }


        #endregion

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[Query] - Rejected - {this.Detail}";
        }
    }

    /// <summary>
    /// The query couldn't resolve with the current information.
    /// This response provide an id that could be used to cancel the query or await in the deferred democrite system
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed record class BlackboardQueryDeferredResponse<TResponseData>(in DeferredId DeferredAwaiterId) : BlackboardQueryResponse<TResponseData>(QueryReponseTypeEnum.Deferred)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[Query] - Deferred - {this.DeferredAwaiterId}";
        }
    }

    /// <summary>
    /// Provide direct response of the request
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed record class BlackboardQueryDirectResponse<TResponseData>(in TResponseData Response, Guid QueryId) : BlackboardQueryResponse<TResponseData>(QueryReponseTypeEnum.Direct)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[Query] - Direct - From {this.QueryId}";
        }
    }
}
