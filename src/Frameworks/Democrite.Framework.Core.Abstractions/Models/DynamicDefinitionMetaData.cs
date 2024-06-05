// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Surrogates;

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

    [GenerateSerializer]
    public record struct DynamicDefinitionMetaDataSurrogate(Guid Uid,
                                                            string DisplayName,
                                                            IConcretTypeSurrogate MainDefintionType,
                                                            IReadOnlyCollection<IConcretTypeSurrogate> RelatedTypes,
                                                            DateTime UTCCreate,
                                                            string? CreatedBy,
                                                            DateTime UTCLastUpdate,
                                                            string? LastUpdateBy,
                                                            bool IsEnabled,
                                                            bool Lost);

    [RegisterConverter]
    public sealed class DynamicDefinitionMetaDataConverter : IConverter<DynamicDefinitionMetaData, DynamicDefinitionMetaDataSurrogate>
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static DynamicDefinitionMetaDataConverter()
        {
            Instance = new DynamicDefinitionMetaDataConverter();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static DynamicDefinitionMetaDataConverter Instance { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public DynamicDefinitionMetaData ConvertFromSurrogate(in DynamicDefinitionMetaDataSurrogate surrogate)
        {
            return new DynamicDefinitionMetaData(surrogate.Uid,
                                                 surrogate.DisplayName,
                                                 (ConcretType)ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.MainDefintionType),
                                                 surrogate.RelatedTypes?.Select(r => (ConcretType)ConcretBaseTypeConverter.ConvertFromSurrogate(r)).ToArray() ?? EnumerableHelper<ConcretType>.ReadOnlyArray,
                                                 surrogate.UTCCreate,
                                                 surrogate.CreatedBy,
                                                 surrogate.UTCLastUpdate,
                                                 surrogate.LastUpdateBy,
                                                 surrogate.IsEnabled,
                                                 surrogate.Lost);
        }

        /// <inheritdoc />
        public DynamicDefinitionMetaDataSurrogate ConvertToSurrogate(in DynamicDefinitionMetaData value)
        {
            return new DynamicDefinitionMetaDataSurrogate(value.Uid,
                                                          value.DisplayName,
                                                          ConcretBaseTypeConverter.ConvertToSurrogate(value.MainDefintionType),
                                                          value.RelatedTypes?.Select(r => ConcretBaseTypeConverter.ConvertToSurrogate(r)).ToArray() ?? EnumerableHelper<IConcretTypeSurrogate>.ReadOnlyArray,
                                                          value.UTCCreate,
                                                          value.CreatedBy,
                                                          value.UTCLastUpdate,
                                                          value.LastUpdateBy,
                                                          value.IsEnabled,
                                                          value.Lost);
        }

        #endregion
    }
}
