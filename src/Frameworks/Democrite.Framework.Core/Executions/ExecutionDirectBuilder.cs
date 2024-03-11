// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Executions
{
    using Democrite.Framework.Core.Abstractions;
    using Elvex.Toolbox;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <inheritdoc />
    internal readonly struct ExecutionDirectBuilder<TVGrain> : IExecutionDirectBuilder<TVGrain>
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly ILogger<IDemocriteExecutionHandler> _logger;
        private readonly IVGrainProvider _vgrainProvider;
        private readonly IdSpan? _targetForceGrainId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionDirectBuilder{TVGrain}"/> class.
        /// </summary>
        public ExecutionDirectBuilder(IVGrainProvider vgrainProvider,
                                      ILogger<IDemocriteExecutionHandler> logger,
                                      IdSpan? targetForceGrainId = null)
        {
            this._vgrainProvider = vgrainProvider;
            this._logger = logger;
            this._targetForceGrainId = targetForceGrainId;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, IExecutionContext, Task<TResult>>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, NoneType, NoneType, TResult>(this._logger,
                                                                                    this._vgrainProvider,
                                                                                    null,
                                                                                    null,
                                                                                    call,
                                                                                    this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionLauncher Call(Expression<Func<TVGrain, IExecutionContext, Task>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, NoneType, NoneType, NoneType>(this._logger,
                                                                                     this._vgrainProvider,
                                                                                     null,
                                                                                     null,
                                                                                     call,
                                                                                     this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionDirectBuilderWithConfiguration<TVGrain, TConfig> SetConfiguration<TConfig>(TConfig? config)
        {
            return new ExecutionDirectBuilderWithConfiguration<TVGrain, TConfig>(this._vgrainProvider,
                                                                                this._logger,
                                                                                config,
                                                                                this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain, TInput> SetInput<TInput>(TInput? input)
        {
            return new ExecutionDirectBuilder<TVGrain, TInput>(this._vgrainProvider,
                                                               this._logger,
                                                               input,
                                                               this._targetForceGrainId);
        }

        #endregion
    }

    /// <inheritdoc />
    file readonly struct ExecutionDirectBuilder<TVGrain, TInput> : IExecutionDirectBuilder<TVGrain, TInput>
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly ILogger<IDemocriteExecutionHandler> _logger;
        private readonly IVGrainProvider _vgrainProvider;
        private readonly TInput? _input;
        private readonly IdSpan? _targetForceGrainId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionDirectBuilder{TVGrain, TInput}"/> class.
        /// </summary>
        public ExecutionDirectBuilder(IVGrainProvider vgrainProvider,
                                      ILogger<IDemocriteExecutionHandler> logger,
                                      TInput? intput,
                                      IdSpan? targetForceGrainId)
        {
            this._vgrainProvider = vgrainProvider;
            this._logger = logger;
            this._input = intput;
            this._targetForceGrainId = targetForceGrainId;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, TInput, IExecutionContext, Task<TResult>>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, TInput, NoneType, TResult>(this._logger,
                                                                                  this._vgrainProvider,
                                                                                  null,
                                                                                  this._input,
                                                                                  call,
                                                                                  this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionLauncher Call(Expression<Func<TVGrain, TInput, IExecutionContext, Task>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, TInput, NoneType, NoneType>(this._logger,
                                                                                   this._vgrainProvider,
                                                                                   null,
                                                                                   this._input,
                                                                                   call,
                                                                                   this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig> SetConfiguration<TConfig>(TConfig? config)
        {
            return new ExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig>(this._vgrainProvider,
                                                                                        this._logger,
                                                                                        this._input,
                                                                                        config,
                                                                                        this._targetForceGrainId);
        }

        #endregion
    }

    /// <inheritdoc />
    file readonly struct ExecutionDirectBuilderWithConfiguration<TVGrain, TConfig> : IExecutionDirectBuilderWithConfiguration<TVGrain, TConfig>
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly ILogger<IDemocriteExecutionHandler> _logger;
        private readonly IVGrainProvider _vgrainProvider;
        private readonly TConfig? _config;
        private readonly IdSpan? _targetForceGrainId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionDirectBuilderWithConfiguration{TVGrain}"/> class.
        /// </summary>
        public ExecutionDirectBuilderWithConfiguration(IVGrainProvider vgrainProvider,
                                                       ILogger<IDemocriteExecutionHandler> logger,
                                                       TConfig? config,
                                                       IdSpan? targetForceGrainId)
        {
            this._vgrainProvider = vgrainProvider;
            this._logger = logger;
            this._config = config;
            this._targetForceGrainId = targetForceGrainId;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, IExecutionContext<TConfig>, Task<TResult>>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, NoneType, TConfig, TResult>(this._logger,
                                                                                   this._vgrainProvider,
                                                                                   this._config,
                                                                                   null,
                                                                                   call, 
                                                                                   this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionLauncher Call(Expression<Func<TVGrain, IExecutionContext<TConfig>, Task>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, NoneType, TConfig, NoneType>(this._logger,
                                                                                    this._vgrainProvider,
                                                                                    this._config,
                                                                                    null,
                                                                                    call, 
                                                                                    this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig> SetInput<TInput>(TInput? input)
        {
            return new ExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig>(this._vgrainProvider,
                                                                                        this._logger,
                                                                                        input,
                                                                                        this._config,
                                                                                        this._targetForceGrainId);
        }

        #endregion
    }

    /// <inheritdoc />
    file readonly struct ExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig> : IExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig>
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly ILogger<IDemocriteExecutionHandler> _logger;
        private readonly IVGrainProvider _vgrainProvider;
        private readonly TConfig? _config;
        private readonly IdSpan? _targetForceGrainId;
        private readonly TInput? _input;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionDirectBuilder{TVGrain, TInput}"/> class.
        /// </summary>
        public ExecutionDirectBuilderWithConfiguration(IVGrainProvider vgrainProvider,
                                                       ILogger<IDemocriteExecutionHandler> logger,
                                                       TInput? intput,
                                                       TConfig? config,
                                                       IdSpan? targetForceGrainId)
        {
            this._vgrainProvider = vgrainProvider;
            this._logger = logger;
            this._input = intput;
            this._config = config;
            this._targetForceGrainId = targetForceGrainId;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, TInput, IExecutionContext<TConfig>, Task<TResult>>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, TInput, TConfig, TResult>(this._logger,
                                                                                 this._vgrainProvider,
                                                                                 this._config,
                                                                                 this._input,
                                                                                 call,
                                                                                 this._targetForceGrainId);
        }

        /// <inheritdoc />
        public IExecutionLauncher Call(Expression<Func<TVGrain, TInput, IExecutionContext<TConfig>, Task>> call)
        {
            return new ExecutionDirectLauncher<TVGrain, TInput, TConfig, NoneType>(this._logger,
                                                                                  this._vgrainProvider,
                                                                                  this._config,
                                                                                  this._input,
                                                                                  call,
                                                                                  this._targetForceGrainId);
        }

        #endregion
    }
}
