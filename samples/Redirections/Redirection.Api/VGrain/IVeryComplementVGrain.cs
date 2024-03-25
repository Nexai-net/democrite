namespace Redirection.Api.VGrain
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    [VGrainStatelessWorker]
    public interface IVeryComplementVGrain : IComplementVGrain
    {
    }
}
