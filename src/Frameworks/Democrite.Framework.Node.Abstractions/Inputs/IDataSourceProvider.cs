// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Provider using a configure source  define by <see cref="IInputSourceDefinition"/> 
    /// </summary>
    public interface IDataSourceProvider
    {
        #region Propertie

        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        AbstractType DataType { get; }

        /// <summary>
        /// Gets a value indicating whether this provider is statefull.
        /// </summary>
        bool UseState { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether provider is still valid based on specific definition.
        /// </summary>
        ValueTask<bool> IsStillValidAsync(DataSourceDefinition inputSourceDefinition, CancellationToken token = default);

        /// <summary>
        /// Gets the next "input".
        /// </summary>
        ValueTask<object?> GetNextAsync(object? sourceValue, CancellationToken token = default);

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
