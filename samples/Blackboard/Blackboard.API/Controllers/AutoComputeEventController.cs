namespace Democrite.Sample.Blackboard.Memory.Controllers
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;
    using Democrite.Framework.Node.Blackboard.VGrains;
    using Democrite.Sample.Blackboard.Memory.Models;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class AutoComputeEventController : BlackboardBaseEventControllerGrain<AutoComputeBlackboardOptions, IAutoComputeEventController>, IAutoComputeEventController
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoComputeEventController"/> class.
        /// </summary>
        public AutoComputeEventController(ILogger<IAutoComputeEventController> logger,
                                          IBlackboardProvider blackboardProvider,
                                          [PersistentState(BlackboardConstants.BlackboardStateStorageKey, BlackboardConstants.BlackboardStateStorageConfigurationKey)] IPersistentState<AutoComputeBlackboardOptions> persistentState) 
            : base(logger, blackboardProvider, persistentState)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task<IReadOnlyCollection<BlackboardCommand>?> OnInitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            if (option is AutoComputeBlackboardOptions auto)
                await PushStateAsync(auto, cancellationToken.CancellationToken);
            return null;
        }

        /// <inheritdoc />
        protected override async Task<IReadOnlyCollection<BlackboardCommand>?> OnReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token)
        {
            if (this.State is null || this.State.ComputeSequenceUid == Guid.Empty)
                return null;

            IReadOnlyCollection<BlackboardCommand>? execCmd = null;
            if (eventBook.StorageEventCount == 0 || eventBook.GetEventStorages(e => e.Action == BlackboardEventStorageTypeEnum.Add || e.Action == BlackboardEventStorageTypeEnum.ChangeStatus).Any())
            {

                var values = await this.Blackboard!.GetAllStoredMetaDataFilteredAsync("Values", token.CancellationToken);

                if (values.Count >= 5)
                    execCmd = new BlackboardCommandTriggerSequence<string>(this.State.ComputeSequenceUid, this.Blackboard.Name).AsEnumerable().ToArray();
            }
            return execCmd;
        }

        #endregion

    }
}
