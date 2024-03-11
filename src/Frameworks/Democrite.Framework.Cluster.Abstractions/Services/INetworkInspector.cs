// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Services
{
    using Elvex.Toolbox.Abstractions.Disposables;

    using System.Collections.Generic;
    using System.Net;

    /// <summary>
    /// Inspector used to solve DNS, Address, ... about network
    /// </summary>
    public interface INetworkInspector
    {
        /// <summary>
        /// Gets the host addresses information from <paramref name="hostName"/>
        /// </summary>
        IReadOnlyCollection<IPAddress> GetHostAddresses(string hostName);

        /// <summary>
        /// Solve host DNS to ip:port from DNS_NAME:port
        /// </summary>
        IReadOnlyCollection<string> SolveHostAddresse(string hostName);

        /// <summary>
        /// Gets current host information
        /// </summary>
        string GetHostName();

        /// <inheritdoc cref="NetworkHelper.GetNextUnusedPort"/>
        int GetNextUnusedPort(int minPortIncluded, int maxPortExcluded);

        /// <inheritdoc cref="NetworkHelper.GetNextUnusedPort"/>
        /// <returns>
        ///     <c>Token</c> return a token used to reserved the port.
        ///     Reservation is available until token is disposed.
        /// </returns>
        ISecureContextToken<int> GetAndReservedNextUnusedPort(int minPortIncluded, int maxPortExcluded);
    }
}
