// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Models;

    using Microsoft.CodeAnalysis;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class DynamicDefinitionHandlerState
    {
        #region Fields

        private readonly Dictionary<Guid, DynamicDefinitionMetaData> _definitionMetaDatas;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDefinitionHandlerState"/> class.
        /// </summary>
        public DynamicDefinitionHandlerState(IEnumerable<DynamicDefinitionMetaData> definitionMetaDatas, string etag)
        {
            this._definitionMetaDatas = definitionMetaDatas?.ToDictionary(k => k.Uid) ?? new Dictionary<Guid, DynamicDefinitionMetaData>();
            this.Etag = etag;

            if (string.IsNullOrEmpty(this.Etag))
                UpdateEtag();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the meta data ids.
        /// </summary>
        public IReadOnlyCollection<Guid> MetaDataIds
        {
            get { return this._definitionMetaDatas.Keys; }
        }

        /// <summary>
        /// Gets the definition meta datas.
        /// </summary>
        public IReadOnlyCollection<DynamicDefinitionMetaData> DefinitionMetaDatas
        {
            get { return this._definitionMetaDatas.Values; }
        }

        /// <summary>
        /// Gets the etag.
        /// </summary>
        public string Etag { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the meta data by ids.
        /// </summary>
        internal IEnumerable<DynamicDefinitionMetaData> GetMetaDataByIds(IReadOnlyCollection<Guid> uids, bool onlyEnable)
        {
            foreach (var uid in uids)
            {
                if (this._definitionMetaDatas.TryGetValue(uid, out var metaData) && (!onlyEnable || metaData.IsEnabled))
                    yield return metaData;
            }
        }

        /// <summary>
        /// Pushes the definition.
        /// </summary>
        internal DynamicDefinitionMetaData PushDefinition<TDefinition>(TDefinition definition,
                                                                       bool @override,
                                                                       ITimeManager timeManager,
                                                                       IIdentityCard identityCard,
                                                                       out bool updateTime) 
            where TDefinition : IDefinition
        {
            DynamicDefinitionMetaData? metaData = null;
            updateTime = false;

            if (!this._definitionMetaDatas.TryGetValue(definition.Uid, out metaData))
            {
                var type = definition.GetType();

                var mainType = (ConcretType)type.GetAbstractType();

                var relatedTypes = type.GetTypeInfoExtension()
                                       .GetAllCompatibleTypes()
                                       .Where(t => !mainType.IsEqualTo(t) && t != typeof(IEntityWithId<Guid>) && t != typeof(IDefinition) && t.IsAssignableTo(typeof(IEntityWithId<Guid>)))
                                       .Select(t => (ConcretType)t.GetAbstractType())
                                       .OrderBy(t => t.IsInterface ? 0 : 1) // Set interface first to optimize search
                                       .ToArray();

                metaData = new DynamicDefinitionMetaData(definition.Uid,
                                                         definition.DisplayName,
                                                         mainType,
                                                         relatedTypes,
                                                         timeManager.UtcNow,
                                                         identityCard?.ToString(),
                                                         timeManager.UtcNow,
                                                         null,
                                                         true,
                                                         false);

                updateTime = true;
            }
            else
            {
                updateTime = @override;
            }

            var resultMetaData = metaData;

            if (updateTime)
            {
                resultMetaData = metaData.ToUpdated(timeManager, identityCard);
                this._definitionMetaDatas[definition.Uid] = resultMetaData;

                UpdateEtag();
            }

            return resultMetaData;
        }

        /// <summary>
        /// Changes the definition status.
        /// </summary>
        internal bool ChangeDefinitionStatus(Guid definitionUid, bool enable, ITimeManager timeManager, IIdentityCard? identityCard)
        {
            if (this._definitionMetaDatas.TryGetValue(definitionUid, out var metaData) && metaData.IsEnabled != enable)
            {
                this._definitionMetaDatas[definitionUid] = metaData.ToUpdated(timeManager, identityCard, enable);
                UpdateEtag();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the definition asynchronous.
        /// </summary>
        internal void RemoveDefinitionAsync(IReadOnlyCollection<Guid> definitionIds)
        {
            bool removed = false;
            foreach (var definitionId in definitionIds)
                removed |= this._definitionMetaDatas.Remove(definitionId);

            if (removed)
                UpdateEtag();
        }

        #region Tools

        /// <summary>
        /// Updates the etag.
        /// </summary>
        private void UpdateEtag()
        {
            this.Etag = DateTime.UtcNow.Ticks + "-" + Guid.NewGuid().ToString("N");
        }

        #endregion

        #endregion
    }

    [GenerateSerializer]
    internal record struct DynamicDefinitionHandlerStateSurrogate(IReadOnlyCollection<DynamicDefinitionMetaDataSurrogate> DefinitionMetaDatas, string Etag);

    [RegisterConverter]
    internal sealed class DynamicDefinitionHandlerConverter : IConverter<DynamicDefinitionHandlerState, DynamicDefinitionHandlerStateSurrogate>
    {
        /// <inheritdoc />
        public DynamicDefinitionHandlerState ConvertFromSurrogate(in DynamicDefinitionHandlerStateSurrogate surrogate)
        {
            return new DynamicDefinitionHandlerState(surrogate.DefinitionMetaDatas?
                                                              .Select(d => DynamicDefinitionMetaDataConverter.Instance.ConvertFromSurrogate(d)) ?? EnumerableHelper<DynamicDefinitionMetaData>.ReadOnlyArray, 
                                                     surrogate.Etag);
        }

        /// <inheritdoc />
        public DynamicDefinitionHandlerStateSurrogate ConvertToSurrogate(in DynamicDefinitionHandlerState value)
        {
            return new DynamicDefinitionHandlerStateSurrogate(value.DefinitionMetaDatas
                                                                   .Select(d => DynamicDefinitionMetaDataConverter.Instance.ConvertToSurrogate(d))
                                                                   .ToArray(), 
                                                              value.Etag);
        }
    }
}
