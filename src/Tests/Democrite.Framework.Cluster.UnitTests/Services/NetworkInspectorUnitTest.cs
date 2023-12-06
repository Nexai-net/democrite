// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.UnitTests.Services
{
    using Democrite.Framework.Cluster.Services;

    using NFluent;

    using System.Net;

    /// <summary>
    /// Test <see cref="NetworkInspector"/>
    /// </summary>
    public sealed class NetworkInspectorUnitTest
    {
        #region Methods

        /// <summary>
        /// Ensure ip string is not resolved
        /// </summary>
        [Theory]
        [InlineData("135.256.32.1", "135.256.32.1", false)]
        [InlineData("135.145.32.1:4256", "135.145.32.1:4256", true)]
        [InlineData("localhost:4256", "127.0.0.1:4256", false)]
        public void SolveHostAddresse_Resolve(string hostName, string? expected, bool ipEndpointParsed)
        {
            var inspector = new NetworkInspector();

            var results = inspector.SolveHostAddresse(hostName);

            if (expected != null)
            {
                Check.That(results).IsNotNull().And.CountIs(1).And.ContainsExactly(expected);

                if (ipEndpointParsed)
                {
                    var parseSuccess = IPEndPoint.TryParse(results.First(), out var parseIp);
                    Check.That(parseSuccess).IsTrue();
                    Check.That(parseIp).IsNotNull();
                    Check.That(parseIp!.Address + ":" + parseIp.Port).IsEqualTo(expected);
                }
            }
            else
            {
                Check.That(results).IsNotNull().And.IsEmpty();
            }
        }

        #endregion
    }
}
