// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Supports
{
    using System.Threading.Tasks;

    /// <summary>
    /// Define an instance that support initialization
    /// </summary>
    public interface ISupportInitialization
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is initializing.
        /// </summary>
        bool IsInitializing { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        bool IsInitialized { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the instance
        /// </summary>
        ValueTask InitializationAsync<TState>(TState? initializationState = default, CancellationToken token = default);

        #endregion
    }
}
