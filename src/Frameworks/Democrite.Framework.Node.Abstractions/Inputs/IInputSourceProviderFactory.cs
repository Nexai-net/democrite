// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    /// <summary>
    /// Factory in change to provide a <see cref="IInputProvider"/> by <see cref="InputSourceDefinition"/>
    /// </summary>
    public interface IInputSourceProviderFactory
    {
        /// <summary>
        /// Determines whether <paramref name="provider"/> is still a valid one for the input <paramref name="inputSourceDefinition"/>
        /// </summary>
        ValueTask<bool> IsStillValidAsync(IInputProvider provider, InputSourceDefinition inputSourceDefinition, CancellationToken token = default);

        /// <summary>
        /// Gets a provider associate to <paramref name="inputSourceDefinition"/>
        /// </summary>
        IInputProvider GetProvider(InputSourceDefinition inputSourceDefinition);
    }
}
