// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Orleans.Messaging;

    /// <summary>
    /// Wizard used to setup cluster configuration
    /// </summary>
    public interface IDemocriteClusterBuilder : IBuilderDemocriteBaseWizard, IDemocriteBaseGenericBuilder
    {
        #region Methods

        /// <summary>
        /// Customizes the cluster identifier.
        /// </summary>
        /// <param name="clusterId">The cluster identity. This used to be called DeploymentId before Orleans 2.0 name.</param>
        /// <param name="serviceId">Unique identifier for this service, which should survive deployment and redeployment, where as <see cref="ClusterId"/> might not.</param>
        IDemocriteClusterBuilder CustomizeClusterId(string serviceId, string clusterId);

        /// <summary>
        /// [CLIENT] Adds the gateway list provider, used for the client to get vgrain list from cluster.
        /// </summary>
        IDemocriteClusterBuilder AddGatewayListProvider<TListProvider>() where TListProvider : class, IGatewayListProvider;

        /// <summary>
        /// Adds the configuration validator.
        /// </summary>
        IDemocriteClusterBuilder AddConfigurationValidator<TValidator>() where TValidator : class, IConfigurationValidator;

        /// <summary>
        /// [SERVER] Adds the membership table managment service.
        /// </summary>
        /// <remarks>
        ///     This service is responsible of the node synchronisation that form a cluster bubble
        /// </remarks>
        IDemocriteClusterBuilder AddMembershipTable<TMembership>() where TMembership : class, IMembershipTable;

        #endregion
    }
}
