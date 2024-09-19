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
        ValueTask<MethodInfo?> GetReferenceMethodAsync(Uri methodRefId, Type sourceType);

        /// <summary>
        /// Gets the type associate to reference
        /// </summary>
        ValueTask<Tuple<Type, Uri>?> GetReferenceType(Uri typeRefId);

        /// <summary>
        /// Gets the reference definitions.
        /// </summary>
        ValueTask<IReadOnlyCollection<IDefinition>> GetReferenceDefinitions(Uri definitionRefId);

        /// <summary>
        /// Tries the get reference definition.
        /// </summary>
        ValueTask<IDefinition?> TryGetReferenceDefinition(Uri definitionRefId);
    }
}
