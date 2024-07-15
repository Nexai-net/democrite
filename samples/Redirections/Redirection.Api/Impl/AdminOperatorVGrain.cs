namespace Redirection.Api.Impl
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Node.Abstractions.Administrations;

    using Microsoft.Extensions.Logging;

    using Redirection.Api.VGrain;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class AdminOperatorVGrain : VGrainBase<IAdminOperatorVGrain>, IAdminOperatorVGrain
    {
        #region Fields

        private readonly IVGrainDemocriteSystemProvider _democriteSystemProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminOperatorVGrain"/> class.
        /// </summary>
        public AdminOperatorVGrain(ILogger<IAdminOperatorVGrain> logger,
                                   IVGrainDemocriteSystemProvider democriteSystemProvider)
            : base(logger)
        {
            this._democriteSystemProvider = democriteSystemProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> ApplyGlobalComplementRedirection(IExecutionContext executionContext)
        {
            var grain = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(executionContext);
            return await grain.RequestAppendRedirectionAsync(VGrainInterfaceRedirectionDefinition.Create<IComplementVGrain, IVeryComplementVGrain>(), this.IdentityCard!);
        }

        /// <inheritdoc />
        public async Task<bool> ApplyGlobalConjuctionRedirection(IExecutionContext executionContext)
        {
            var grain = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(executionContext);
            return await grain.RequestAppendRedirectionAsync(VGrainClassNameRedirectionDefinition.Create<ISeparatorComplementVGrain>(typeof(ConjuctionSeparatorComplementVGrain)), this.IdentityCard!);
        }

        /// <inheritdoc />
        public async Task<bool> ClearAllRedirection(IExecutionContext executionContext)
        {
            var allGrain = await GetAllRedirection(executionContext);

            var requests = allGrain.Select(g => g.Uid).Distinct().ToArray();

            if (!requests.Any())
                // No need to clean up, true to not dedicatd behavior
                return true;

            var grain = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(executionContext);
            return await grain.RequestPopRedirectionAsync(requests, this.IdentityCard!);
        }

        /// <inheritdoc />
        public async Task<bool> ClearGlobalComplementRedirection(IExecutionContext executionContext)
        {
            return await ClearGlobalSpecificRedirection(executionContext, typeof(IComplementVGrain));
        }

        /// <inheritdoc />
        public async Task<bool> ClearGlobalConjuctionRedirection(IExecutionContext executionContext)
        {
            return await ClearGlobalSpecificRedirection(executionContext, typeof(ISeparatorComplementVGrain));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<VGrainRedirectionDefinition>> GetAllRedirection(IExecutionContext executionContext)
        {
            var grain = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(executionContext);
            using (var grainToken = executionContext.CancellationToken.ToGrainCancellationTokenSource())
            {
                var redirections = await grain.GetGlobalRedirection(string.Empty, grainToken.Token);
                return redirections.Value.Info;
            }
        }

        #region Tools

        private async Task<bool> ClearGlobalSpecificRedirection(IExecutionContext executionContext, Type redirectionSource)
        {
            var allGrain = await GetAllRedirection(executionContext);

            var request = allGrain.FirstOrDefault(g => g.Source.IsEqualTo(redirectionSource));

            if (request is null)
                // No need to clean up; false due to dedicated request
                return false;

            var grain = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(executionContext);
            return await grain.RequestPopRedirectionAsync(request.Uid, this.IdentityCard!);
        }

        #endregion

        #endregion
    }
}
