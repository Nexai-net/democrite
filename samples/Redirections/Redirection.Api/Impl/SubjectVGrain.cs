namespace Redirection.Api.Impl
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Redirection.Api.Models;
    using Redirection.Api.VGrain;

    public sealed class SubjectVGrain : VGrainBase<ISubjectVGrain>, ISubjectVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SubjectVGrain"/> class.
        /// </summary>
        public SubjectVGrain(ILogger<ISubjectVGrain> logger)
            : base(logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<TBuilder> PopulateSubject<TBuilder>(TBuilder input, IExecutionContext ctx)
            where TBuilder : ITextBuilder
        {
            input.SetSubject("Democrite");
            return Task.FromResult(input);
        }

        #endregion
    }
}
