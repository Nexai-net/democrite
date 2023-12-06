// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Proxy controller over a external process
    /// </summary>
    public interface IExternalProcess : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the executable
        /// </summary>
        public string Executable { get; }

        /// <summary>
        /// Gets the arguments used to run.
        /// </summary>
        public IReadOnlyCollection<string> Arguments { get; }

        /// <summary>
        /// Gets an observable over the standard output
        /// </summary>
        IObservable<string> StandardOutputObservable { get; }

        /// <summary>
        /// Gets collection that group all the standard outputs.
        /// </summary>
        IReadOnlyCollection<string> StandardOutput { get; }

        /// <summary>
        /// Gets an observable over the error output
        /// </summary>
        IObservable<string> ErrorOutputObservable { get; }

        /// <summary>
        /// Gets collection that group all the error outputs.
        /// </summary>
        IReadOnlyCollection<string> ErrorOutput { get; }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        int? ExitCode { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Kills the process execution
        /// </summary>
        Task KillAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the awaiter task.
        /// </summary>
        Task GetAwaiterTask();

        #endregion
    }
}
