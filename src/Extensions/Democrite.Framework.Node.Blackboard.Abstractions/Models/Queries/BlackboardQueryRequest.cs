﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries
{
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base class of every Blacboard query type request
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class BlackboardQueryRequest<TResponseType>(Guid QueryUid) : BlackboardBaseQuery(BlackboardQueryTypeEnum.Request, QueryUid)
    {
        #region Fields
        
        private static readonly ConcretBaseType s_expectedResponseType;
        
        #endregion

        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static BlackboardQueryRequest()
        {
            s_expectedResponseType = (ConcretBaseType)typeof(TResponseType).GetAbstractType();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the expected type of the response. null if not response is expected
        /// </summary>
        public override ConcretBaseType? ExpectedResponseType
        {
            get { return s_expectedResponseType; }
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="ISupportDebugDisplayName.ToDebugDisplayName" />
        protected abstract string OnRequestDebugDisplayName();

        /// <inheritdoc />
        public sealed override string OnDebugDisplayName()
        {
            return $"Request {OnRequestDebugDisplayName()}";
        }

        #endregion
    }
}
