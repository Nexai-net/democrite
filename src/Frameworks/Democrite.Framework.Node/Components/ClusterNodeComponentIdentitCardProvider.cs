// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Components
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// <see cref="IComponentIdentityCard"/> provider
    /// </summary>
    /// <seealso cref="GrainService" />
    /// <seealso cref="IComponentIdentitCardProvider" />
    internal sealed class ClusterNodeComponentIdentitCardProvider : DemocriteVGrainService, IComponentIdentitCardProvider
    {
        #region Fields

        private static readonly TMPIdentityCard s_tmpIdentityCard = new TMPIdentityCard();

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeComponentIdentitCardProvider"/> class.
        /// </summary>
        public ClusterNodeComponentIdentitCardProvider(GrainId grainId,
                                                       Silo silo,
                                                       ILoggerFactory loggerFactory) 
            : base(grainId, silo, loggerFactory)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<IComponentIdentityCard> GetComponentIdentityCardAsync(GrainId grainId, Guid componentId)
        {
            // TODO : based on component create a correct dedicated component, cache results and synchnized between silo

            return ValueTask.FromResult<IComponentIdentityCard>(s_tmpIdentityCard);
        }

        /// <inheritdoc />
        protected override Task RefreshInfoAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
