// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Orleans.Runtime.Services;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Local (by silo) consumable service to access <see cref="ISignalLocalGrainServiceRelay"/> GrainService
    /// </summary>
    /// <seealso cref="ISignalLocalGrainServiceRelay&gt;" />
    /// <seealso cref="Democrite.Framework.Node.Abstractions.Services.ISignalLocalServiceRelay" />
    internal sealed class SignalLocalGrainServiceRelayClient : GrainServiceClient<ISignalLocalGrainServiceRelay>, ISignalLocalGrainServiceRelayClient
    {
        #region Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalLocalGrainServiceRelayClient"/> class.
        /// </summary>
        public SignalLocalGrainServiceRelayClient(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<SubscriptionId> Subscribe(SignalId signalId)
        {
            return GetService()?.Subscribe(signalId) ?? throw new InvalidOperationException("CurrentGrainReference not initialized");
        }

        /// <inheritdoc />
        public Task Unsubscribe(SubscriptionId subscriptionId)
        {
            // Allow CurrentGrainReference to be missing due to server shutdown
            // The shutdown set CurrentGrainReference to null we want to prevent not necessary exceptions
            return GetService()?.Unsubscribe(subscriptionId) ?? Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ReceiveSignalAsync(SignalMessage signal)
        {
            throw new NotSupportedException("MUST NOT BE CALLED");
        }

        #region Tools

        // For convenience when implementing methods, you can define a property which gets the IDataService
        // corresponding to the grain which is calling the DataServiceClient.
        private ISignalLocalGrainServiceRelay? GetService()
        {
            if (this.CurrentGrainReference is null)
                return null;

            return GetGrainService(this.CurrentGrainReference.GrainId);
        }

        #endregion

        #endregion
    }
}
