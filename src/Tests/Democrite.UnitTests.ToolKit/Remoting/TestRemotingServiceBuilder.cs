// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Remoting
{
    using Orleans.TestingHost;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builder used to setup all the remoting call
    /// </summary>
    /// <seealso cref="ITestRemotingServiceBuilder" />
    internal sealed class TestRemotingServiceBuilder : ITestRemotingServiceBuilder
    {
        #region Fields

        /// <summary>
        /// Gets the prefix configuration keys.
        /// </summary>
        internal static string PrefixConfigKeys
        {
            get { return nameof(IRemoteControllerService); }
        }

        // Static hashset to ensure test tunning in parallel will not use the same communication name pipe
        private static readonly HashSet<string> s_uniqueContextKeyHandler;

        private readonly List<IRemoteControllerService> _remoteControllers;
        private readonly TestClusterBuilder _testClusterBuilder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TestRemotingServiceBuilder"/> class.
        /// </summary>
        static TestRemotingServiceBuilder()
        {
            s_uniqueContextKeyHandler = new HashSet<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRemotingServiceBuilder"/> class.
        /// </summary>
        public TestRemotingServiceBuilder(TestClusterBuilder testClusterBuilder)
        {
            this._testClusterBuilder = testClusterBuilder;
            this._remoteControllers = new List<IRemoteControllerService>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ITestRemotingServiceBuilder AddSingleton<TService>(TService service)
            where TService : class
        {
            ArgumentNullException.ThrowIfNull(service);

            var uniqueInstanceKey = GetUniqueKey();

            var remoteController = new RemoteControllerService<TService>(uniqueInstanceKey, service);

            this._testClusterBuilder.Properties.TryAdd(PrefixConfigKeys + "_" + remoteController.Uid, typeof(TService).AssemblyQualifiedName);

            this._remoteControllers.Add(remoteController);

            return this;
        }

        /// <inheritdoc />
        public ITestRemotingService Build()
        {
            if (this._remoteControllers.Any())
                this._testClusterBuilder.AddSiloBuilderConfigurator<RemoteTestClusterServicesConfiguration>();

            return new TestRemotingService(this._remoteControllers);
        }

        /// <summary>
        /// Gets the unique key.
        /// </summary>
        private static string GetUniqueKey()
        {
            lock (s_uniqueContextKeyHandler)
            {
                string instKey = string.Empty;

                do
                {
                    instKey = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);

                } while (s_uniqueContextKeyHandler.Contains(instKey));

                s_uniqueContextKeyHandler.Add(instKey);

                return instKey;
            }
        }

        #endregion
    }
}
