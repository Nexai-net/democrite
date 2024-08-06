// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Models
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal sealed class GrainStateContainer<TState> : IEntityWithId<string>
    {
        [BsonId]
        [NotNull]
        [BsonElement("_id")]
        public string Uid { get; init; }

        [BsonElement("_tag")]
        [NotNull]
        public string Etag { get; init; }

        [BsonElement("_doc")]
        public TState? State { get; init; }

        public bool RecordExists { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
