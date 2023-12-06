// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    using System.Threading.Tasks;

    /// <summary>
    /// Consume <see cref="IDiagnosticLog"/>
    /// </summary>
    public interface IDiagnosticLogConsumer
    {
        /// <summary>
        /// Process diagnostic log
        /// </summary>
        Task LogAsync<TLog>(TLog log, CancellationToken token = default) where TLog : class, IDiagnosticLog;
    }
}
