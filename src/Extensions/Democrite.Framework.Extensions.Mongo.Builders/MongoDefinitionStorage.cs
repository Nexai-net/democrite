// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Builders
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Extensions.Mongo.Abstractions;
    using Democrite.Framework.Extensions.Mongo.Models;
    using Elvex.Toolbox.Disposables;

    using MongoDB.Driver;

    public sealed class MongoDefinitionStorage : SafeDisposable
    {
        #region Fields

        private readonly string _connectionString;
        private readonly string _databases;
        private readonly string _collectionName;

        private IMongoCollection<DefinitionContainer>? _collection;
        private MongoClient? _client;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoDefinitionStorage"/> class.
        /// </summary>
        static MongoDefinitionStorage()
        {
            DemocriteMongoDefaultSerializerConfig.SetupSerializationConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDefinitionStorage"/> class.
        /// </summary>
        public MongoDefinitionStorage(string connectionString, string databases, string collectionName = "Definitions")
        {
            ArgumentNullException.ThrowIfNullOrEmpty(connectionString);
            ArgumentNullException.ThrowIfNullOrEmpty(collectionName);
            ArgumentNullException.ThrowIfNullOrEmpty(databases);

            this._connectionString = connectionString;
            this._databases = databases;
            this._collectionName = collectionName;
        }

        #endregion

        #region Nested

        private sealed class PushRequest : IPushRequest
        {
            #region Fields

            private readonly Queue<WriteModel<DefinitionContainer>> _definitions;
            private readonly MongoDefinitionStorage _storage;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="PushRequest"/> class.
            /// </summary>
            public PushRequest(MongoDefinitionStorage storage)
            {
                this._storage = storage;
                this._definitions = new Queue<WriteModel<DefinitionContainer>>();
            }

            #endregion

            #region Methods

            /// <inheritdoc />
            public Task<bool> PushAsync(CancellationToken token)
            {
                return this._storage.PushAsync(this._definitions, token);
            }

            /// <inheritdoc />
            IPushRequest IPushRequest.AppendDefinition<TDefinition>(params TDefinition[] definitions)
            {
                var writes = definitions?.Select(d => new ReplaceOneModel<DefinitionContainer>(Builders<DefinitionContainer>.Filter.Eq(w => w.Uid, d.Uid),
                                                                                               DefinitionContainer.Create(d))
                                                                                              { IsUpsert = true })

                                         .ToArray() ?? EnumerableHelper<ReplaceOneModel<DefinitionContainer>>.ReadOnlyArray;

                foreach (var w in writes)
                    this._definitions.Enqueue(w);

                return this;
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IPushRequest AppendDefinition<TDefinition>(params TDefinition[] definitions)
             where TDefinition : class, IDefinition
        {
            var req = new PushRequest(this);

            return ((IPushRequest)req).AppendDefinition(definitions);
        }

        /// <inheritdoc />
        internal async Task<bool> PushAsync(IEnumerable<WriteModel<DefinitionContainer>> writeModels, CancellationToken token)
        {
            var collection = GetCollection();

            var result = await collection.BulkWriteAsync(writeModels, options: null, cancellationToken: token);
            return (result.ModifiedCount + result.Upserts.LongCount()) == writeModels.LongCount();
        }

        #region Tools

        /// <summary>
        /// Gets the collection.
        /// </summary>
        private IMongoCollection<DefinitionContainer> GetCollection()
        {
            lock (this)
            {
                if (this._collection is null)
                {
                    this._client = new MongoClient(this._connectionString);
                    var db = this._client.GetDatabase(this._databases);
                    this._collection = db.GetCollection<DefinitionContainer>(this._collectionName);
                }
            }

            return this._collection;
        }

        #endregion

        #endregion
    }
}
