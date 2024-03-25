namespace Redirection.Api.Impl
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Redirection.Api.Models;
    using Redirection.Api.VGrain;

    public sealed class VeryComplementVGrain : VGrainBase<IVeryComplementVGrain>, IVeryComplementVGrain
    {
        #region Fields

#pragma warning disable IDE1006 // Naming Styles
        private static readonly string[] s_complements = ["Very Cool", "Very Usefull", "Very Powerfull"];
#pragma warning restore IDE1006 // Naming Styles

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VeryComplementVGrain"/> class.
        /// </summary>
        public VeryComplementVGrain(ILogger<IVeryComplementVGrain> logger)
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
