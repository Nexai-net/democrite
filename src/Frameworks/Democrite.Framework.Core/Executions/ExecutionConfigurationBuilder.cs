// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Executions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Execution Configuraiton builder
    /// </summary>
    /// <seealso cref="IExecutionConfigurationBuilder" />
    public sealed class ExecutionConfigurationBuilder : IExecutionConfigurationBuilder
    {
        #region Fields

        private readonly Dictionary<RedirectionKey, VGrainRedirectionDefinition> _grainRedirectionDefinitions;
        private readonly Dictionary<Guid, EndSignalFireDescription> _endSignalResult;
        private bool _preventSequenceExecutorStateStorage;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionConfigurationBuilder"/> class.
        /// </summary>
        public ExecutionConfigurationBuilder()
        {
            this._grainRedirectionDefinitions = new Dictionary<RedirectionKey, VGrainRedirectionDefinition>();
            this._endSignalResult = new Dictionary<Guid, EndSignalFireDescription>();
        }

        #endregion

        #region Nested

        private record struct RedirectionKey(Guid? StageUid, ConcretType GrainToRedirect);

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionConfigurationBuilder RedirectGrain<TOldGrain, TNewGrain>(params Guid[] stageUids)
            where TOldGrain : IVGrain
            where TNewGrain : TOldGrain
        {
            var sourceConcretType = (ConcretType)typeof(TOldGrain).GetAbstractType();
            var redirection = VGrainInterfaceRedirectionDefinition.Create<TOldGrain, TNewGrain>();

            if (stageUids is not null && stageUids.Any())
            {
                foreach (var stageUid in stageUids)
                {
                    var key = new RedirectionKey(stageUid, sourceConcretType);
                    this._grainRedirectionDefinitions.Add(key, redirection);
                }
            }
            else
            {
                var key = new RedirectionKey(null, sourceConcretType);
                this._grainRedirectionDefinitions.Add(key, redirection);
            }

            return this;
        }

        /// <inheritdoc />
        public IExecutionConfigurationBuilder ResultSignal(in SignalId signalId, bool includeResult = false)
        {
            this._endSignalResult[signalId.Uid] = new EndSignalFireDescription(signalId, includeResult);
            return this;
        }

        /// <inheritdoc />
        public IExecutionConfigurationBuilder PreventSequenceExecutorStateStorage()
        {
            this._preventSequenceExecutorStateStorage = true;
            return this;
        }

        /// <summary>
        /// Builds configuration
        /// </summary>
        public ExecutionCustomizationDescriptions? Build()
        {
            if (this._grainRedirectionDefinitions.Any() || this._endSignalResult.Any() || this._preventSequenceExecutorStateStorage)
            {
                return new ExecutionCustomizationDescriptions(this._grainRedirectionDefinitions.Select(kv => new StageVGrainRedirectionDescription(kv.Key.StageUid, kv.Value))
                                                                                               .ToArray(),

                                                              this._endSignalResult.Values.ToArray(),
                                                              DeferredId: null,
                                                              PreventSequenceExecutorStateStorage: this._preventSequenceExecutorStateStorage);
            }

            return null;
        }

        #endregion
    }
}
