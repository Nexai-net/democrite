// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models.Surrogates
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;

    [GenerateSerializer]
    internal record struct BlackboardDeferredQueryStateSurrogate(DeferredId DeferredId,
                                                                 IConcretTypeSurrogate? ResponseType,
                                                                 BlackboardQueryTypeEnum Type,
                                                                 byte[] SerializeQuery);

    [GenerateSerializer]
    internal record struct BlackboardGrainStateSurrogate(BlackboardTemplateDefinitionSurrogate? TemplateDefinition,
                                                         BlackboardId BlackboardId,
                                                         string Name,
                                                         BlackboardRecordRegistryStateSurrogate BlackboardRecordRegistryState,
                                                         BlackboardDeferredQueryStateSurrogate[] Queries,
                                                         BlackboardLifeStatusEnum CurrentLifeStatus,
                                                         SubscriptionId[] SubscriptionIds);

    [RegisterConverter]
    internal sealed class BlackboardGrainStateConverter : IConverter<BlackboardGrainState, BlackboardGrainStateSurrogate>
    {
        #region Fields

        private readonly IDemocriteSerializer _democriteSerializer;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardGrainStateConverter"/> class.
        /// </summary>
        public BlackboardGrainStateConverter(IDemocriteSerializer democriteSerializer)
        {
            this._democriteSerializer = democriteSerializer;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public BlackboardGrainState ConvertFromSurrogate(in BlackboardGrainStateSurrogate surrogate)
        {
            return new BlackboardGrainState((surrogate.TemplateDefinition is not null ? BlackboardTemplateDefinitionConverter.Default.ConvertFromSurrogate(surrogate.TemplateDefinition.Value) : null),
                                            surrogate.BlackboardId,
                                            surrogate.Name,
                                            BlackboardRecordRegistryStateConverter.Default.ConvertFromSurrogate(surrogate.BlackboardRecordRegistryState),
                                            surrogate.Queries?.Select(q => BlackboardDeferredQueryState.Create(q, this._democriteSerializer)) ?? EnumerableHelper<BlackboardDeferredQueryState>.ReadOnly,
                                            surrogate.CurrentLifeStatus,
                                            surrogate.SubscriptionIds);
        }

        /// <inheritdoc />
        public BlackboardGrainStateSurrogate ConvertToSurrogate(in BlackboardGrainState value)
        {
            var surrogate = new BlackboardGrainStateSurrogate()
            {
                TemplateDefinition = (value.TemplateCopy is not null ? BlackboardTemplateDefinitionConverter.Default.ConvertToSurrogate(value.TemplateCopy) : null),
                BlackboardId = value.BlackboardId,
                Name = value.Name,
                BlackboardRecordRegistryState = BlackboardRecordRegistryStateConverter.Default.ConvertToSurrogate(value.Registry),
                Queries = value.GetQueries()?.Select(r => r.ToSurrogate()).ToArray() ?? EnumerableHelper<BlackboardDeferredQueryStateSurrogate>.ReadOnlyArray,
                CurrentLifeStatus = value.CurrentLifeStatus,
                SubscriptionIds = value.GetSubscriptions().ToArray()
            };

            return surrogate;
        }

        #endregion
    }
}
