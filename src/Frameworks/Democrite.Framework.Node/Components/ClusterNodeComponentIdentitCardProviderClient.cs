// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Components
{
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Runtime.Services;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// <see cref="IComponentIdentityCard"/> provider
    /// </summary>
    /// <seealso cref="Orleans.Runtime.GrainService" />
    /// <seealso cref="IComponentIdentitCardProvider" />
    internal sealed class ClusterNodeComponentIdentitCardProviderClient : GrainServiceClient<IComponentIdentitCardProvider>, IComponentIdentitCardProviderClient
    {
        #region Fields

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeComponentIdentitCardProviderClient"/> class.
        /// </summary>
        public ClusterNodeComponentIdentitCardProviderClient(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion

        #region Properties

        // For convenience when implementing methods, you can define a property which gets the IDataService
        // corresponding to the grain which is calling the IComponentIdentitCardProviderClient.
        private IComponentIdentitCardProvider GrainService
        {
            get { return GetGrainService(this.CurrentGrainReference.GrainId); }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<IComponentIdentityCard> GetComponentIdentityCardAsync(GrainId grainId, Guid componentId)
        {
            return this.GrainService.GetComponentIdentityCardAsync(grainId, componentId);
        }

        #endregion
    }
}
