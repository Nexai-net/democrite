namespace Democrite.Sample.Blackboard.Memory.IVGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Sample.Blackboard.Memory.Models;

    using System.Threading.Tasks;

    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.Configuration}")]
    public interface IStoreVGrain : IVGrain
    {
        Task<Guid> PrepareSlotAsync(string logicalType, IExecutionContext<string> ctx);

        Task PushNewValueAsync<TVal>(TVal val, IExecutionContext<string> ctx);

        Task<IReadOnlyCollection<RecordDisplay>> GetAllValuesAsync(IExecutionContext<string> ctx);

        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllMetaDataAsync(IExecutionContext<string> ctx);
    }
}
