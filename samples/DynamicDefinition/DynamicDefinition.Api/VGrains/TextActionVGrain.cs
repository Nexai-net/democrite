namespace DynamicDefinition.Api.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using System.Threading.Tasks;

    public sealed class TextActionVGrain : VGrainBase<ITextActionVGrain>, ITextActionVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TextActionVGrain"/> class.
        /// </summary>
        public TextActionVGrain(ILogger<ITextActionVGrain> logger)
            : base(logger)
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<string> Concat(string source, IExecutionContext<string> executionContext)
        {
            if (string.IsNullOrEmpty(source))
                return Task.FromResult(executionContext.Configuration!);

            return Task.FromResult(source + " " + executionContext.Configuration);
        }

        #endregion
    }
}
