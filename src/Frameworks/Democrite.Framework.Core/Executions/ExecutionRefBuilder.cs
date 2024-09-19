// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Executions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models.References;
    using Democrite.Framework.Core.Abstractions.References;

    using Elvex.Toolbox;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal readonly struct ExecutionRefBuilder : IExecutionRefBuilder
    {
        #region Fields

        private readonly IGrainFactory _grainFactory;
        private readonly RefVGrainQuery _refVGrain;
        private readonly IdSpan? _forceGrainId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionRefBuilder"/> class.
        /// </summary>
        public ExecutionRefBuilder(IGrainFactory grainFactory, RefVGrainQuery refVGrain, in IdSpan? forceGrainId)
        {
            this._grainFactory = grainFactory;
            this._refVGrain = refVGrain;
            this._forceGrainId= forceGrainId;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionRefLauncher Call(RefMethodQuery refMethod)
        {
            ArgumentNullException.ThrowIfNull(refMethod);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(refMethod.SimpleNameIdentifier);

            var cmd = new RefVGrainExecuteCommand<NoneType, NoneType>(this._refVGrain.ToUri(), refMethod.SimpleNameIdentifier, ForceId: this._forceGrainId);
            return new ExecutionRefLauncher<RefVGrainExecuteCommand<NoneType, NoneType>>(this._grainFactory, cmd);
        }

        /// <inheritdoc />
        public IExecutionRefBuilder SetConfiguration<TConfig>(TConfig? config)
        {
            return new ExecutionRefBuilder<NoneType, TConfig>(this._grainFactory, this._refVGrain, NoneType.Instance, config, this._forceGrainId);
        }

        /// <inheritdoc />
        public IExecutionRefBuilder SetInput<TInput>(TInput? input)
        {
            return new ExecutionRefBuilder<TInput, NoneType>(this._grainFactory, this._refVGrain, input, NoneType.Instance, this._forceGrainId);
        }

        #endregion
    }

    internal struct ExecutionRefBuilder<TInput, TConfig> : IExecutionRefBuilder
    {
        #region Fields

        private readonly IGrainFactory _grainFactory;
        private readonly IdSpan? _forceGrainId;
        private readonly RefVGrainQuery _refVGrain;

        private TInput? _input;
        private TConfig? _config;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionRefBuilder"/> class.
        /// </summary>
        public ExecutionRefBuilder(IGrainFactory grainFactory, RefVGrainQuery refVGrain, TInput? input, TConfig? config, in IdSpan? forceGrainId)
        {
            this._grainFactory = grainFactory;
            this._forceGrainId = forceGrainId;
            this._refVGrain = refVGrain;
            this._input = input;
            this._config = config;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionRefLauncher Call(RefMethodQuery refMethod)
        {
            ArgumentNullException.ThrowIfNull(refMethod);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(refMethod.SimpleNameIdentifier);

            var cmd = new RefVGrainExecuteCommand<TInput, TConfig>(this._refVGrain.ToUri(), refMethod.SimpleNameIdentifier, this._input, this._config, this._forceGrainId);
            return new ExecutionRefLauncher<RefVGrainExecuteCommand<TInput, TConfig>>(this._grainFactory, cmd);
        }

        /// <inheritdoc />
        public IExecutionRefBuilder SetConfiguration<TOtherConfig>(TOtherConfig? config)
        {
            if (config is TConfig localCfg)
            {
                this._config = localCfg;
                return this;
            }

            return new ExecutionRefBuilder<TInput, TOtherConfig>(this._grainFactory, this._refVGrain, this._input, config, this._forceGrainId);
        }

        /// <inheritdoc />
        public IExecutionRefBuilder SetInput<TOtherInput>(TOtherInput? input)
        {
            if (input is TInput localInput)
            {
                this._input = localInput;
                return this;
            }

            return new ExecutionRefBuilder<TOtherInput, TConfig>(this._grainFactory, this._refVGrain, input, this._config, this._forceGrainId);
        }

        #endregion
    }
}
