// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Builders
{
    using Democrite.Framework.Core.Abstractions;

    using System.Threading.Tasks;

    /// <summary>
    /// request used to push multiple definitions
    /// </summary>
    public interface IPushRequest
    {
        /// <summary>
        /// Appends the definition into the request
        /// </summary>
        IPushRequest AppendDefinition<TDefinition>(params TDefinition[] definitions)
             where TDefinition : class, IDefinition;

        /// <summary>
        /// Pushes defintions appended to the storages
        /// </summary>
        Task<bool> PushAsync(CancellationToken token);
    }
}
