namespace Democrite.Sample.Blackboard.Memory.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;
    using Democrite.Sample.Blackboard.Memory.IVGrains;
    using Democrite.Sample.Blackboard.Memory.Models;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class StoreVGrain : VGrainBase<IStoreVGrain>, IStoreVGrain
    {
        #region Fields

        private readonly IBlackboardProvider _blackboardProvider;
        private IBlackboardRef? _blackboard;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreVGrain"/> class.
        /// </summary>
        public StoreVGrain(ILogger<IStoreVGrain> logger,
                           IBlackboardProvider blackboardProvider)
            : base(logger)
        {
            this._blackboardProvider = blackboardProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<RecordDisplay>> GetAllValuesAsync(IExecutionContext<string> ctx)
        {
            var data = await this._blackboard!.GetAllStoredDataFilteredAsync<double>(".*", ctx.CancellationToken);
            return data.Select(d => new RecordDisplay(d.Data, d.LogicalType, d.Status.ToString())).ToReadOnly();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllMetaDataAsync(IExecutionContext<string> ctx)
        {
            var data = await this._blackboard!.GetAllStoredMetaDataAsync(ctx.CancellationToken);
            return data;
        }

        /// <inheritdoc />
        public async Task PushNewValueAsync<TData>(TData val, IExecutionContext<string> ctx)
        {
            await this._blackboard!.PushNewDataAsync(val, "Values", "Val" + (val?.ToString() ?? string.Empty), RecordStatusEnum.Ready, DataRecordPushRequestTypeEnum.OnlyNew, token: ctx.CancellationToken);
        }

        /// <inheritdoc />
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var ctxStr = base.GetGrainId().Key.ToString()!;

            var blackboard = await this._blackboardProvider.GetBlackboardAsync(ctxStr, "MathBlackboard");
            ArgumentNullException.ThrowIfNull(blackboard);

            this._blackboard = blackboard;

            await base.OnActivateAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Guid> PrepareSlotAsync(string logicalType, IExecutionContext<string> ctx)
        {
            var id = Guid.NewGuid();
            await this._blackboard!.PrepareDataSlotAsync(id, logicalType, "Prepared " + logicalType, ctx.CancellationToken);
            return id;
        }

        #endregion
    }
}
