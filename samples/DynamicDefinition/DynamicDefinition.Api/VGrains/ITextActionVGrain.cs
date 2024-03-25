namespace DynamicDefinition.Api.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    [VGrainIdSingleton]
    [VGrainStatelessWorker]
    public interface ITextActionVGrain : IVGrain
    {
        Task<string> Concat(string source, IExecutionContext<string> executionContext);
    }
}
