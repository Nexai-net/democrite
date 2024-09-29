// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.References
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to used cluster information to solve democrite resource reference
    /// </summary>
    public interface IDemocriteReferenceSolverService
    {
        /// <summary>
        /// Gets the method associate to reference
        /// </summary>
        ValueTask<MethodInfo?> GetReferenceMethodAsync(Uri methodRefId, Type? sourceType = null, CancellationToken token = default);

        /// <summary>
        /// Gets the type associate to reference
        /// </summary>
        ValueTask<Tuple<Type, Uri>?> GetReferenceTypeAsync(Uri typeRefId, CancellationToken token = default);

        /// <summary>
        /// Gets the reference definitions.
        /// </summary>
        ValueTask<IReadOnlyCollection<IDefinition>> GetReferenceDefinitionsAsync(Uri definitionRefId, CancellationToken token = default);

        /// <summary>
        /// Gets the reference definitions UID.
        /// </summary>
        ValueTask<IReadOnlyCollection<Guid>> GetReferenceDefinitionUidAsync(Uri definitionRefId, CancellationToken token = default);

        /// <summary>
        /// Tries the get the only definition target by the reference.
        /// </summary>
        ValueTask<IDefinition?> TryGetReferenceDefinitionAsync(Uri definitionRefId, CancellationToken token = default);

        /// <summary>
        /// Tries the get the only definition target by the reference.
        /// </summary>
        ValueTask<Guid?> TryGetReferenceDefinitionUriAsync(Uri definitionRefId, CancellationToken token = default);
    }
}
