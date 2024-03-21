﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class DynamicDefinitionMetaData(Guid Uid,
                                                         string DisplayName,
                                                         ConcretType MainDefintionType,
                                                         IReadOnlyCollection<ConcretType> RelatedTypes,
                                                         DateTime UTCCreate,
                                                         string? CreatedBy,
                                                         DateTime UTCLastUpdate,
                                                         string? LastUpdateBy,
                                                         bool IsEnabled,
                                                         bool Lost)
    {
        /// <summary>
        /// Updates the specified time manager.
        /// </summary>
        public DynamicDefinitionMetaData ToUpdated(ITimeManager timeManager, IIdentityCard? identityCard, bool? enable = null)
        {
            return new DynamicDefinitionMetaData(this.Uid,
                                                 this.DisplayName,
                                                 this.MainDefintionType,
                                                 this.RelatedTypes,
                                                 this.UTCCreate,
                                                 this.CreatedBy,
                                                 timeManager.UtcNow,
                                                 identityCard?.ToString(),
                                                 enable ?? this.IsEnabled,
                                                 this.Lost);
        }
    }
}
