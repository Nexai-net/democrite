// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit
{
    using Democrite.UnitTests.ToolKit.Remoting;

    using Orleans.TestingHost;

    using System;

    /// <summary>
    /// Extension to enhance testing through 
    /// </summary>
    public static class TestClusterBuilderExtensions
    {
        /// <summary>
        /// Adds the mock service remotly control through <see cref="AppDomain"/>
        /// </summary>
        public static TestClusterBuilder AddRemoteMockService(this TestClusterBuilder inst, Action<ITestRemotingServiceBuilder> cfg)
        {
            var instCfg = new TestRemotingServiceBuilder(inst);
            cfg(instCfg);
            return inst;
        }
    }
}
