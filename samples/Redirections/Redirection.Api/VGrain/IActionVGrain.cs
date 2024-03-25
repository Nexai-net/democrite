namespace Redirection.Api.VGrain
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Redirection.Api.Models;

    [VGrainStatelessWorker]
    public interface IActionVGrain : IVGrain
    {
        Task<TBuilder> PopulateAction<TBuilder>(TBuilder input, IExecutionContext ctx) where TBuilder : ITextBuilder;
    }
}
