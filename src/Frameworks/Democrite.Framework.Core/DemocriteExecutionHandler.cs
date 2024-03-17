// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Executions;
    using Elvex.Toolbox;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Default implementation of <see cref="IDemocriteExecutionHandler"/>
    /// </summary>
    /// <seealso cref="IDemocriteExecutionHandler" />
    internal sealed class DemocriteExecutionHandler : IDemocriteExecutionHandler
    {
        #region Fields

        private readonly ILogger<IDemocriteExecutionHandler> _logger;
        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly IVGrainProvider _vgrainProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteExecutionHandler"/> class.
        /// </summary>
        public DemocriteExecutionHandler(IVGrainProvider vgrainProvider,
                                         IDemocriteSerializer democriteSerializer,
                                         ILogger<IDemocriteExecutionHandler>? logger = null)
        {
            this._vgrainProvider = vgrainProvider;
            this._democriteSerializer = democriteSerializer;
            this._logger = logger ?? NullLogger<IDemocriteExecutionHandler>.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>()
            where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(null);
        }

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(Guid id, string? customIdPart = null)
            where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(GrainIdKeyExtensions.CreateGuidKey(id, customIdPart));
        }

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(long id, string? customIdPart = null) where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(GrainIdKeyExtensions.CreateIntegerKey(id, customIdPart));
        }

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(string id) where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(IdSpan.Create(id));
        }

        /// <inheritdoc />
        public IExecutionLauncher Sequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            return SequenceImpl<NoneType>(sequenceId, cfgBuilder);
        }

        /// <inheritdoc />
        public IExecutionBuilder<TInput> Sequence<TInput>(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            return SequenceImpl<TInput>(sequenceId, cfgBuilder);
        }

        /// <inheritdoc />
        public IExecutionBuilder<object> SequenceWithInput(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            return SequenceImpl<object>(sequenceId, cfgBuilder);
        }

        #region Tools

        /// <inheritdoc />
        private IExecutionDirectBuilder<TVGrain> VGrainImpl<TVGrain>(IdSpan? forcedGrainId)
            where TVGrain : IVGrain
        {
            return new ExecutionDirectBuilder<TVGrain>(this._vgrainProvider, this._logger, forcedGrainId);
        }

        /// <inheritdoc />
        public ExecutionBuilderLauncher<ISequenceExecutorVGrain, TInput> SequenceImpl<TInput>(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            ExecutionCustomizationDescriptions? executionCustomization = null;

            if (cfgBuilder != null)
            {
                var builder = new ExecutionConfigurationBuilder();
                cfgBuilder.Invoke(builder);

                // To config definition
                executionCustomization = builder.Build();
            }

            return new ExecutionBuilderLauncher<ISequenceExecutorVGrain, TInput>(sequenceId,
                                                                                 this._vgrainProvider,
                                                                                 this._logger,
                                                                                 this._democriteSerializer,
                                                                                 executionCustomization);
        }

        #endregion

        #endregion
    }
}
