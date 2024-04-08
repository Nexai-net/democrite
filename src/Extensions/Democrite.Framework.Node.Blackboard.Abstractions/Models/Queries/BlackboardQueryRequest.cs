// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries
{
    using Elvex.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Base class of every Blacboard requests
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public abstract record class BlackboardQueryRequest(Guid QueryUid) : ISupportDebugDisplayName // bool AllowPartial, Timespan? maxWaitingTime
    {
        /// <summary>
        /// Called when <see cref="ISupportDebugDisplayName.ToDebugDisplayName"/> called.
        /// </summary>
        public abstract object OnDebugDisplayName();

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"[Query] - Request {OnDebugDisplayName()}";
        }
    }
}
