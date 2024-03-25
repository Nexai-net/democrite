namespace Redirection.Api.Impl
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Orleans.Metadata;

    using Redirection.Api.Models;
    using Redirection.Api.VGrain;

    [GrainType("HappyComplement")]
    public sealed class HappyComplementVGrain : VGrainBase<IComplementVGrain>, IComplementVGrain
    {
        #region Fields

#pragma warning disable IDE1006 // Naming Styles
        private static readonly string[] s_complements = ["super", "great", "fun", "powerfull"];
#pragma warning restore IDE1006 // Naming Styles

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HappyComplementVGrain"/> class.
        /// </summary>
        public HappyComplementVGrain(ILogger<IComplementVGrain> logger)
            : base(logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<TBuilder> PopulateComplement<TBuilder>(TBuilder input, IExecutionContext ctx)
            where TBuilder : ITextBuilder
        {
            input.AppendComplement(s_complements[Random.Shared.Next(0, s_complements.Length)]);
            return Task.FromResult(input);
        }

        #endregion
    }
}
