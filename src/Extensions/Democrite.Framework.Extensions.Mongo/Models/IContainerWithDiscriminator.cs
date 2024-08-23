// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Models
{
    using MongoDB.Driver;

    internal interface IContainerWithDiscriminator<TContainer>
    {
        /// <summary>
        /// Gets the discriminator filter.
        /// </summary>
        FilterDefinition<TContainer>? DiscriminatorFilter { get; }
    }
}
