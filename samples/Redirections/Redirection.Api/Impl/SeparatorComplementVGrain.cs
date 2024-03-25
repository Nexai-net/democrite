namespace Redirection.Api.Impl
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Redirection.Api.Models;
    using Redirection.Api.VGrain;

    public sealed class SeparatorComplementVGrain : VGrainBase<ISeparatorComplementVGrain>, ISeparatorComplementVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SeparatorComplementVGrain"/> class.
        /// </summary>
        public SeparatorComplementVGrain(ILogger<ISeparatorComplementVGrain> logger)
            : base(logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<TBuilder> PopulateComplement<TBuilder>(TBuilder input, IExecutionContext ctx)
            where TBuilder : ITextBuilder
        {
            input.AppendComplement("and");
            return Task.FromResult(input);
        }

        #endregion
    }
}
