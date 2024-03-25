namespace Redirection.Api.VGrain
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Metadata;

    [VGrainStatelessWorker]
    [DefaultGrainType("HappyComplement")]
    public interface IComplementVGrain : IComplementBaseVGrain
    {
    }
}
