// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using System.Collections.Generic;

    /// <summary>
    /// Define an executable artifact
    /// </summary>
    /// <seealso cref="IArtifactResource" />
    public interface IArtifactExecutableResource : IArtifactResource
    {
        #region Properties

        /// <summary>
        /// Gets the executable path.
        /// </summary>
        string ExecutablePath { get; }

        /// <summary>
        /// Gets a value indicating whether the executable managed to stay alive an call multiple times.
        /// </summary>
        bool AllowPersistence { get; }

        /// <summary>
        /// Gets the execution arguments.
        /// </summary>
        IReadOnlyCollection<string> Arguments { get; }

        /// <summary>
        /// Gets the executor needed to execute the <see cref="ExecutablePath"/>.
        /// </summary>
        /// <remarks>
        ///     example: '<c>pyhton:6.0</c>', '<c>dotnet:6.0</c>', '<c>nodejs:2.0</c>', '<c>php:7.5</c>'
        /// </remarks>
        string? Executor { get; }

        #endregion
    }
}
