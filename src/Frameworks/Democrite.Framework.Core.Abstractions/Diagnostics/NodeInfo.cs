// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    using System;
    using System.ComponentModel;

    [Immutable]
    [Serializable]
    [ImmutableObject(true)]
    [GenerateSerializer]
    public sealed class NodeInfo
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeInfo"/> class.
        /// </summary>
        public NodeInfo(string clusterId, string dnsHostName, string nodeAddress)
        {
            this.ClusterId = clusterId;
            this.DnsHostName = dnsHostName;
            this.NodeAddress = nodeAddress;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        [Id(0)]
        public string ClusterId { get; }

        /// <summary>
        /// Gets the name of the DNS host.
        /// </summary>
        [Id(1)]
        public string DnsHostName { get; }

        /// <summary>
        /// Gets the node address.
        /// </summary>
        [Id(2)]
        public string NodeAddress { get; }

        #endregion
    }
}
