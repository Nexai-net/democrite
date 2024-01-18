﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Executions;
    using Democrite.Framework.Toolbox;

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
        private readonly IVGrainProvider _vgrainProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteExecutionHandler"/> class.
        /// </summary>
        public DemocriteExecutionHandler(IVGrainProvider vgrainProvider,
                                         ILogger<IDemocriteExecutionHandler>? logger = null)
        {
            this._vgrainProvider = vgrainProvider;
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
        public IExecutionLauncher Sequence(Guid sequenceId)
        {
            return new ExecutionBuilderLauncher<ISequenceExecutorVGrain, NoneType>(sequenceId,
                                                                             this._vgrainProvider,
                                                                             this._logger);
        }

        /// <inheritdoc />
        public IExecutionBuilder<TInput> Sequence<TInput>(Guid sequenceId)
        {
            return new ExecutionBuilderLauncher<ISequenceExecutorVGrain, TInput>(sequenceId,
                                                                           this._vgrainProvider,
                                                                           this._logger);
        }

        /// <inheritdoc />
        public IExecutionBuilder<object> SequenceWithInput(Guid sequenceId)
        {
            return new ExecutionBuilderLauncher<ISequenceExecutorVGrain, object>(sequenceId,
                                                                                 this._vgrainProvider,
                                                                                 this._logger);
        }

        #region Tools

        /// <inheritdoc />
        private IExecutionDirectBuilder<TVGrain> VGrainImpl<TVGrain>(IdSpan? forcedGrainId)
            where TVGrain : IVGrain
        {
            return new ExecutionDirectBuilder<TVGrain>(this._vgrainProvider, this._logger, forcedGrainId);
        }

        #endregion

        #endregion
    }
}
