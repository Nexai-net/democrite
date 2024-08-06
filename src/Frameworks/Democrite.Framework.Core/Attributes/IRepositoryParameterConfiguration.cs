// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Attributes
{
    public interface IRepositoryParameterConfiguration
    {
        #region Properties

        /// <summary>
        /// Gets the name of the storage.
        /// </summary>
        string StorageName { get; }

        /// <summary>
        /// Gets the name of the state.
        /// </summary>
        string? ConfigurationName { get; }

        #endregion
    }
}
