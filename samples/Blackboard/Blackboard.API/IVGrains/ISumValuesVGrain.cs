namespace Democrite.Sample.Blackboard.Memory.IVGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    [VGrainStatelessWorker]
    public interface ISumValuesVGrain : IVGrain
    {
        Task<int> Sum(IReadOnlyCollection<int> count, IExecutionContext context);
    }
}
