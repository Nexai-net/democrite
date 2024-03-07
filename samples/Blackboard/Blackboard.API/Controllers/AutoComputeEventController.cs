namespace Democrite.Sample.Blackboard.Memory.Controllers
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;
    using Democrite.Sample.Blackboard.Memory.Models;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class AutoComputeEventController : VGrainBase<AutoComputeBlackboardOptions, IAutoComputeEventController>, IAutoComputeEventController
    {
        #region Fields
        
        private readonly IBlackboardProvider _blackboardProvider;
        private IBlackboardRef? _blackboard;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoComputeEventController"/> class.
        /// </summary>
        public AutoComputeEventController(ILogger<IAutoComputeEventController> logger,
                                          IBlackboardProvider blackboardProvider,
                                          [PersistentState(BlackboardConstants.BlackboardStorageStateKey, BlackboardConstants.BlackboardStorageConfigurationKey)] IPersistentState<AutoComputeBlackboardOptions> persistentState) 
            : base(logger, persistentState)
        {
            this._blackboardProvider = blackboardProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task InitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            if (option is AutoComputeBlackboardOptions auto)
                await PushStateAsync(auto, cancellationToken.CancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ReactToEventsAsync(IReadOnlyCollection<BlackboardEvent> events, GrainCancellationToken token)
        {
            if (this.State is null || this.State.ComputeSequenceUid == Guid.Empty)
                return null;

            if (!events.Any(e => e.EventType == BlackboardEventTypeEnum.Storage && e is BlackboardEventStorage eStorage && eStorage.Action == BlackboardEventStorageTypeEnum.Add && eStorage.Metadata?.LogicalType == "Values"))
                return null;

            var values = await this._blackboard!.GetAllStoredMetaDataByTypeAsync("Values", token.CancellationToken);

            IReadOnlyCollection<BlackboardCommand>? execCmd = null;

            if (values.Count >= 5)
                execCmd = new BlackboardCommandTriggerSequence<string>(this.State.ComputeSequenceUid, this._blackboard.Name).AsEnumerable().ToArray();

            return execCmd;
        }

        /// <inheritdoc />
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var bbuid = base.GetGrainId().GetGuidKey();
            this._blackboard = await this._blackboardProvider.GetBlackboardAsync(bbuid, cancellationToken);

            await base.OnActivateAsync(cancellationToken);
        }

        #endregion

    }
}
