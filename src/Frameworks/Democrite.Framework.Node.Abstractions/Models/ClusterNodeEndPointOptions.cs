// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Options used to define how the current node will bind to network
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ClusterNodeEndPointOptions : INodeOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ClusterNodeEndPointOptions"/> class.
        /// </summary>
        static ClusterNodeEndPointOptions()
        {
            Default = new ClusterNodeEndPointOptions(false, 0, 0, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeEndPointOptions"/> class.
        /// </summary>
        public ClusterNodeEndPointOptions(bool loopback = false,
                                          uint siloPort = 0,
                                          int? gatewayPort = 0,
                                          bool autoGatewayPort = false)
        {
            this.Loopback = loopback;
            this.SiloPort = siloPort;
            this.GatewayPort = gatewayPort;
            this.AutoGatewayPort = autoGatewayPort;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default <see cref="ClusterNodeEndPointOptions"/>.
        /// </summary>
        public static ClusterNodeEndPointOptions Default { get; }

        /// <summary>
        /// Gets a value indicating whether connection must be only loop back.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connection is only loop back; otherwise, <c>false</c> open to extenal.
        /// </value>
        public bool Loopback { get; }

        /// <summary>
        /// Gets the port use on communication node-to-node.
        /// </summary>
        public uint SiloPort { get; }

        /// <summary>
        /// Gets the gateway port use to open communication to external client node-to-client.
        /// </summary>
        public int? GatewayPort { get; }

        /// <summary>
        /// Gets a value indicating whether an automatic gateway port must by provide.
        /// </summary>
        public bool AutoGatewayPort { get; }

        #endregion
    }
}
