// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Abstractions.Models
{
    using Elvex.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Option that contains all the information about cluster to connect to
    /// </summary>
    /// <seealso cref="IClientOptions" />
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ClusterClientOptions : IClientOptions
    {
        #region Fields

        public const string ConfiguratioName = "Cluster";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ClusterClientOptions"/> class.
        /// </summary>
        static ClusterClientOptions()
        {
            Default = new ClusterClientOptions(new[]
            {
                IPAddress.Loopback + ":30000"
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterClientOptions"/> class.
        /// </summary>
        public ClusterClientOptions(IEnumerable<string>? endpoints = null)
        {
            this.Endpoints = (endpoints?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static ClusterClientOptions Default { get; }

        /// <summary>
        /// Gets the clusters endpoints.
        /// </summary>
        public IReadOnlyCollection<string> Endpoints { get; }

        #endregion

    }
}
