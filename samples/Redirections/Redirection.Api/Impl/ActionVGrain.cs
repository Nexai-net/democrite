namespace Redirection.Api.Impl
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Redirection.Api.Models;
    using Redirection.Api.VGrain;

    public sealed class ActionVGrain : VGrainBase<IActionVGrain>, IActionVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionVGrain"/> class.
        /// </summary>
        public ActionVGrain(ILogger<IActionVGrain> logger)
            : base(logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<TBuilder> PopulateAction<TBuilder>(TBuilder input, IExecutionContext ctx)
            where TBuilder : ITextBuilder
        {
            input.SetAction("is");
            return Task.FromResult(input);
        }

        #endregion
    }
}
