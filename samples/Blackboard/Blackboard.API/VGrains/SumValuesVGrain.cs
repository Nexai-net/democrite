namespace Democrite.Sample.Blackboard.Memory.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Sample.Blackboard.Memory.IVGrains;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    [StatelessWorker]
    public sealed class SumValuesVGrain : VGrainBase<ISumValuesVGrain>, ISumValuesVGrain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SumValuesVGrain"/> class.
        /// </summary>
        public SumValuesVGrain(ILogger<ISumValuesVGrain> logger) 
            : base(logger)
        {
        }

        public Task<int> Sum(IReadOnlyCollection<int> count, IExecutionContext context)
        {
            return Task.FromResult(count.Sum());
        }
    }
}
