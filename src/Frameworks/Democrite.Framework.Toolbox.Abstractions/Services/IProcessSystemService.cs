// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Services
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge handling external process
    /// </summary>
    public interface IProcessSystemService
    {
        /// <summary>
        /// Check if executable is avaiblable
        /// </summary>
        ValueTask<bool> CheckExecAvailableAsync(string exec,
                                                CancellationToken token,
                                                params string[] arguments);

        /// <summary>
        /// Check if executable is avaiblable at <paramref name="minVersion"/>
        /// </summary>
        ValueTask<bool> CheckExecAvailableAsync(string exec,
                                                string minVersion,
                                                CancellationToken token,
                                                params string[] arguments);

        /// <summary>
        /// Check if executable is avaiblable at <paramref name="minVersion"/>
        /// </summary>
        /// <param name="comparer">
        ///     First arg = expected
        ///     First arg = source
        ///     
        ///     return > 0 if source > expected
        ///     return == 0 if source == expected    
        ///     return < 0 if source < expected    
        ///     
        ///     return null if values are not comparable
        /// </param>
        ValueTask<bool> CheckExecAvailableAsync(string exec,
                                                string minVersion,
                                                Func<string, string, int?> comparer,
                                                CancellationToken token,
                                                params string[] arguments);

        /// <summary>
        /// Launch an external executable
        /// </summary>
        ValueTask<IExternalProcess> StartAsync(string exec,
                                               CancellationToken token,
                                               params string[] arguments);

        /// <summary>
        /// Launch an external executable
        /// </summary>
        ValueTask<IExternalProcess> StartAsync(string exec,
                                               string workingDir,
                                               CancellationToken token,
                                               params string[] arguments);

        /// <summary>
        /// Launch and run external executable
        /// </summary>
        ValueTask<int> RunAsync(string exec,
                                CancellationToken token,
                                params string[] arguments);

        /// <summary>
        /// Launch and run external executable
        /// </summary>
        ValueTask<int> RunAsync(string exec,
                                string workingDir,
                                CancellationToken token,
                                params string[] arguments);
    }
}
