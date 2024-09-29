// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Node.Abstractions.Configurations;
    using Democrite.Framework.Node.Models;

    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Configuration;

    using System;
    using System.Diagnostics;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IDemocriteNodeClusterOptionWizard" />
    internal sealed class DemocriteNodeClusterOptionWizard : IDemocriteNodeClusterOptionWizard
    {
        #region Fields

        private readonly IServiceCollection _serviceDescriptors;
        private bool _clusterMembershipOptions = false;

        private TimeSpan _defunctSiloExpiration;
        private TimeSpan _defunctSiloCleanupPeriod;
        private bool _clusterNodeOptions;
        private bool _addConsoleSiloInfo;
        private readonly TimeSpan _iamAliveTablePublishTimeout;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteNodeClusterOptionWizard"/> class.
        /// </summary>
        public DemocriteNodeClusterOptionWizard(IServiceCollection serviceDescriptors)
        {
            this._serviceDescriptors = serviceDescriptors;

            this._defunctSiloExpiration = TimeSpan.FromMinutes(30);
            this._defunctSiloCleanupPeriod = TimeSpan.FromMinutes(10);
            this._iamAliveTablePublishTimeout = TimeSpan.FromMinutes(2);

            if (Debugger.IsAttached)
            {
                this._defunctSiloExpiration = TimeSpan.FromMinutes(15);
                this._defunctSiloCleanupPeriod = TimeSpan.FromMinutes(2);
                this._iamAliveTablePublishTimeout = TimeSpan.FromSeconds(30);
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDemocriteNodeClusterOptionWizard MembershipTableCleanup(TimeSpan expirationDelay, TimeSpan experiationCheckPeriod)
        {
            this._clusterMembershipOptions = true;

            this._defunctSiloExpiration = expirationDelay;
            this._defunctSiloCleanupPeriod = experiationCheckPeriod;

            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeClusterOptionWizard AddConsoleSiloTitleInfo()
        {
            this._clusterNodeOptions = true;
            this._addConsoleSiloInfo = true;
            return this;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public void Build()
        {
            if (this._clusterMembershipOptions)
            {
                this._serviceDescriptors.Configure<ClusterMembershipOptions>(a =>
                {
                    a.DefunctSiloExpiration = this._defunctSiloExpiration;
                    a.DefunctSiloCleanupPeriod = this._defunctSiloCleanupPeriod;
                    a.IAmAliveTablePublishTimeout = this._iamAliveTablePublishTimeout;
                });
            }

            if (this._clusterNodeOptions)
            {
                this._serviceDescriptors.Configure<ClusterNodeOptions>(c => c.AddSiloInformationToConsole = this._addConsoleSiloInfo);
            }
        }
        
        #endregion
    }
}
