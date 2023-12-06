// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Services
{
    using Democrite.Framework.Toolbox.Abstractions.Services;

    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IProcessSystemService" />
    public sealed class ProcessSystemService : IProcessSystemService
    {
        #region Fields

        private static readonly Regex s_version = new Regex("(?<version>[0-9]+[.]{1}[0-9]+([.]{1}[0-9]+)*)", RegexOptions.Compiled | RegexOptions.Singleline);

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<bool> CheckExecAvailableAsync(string exec, CancellationToken token, params string[] arguments)
        {
            return CheckExecAvailableAsync(exec, string.Empty, (s, e) => null, token, arguments);
        }

        /// <inheritdoc />
        public ValueTask<bool> CheckExecAvailableAsync(string exec, string minVersion, CancellationToken token, params string[] arguments)
        {
            return CheckExecAvailableAsync(exec, minVersion, DefaultMinVersionComparer, token, arguments);
        }

        /// <inheritdoc />
        public async ValueTask<bool> CheckExecAvailableAsync(string exec,
                                                             string minVersion,
                                                             Func<string, string, int?> comparer,
                                                             CancellationToken token,
                                                             params string[] arguments)
        {
            var externalProcess = await StartAsync(exec, token, arguments);
            await externalProcess.GetAwaiterTask();

            if (externalProcess.ExitCode != 0)
                return false;

            if (string.IsNullOrEmpty(minVersion))
                return externalProcess.ExitCode == 0;

            if (!externalProcess.StandardOutput.Any())
            {
                var ex = new InvalidOperationException("No version information was returned by the external executor to compare.");
                ex.Data.Add(nameof(exec), exec);
                ex.Data.Add(nameof(arguments), arguments);

                throw ex;
            }

            ArgumentNullException.ThrowIfNull(comparer);

            return externalProcess.StandardOutput
                                  .Where(log => !string.IsNullOrEmpty(log))
                                  .Any(l => (comparer(minVersion, l) ?? -1) >= 0);
        }

        /// <inheritdoc />
        public ValueTask<int> RunAsync(string exec, CancellationToken token, params string[] arguments)
        {
            return RunAsync(exec, ".", token, arguments);
        }

        /// <inheritdoc />
        public ValueTask<IExternalProcess> StartAsync(string exec,
                                                      CancellationToken token,
                                                      params string[] arguments)
        {
            return StartAsync(exec, ".", token, arguments);
        }

        /// <inheritdoc />
        public async ValueTask<int> RunAsync(string exec,
                                             string workingDir,
                                             CancellationToken token,
                                             params string[] arguments)
        {
            var externalProcess = await StartAsync(exec, token, arguments);
            await externalProcess.GetAwaiterTask();
            return externalProcess.ExitCode ?? -1;
        }

        /// <inheritdoc />
        public async ValueTask<IExternalProcess> StartAsync(string exec,
                                                            string workingDir,
                                                            CancellationToken token,
                                                            params string[] arguments)
        {
            var process = new ProcessStartInfo(exec, string.Join(" ", arguments));
            process.WorkingDirectory = workingDir;

            var externalProcess = new ExternalProcess(process, arguments, token);
            await externalProcess.RunAsync();

            return externalProcess;
        }

        #region Tools

        /// <summary>
        /// Defaults the minimum version comparer.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private int? DefaultMinVersionComparer(string expectedMinVersion, string actualVersion)
        {
            if (string.IsNullOrEmpty(expectedMinVersion) || string.IsNullOrEmpty(actualVersion))
                return null;

            var expectMatch = s_version.Match(expectedMinVersion);
            var actualMatch = s_version.Match(actualVersion);

            if (!expectMatch.Success || !actualMatch.Success)
                return null;

            var expectMatchVersion = Version.Parse(expectMatch.Groups["version"].Value);
            var actualMatchVersion = Version.Parse(actualMatch.Groups["version"].Value);

            return Comparer<Version>.Default.Compare(actualMatchVersion, expectMatchVersion);
        }

        #endregion

        #endregion
    }
}
