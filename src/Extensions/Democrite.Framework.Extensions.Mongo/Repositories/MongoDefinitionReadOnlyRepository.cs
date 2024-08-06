// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Repositories
{
    //using Democrite.Framework.Core.Abstractions;
    //using Democrite.Framework.Core.Abstractions.Repositories;
    //using Democrite.Framework.Extensions.Mongo.Models;

    //using Microsoft.Extensions.Options;

    //using MongoDB.Bson.Serialization;
    //using MongoDB.Driver;

    //using Orleans.Providers.MongoDB.Configuration;
    //using Orleans.Providers.MongoDB.Utils;

    //using System;
    //using System.Collections.Generic;
    //using System.Diagnostics.CodeAnalysis;
    //using System.Linq.Expressions;
    //using System.Threading;
    //using System.Threading.Tasks;

    ///// <summary>
    ///// Repository dedicated to definition
    ///// </summary>
    ///// <typeparam name="TDocumentType">The type of the document type.</typeparam>
    //public sealed class MongoDefinitionReadOnlyRepository<TDocumentType> : MongoReadOnlyBaseRepository<DefinitionContainer<TDocumentType>, TDocumentType>, IReadOnlyRepository<TDocumentType, Guid>, IReadOnlyRepository<DefinitionContainer<TDocumentType>, Guid>
    //    where TDocumentType : class, IDefinition
    //{
    //    #region Fields

    //    private static readonly AggregateExpressionDefinition<DefinitionContainer<TDocumentType>, TDocumentType> s_newRootEntity;
    //    private static readonly FilterDefinition<DefinitionContainer<TDocumentType>> s_discriminatorFilter;

    //    #endregion

    //    #region Ctor

    //    /// <summary>
    //    /// Initializes the <see cref="MongoDefinitionReadOnlyRepository{TDocumentType}"/> class.
    //    /// </summary>
    //    static MongoDefinitionReadOnlyRepository()
    //    {
    //        s_discriminatorFilter = Builders<DefinitionContainer<TDocumentType>>.Filter.Eq(f => f.Discriminator, DefinitionContainer<TDocumentType>.DefaultDiscriminator);
    //        s_newRootEntity = new ExpressionAggregateExpressionDefinition<DefinitionContainer<TDocumentType>, TDocumentType>(c => c.Definition, new ExpressionTranslationOptions() { StringTranslationMode = AggregateStringTranslationMode.Bytes });

    //        BsonClassMap.TryRegisterClassMap<DefinitionContainer<TDocumentType>>();
    //        BsonClassMap.TryRegisterClassMap<TDocumentType>();
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="MongoDefinitionReadOnlyRepository{TDocumentType}"/> class.
    //    /// </summary>
    //    public MongoDefinitionReadOnlyRepository(IMongoClientFactory mongoClientFactory,
    //                                        IServiceProvider serviceProvider,
    //                                        string? collectionName = null,
    //                                        IOptions<MongoDBOptions>? mongoDBOptions = null)
    //        : base(mongoClientFactory, serviceProvider, string.IsNullOrEmpty(collectionName) ? "Definitions" : collectionName, mongoDBOptions)
    //    {
    //    }

    //    #endregion

    //    #region Methods

    //    /// <inheritdoc />
    //    protected override FilterDefinition<DefinitionContainer<TDocumentType>>? PreEntityFilter()
    //    {
    //        return s_discriminatorFilter;
    //    }

    //    /// <inheritdoc />
    //    protected override AggregateExpressionDefinition<DefinitionContainer<TDocumentType>, TDocumentType>? HowToGoToStoredEntity()
    //    {
    //        return s_newRootEntity;
    //    }

    //    /// <inheritdoc />
    //    public ValueTask<TDocumentType?> GetByIdValueAsync([NotNull] Guid entityId, CancellationToken token)
    //    {
    //        return base.GetFirstValueAsync((TDocumentType d) => d.Uid == entityId, token);
    //    }

    //    /// <inheritdoc />
    //    public ValueTask<IReadOnlyCollection<TDocumentType>> GetByIdsValueAsync([NotNull] IReadOnlyCollection<Guid> entityIds, CancellationToken token)
    //    {
    //        return base.GetValuesAsync((TDocumentType d) => entityIds.Contains(d.Uid), token);
    //    }

    //    /// <inheritdoc />
    //    public ValueTask<IReadOnlyCollection<TProjection>> GetByIdsValueAsync<TProjection>([NotNull] IReadOnlyCollection<Guid> entityIds, CancellationToken token)
    //    {
    //        return base.GetValuesAsync<TProjection>((TDocumentType d) => entityIds.Contains(d.Uid), token);
    //    }

    //    /// <inheritdoc />
    //    ValueTask<DefinitionContainer<TDocumentType>?> IReadOnlyRepository<DefinitionContainer<TDocumentType>, Guid>.GetByIdValueAsync([NotNull] Guid entityId, CancellationToken token)
    //    {
    //        return base.GetFirstContainerAsync((DefinitionContainer<TDocumentType> d) => d.Uid == entityId, token);
    //    }

    //    /// <inheritdoc />
    //    ValueTask<IReadOnlyCollection<DefinitionContainer<TDocumentType>>> IReadOnlyRepository<DefinitionContainer<TDocumentType>, Guid>.GetByIdsValueAsync([NotNull] IReadOnlyCollection<Guid> entityIds, CancellationToken token)
    //    {
    //        return base.GetContainersAsync((DefinitionContainer<TDocumentType> d) => entityIds.Contains(d.Uid), token);
    //    }

    //    /// <inheritdoc />
    //    public ValueTask<DefinitionContainer<TDocumentType>?> GetFirstValueAsync([AllowNull] Expression<Func<DefinitionContainer<TDocumentType>, bool>> filterExpression, CancellationToken token)
    //    {
    //        return base.GetFirstContainerAsync(filterExpression, token);
    //    }

    //    /// <inheritdoc />
    //    public ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<DefinitionContainer<TDocumentType>, bool>> filterExpression, CancellationToken token)
    //    {
    //        return base.GetFirstContainerAsync<TProjection>(filterExpression, token);   
    //    }

    //    /// <inheritdoc />
    //    public ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<DefinitionContainer<TDocumentType>, bool>> filterExpression, CancellationToken token)
    //    {
    //        return base.GetContainersAsync<TProjection>(filterExpression, token);   
    //    }

    //    /// <inheritdoc />
    //    public ValueTask<IReadOnlyCollection<DefinitionContainer<TDocumentType>>> GetValuesAsync([AllowNull] Expression<Func<DefinitionContainer<TDocumentType>, bool>> filterExpression, CancellationToken token)
    //    {
    //        return base.GetContainersAsync(filterExpression, token);
    //    }

    //    #endregion
    //}
}
