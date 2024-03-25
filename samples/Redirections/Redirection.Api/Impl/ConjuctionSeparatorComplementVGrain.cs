namespace Redirection.Api.Impl
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Orleans.Metadata;

    using Redirection.Api.Models;
    using Redirection.Api.VGrain;

    public sealed class ConjuctionSeparatorComplementVGrain : VGrainBase<ISeparatorComplementVGrain>, ISeparatorComplementVGrain
    {
        #region Fields

#pragma warning disable IDE1006 // Naming Styles
        private static readonly string[] s_conjuctions = new[] { "but", "or", "so", "not", "also" };
#pragma warning restore IDE1006 // Naming Styles

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SeparatorComplementVGrain"/> class.
        /// </summary>
        public ConjuctionSeparatorComplementVGrain(ILogger<ISeparatorComplementVGrain> logger)
            : base(logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<TBuilder> PopulateComplement<TBuilder>(TBuilder input, IExecutionContext ctx)
            where TBuilder : ITextBuilder
        {
            input.AppendComplement(s_conjuctions[Random.Shared.Next(0, s_conjuctions.Length)]);
            return Task.FromResult(input);
        }

        #endregion
    }
}
