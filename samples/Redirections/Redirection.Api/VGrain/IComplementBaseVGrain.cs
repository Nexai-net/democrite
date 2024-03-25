namespace Redirection.Api.VGrain
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Metadata;

    using Redirection.Api.Models;

    [VGrainStatelessWorker]
    public interface IComplementBaseVGrain : IVGrain
    {
        Task<TBuilder> PopulateComplement<TBuilder>(TBuilder input, IExecutionContext ctx) where TBuilder : ITextBuilder;
    }
}
