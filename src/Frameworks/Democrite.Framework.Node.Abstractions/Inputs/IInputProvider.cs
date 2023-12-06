// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System;

    /// <summary>
    /// Provider using a configure source  define by <see cref="IInputSourceDefinition"/> 
    /// </summary>
    public interface IInputProvider
    {
        #region Propertie

        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        Type InputType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        ValueTask<bool> IsStillValidAsync(InputSourceDefinition inputSourceDefinition, CancellationToken token = default);

        /// <summary>
        /// Gets the next "input".
        /// </summary>
        Task<object?> GetNextAsync(CancellationToken token = default);

        /// <summary>
        /// Gets a restorable state used to dump the provider configuration.
        /// </summary>
        object? GetState();

        /// <summary>
        /// Restores the state.
        /// </summary>
        ValueTask RestoreStateAsync(object? state, CancellationToken token = default);

        #endregion
    }
}
