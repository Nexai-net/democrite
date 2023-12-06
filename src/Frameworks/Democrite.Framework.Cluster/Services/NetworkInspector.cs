// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Services
{
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Toolbox.Abstractions.Disposables;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Inpector in charge to support all network capabilities
    /// </summary>
    /// <seealso cref="INetworkInspector" />
    public sealed class NetworkInspector : INetworkInspector
    {
        #region fields

        private const string IpGroupName = "Ip";
        private const string PortGroupName = "Port";
        private const string HostGroupName = "Host";

        private static readonly Regex s_ipString = new Regex("^(?<Ip>[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3})$", RegexOptions.Compiled);
        private static readonly Regex s_ipPortString = new Regex("^(?<Ip>[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}):(?<Port>[0-9]{2,6})$", RegexOptions.Compiled);
        private static readonly Regex s_hostPortString = new Regex("^(?<Host>[a-zA-Z0-9-.]+):(?<Port>[0-9]{2,6})$", RegexOptions.Compiled);

        #endregion

        #region Methods

        /// <inheritdoc />
        public IReadOnlyCollection<IPAddress> GetHostAddresses(string hostName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(hostName);

            return Dns.GetHostAddresses(hostName);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> SolveHostAddresse(string hostName)
        {
            hostName = hostName?.Trim() ?? string.Empty;

            ArgumentNullException.ThrowIfNullOrEmpty(hostName);

            if (s_ipPortString.IsMatch(hostName) || s_ipString.IsMatch(hostName))
                return new[] { hostName };

            var host = hostName;
            int port = -1;

            var match = s_hostPortString.Match(host);
            if (match.Success)
            {
                host = match.Groups[HostGroupName].Value;
                port = int.Parse(match.Groups[PortGroupName].Value);
            }

            var addresses = GetHostAddresses(host);
            return addresses.Select(ip => ip.ToString())
                            .Where(ip => s_ipString.IsMatch(ip))
                            .Select(ip => ip + ":" + port)
                            .Distinct()
                            .ToArray();
        }

        /// <inheritdoc />
        public string GetHostName()
        {
            return Dns.GetHostName();
        }

        /// <inheritdoc />
        public int GetNextUnusedPort(int minPortIncluded, int maxPortExcluded)
        {
            return NetworkHelper.GetNextUnusedPort(minPortIncluded, maxPortExcluded);
        }

        /// <inheritdoc />
        public ISecureContextToken<int> GetAndReservedNextUnusedPort(int minPortIncluded, int maxPortExcluded)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
