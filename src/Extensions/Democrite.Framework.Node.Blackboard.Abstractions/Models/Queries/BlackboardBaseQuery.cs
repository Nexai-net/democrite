// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries
{
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Base class of every Blacboard queries
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public abstract record class BlackboardBaseQuery(BlackboardQueryTypeEnum Type, Guid QueryUid) : ISupportDebugDisplayName // bool AllowPartial, Timespan? maxWaitingTime
    {
        #region Properties

        /// <summary>
        /// Gets the expected type of the response. null if not response is expected
        /// </summary>
        public abstract ConcretBaseType? ExpectedResponseType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Called when <see cref="ISupportDebugDisplayName.ToDebugDisplayName"/> called.
        /// </summary>
        public abstract object OnDebugDisplayName();

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"[Query] - {OnDebugDisplayName()}";
        }

        #endregion
    }
}
