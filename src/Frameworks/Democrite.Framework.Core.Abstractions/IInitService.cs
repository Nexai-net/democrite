// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Elvex.Toolbox.Abstractions.Supports;

    /// <summary>
    /// Service at node level that need to be initialize at start
    /// </summary>
    /// <seealso cref="ISupportInitialization" />
    public interface IInitService : ISupportInitialization<IServiceProvider>
    {
        /// <summary>
        /// Gets a value indicating whether [expect orlean started].
        /// </summary>
        bool ExpectOrleanStarted { get; }
    }
}
