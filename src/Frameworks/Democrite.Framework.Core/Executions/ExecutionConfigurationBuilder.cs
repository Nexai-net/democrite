// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Executions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;

    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Execution Configuraiton builder
    /// </summary>
    /// <seealso cref="Democrite.Framework.Core.Abstractions.IExecutionConfigurationBuilder" />
    internal sealed class ExecutionConfigurationBuilder : IExecutionConfigurationBuilder
    {
        #region Fields

        private readonly Dictionary<RedirectionKey, VGrainRedirectionDefinition> _grainRedirectionDefinitions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionConfigurationBuilder"/> class.
        /// </summary>
        public ExecutionConfigurationBuilder()
        {
            this._grainRedirectionDefinitions = new Dictionary<RedirectionKey, VGrainRedirectionDefinition>();
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

        /// <summary>
        /// Builds configuration
        /// </summary>
        public ExecutionCustomizationDescriptions? Build()
        {
            if (this._grainRedirectionDefinitions.Any())
            {
                return new ExecutionCustomizationDescriptions(this._grainRedirectionDefinitions.Select(kv => new StageVGrainRedirectionDescription(kv.Key.StageUid, kv.Value))
                                                                                               .ToArray());
            }
            return null;
        }

        #endregion
    }
}
