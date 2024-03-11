// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Executions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Models;
    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Launcher using a executor and a definition like sequence to execute the request
    /// </summary>
    /// <typeparam name="TInput">The type of the inuput.</typeparam>
    /// <typeparam name="TExecutor">Executor id that expect in context an Guid id to found execution information</typeparam>
    internal sealed class ExecutionBuilderLauncher<TExecutor, TInput> : ExecutionBaseLauncher<TExecutor, NoneType>,
                                                                        IExecutionLauncher,
                                                                        IExecutionBuilder,
                                                                        IExecutionBuilder<TInput>
        where TExecutor : IGenericContextedExecutor<Guid>, IAddressable, IVGrain
    {
        #region Fields

        private readonly Guid _executionSchemaId;

        private object? _genericInput;
        private TInput? _input;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionBuilderLauncher{TExecutor, TInput, TResult}"/> class.
        /// </summary>
        public ExecutionBuilderLauncher(Guid executionSchemaId,
                                        IVGrainProvider vgrainProvider,
                                        ILogger? logger)
            : base(logger, vgrainProvider)
        {
            ArgumentNullException.ThrowIfNull(executionSchemaId);
            ArgumentNullException.ThrowIfNull(vgrainProvider);

            this._executionSchemaId = executionSchemaId;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        IExecutionLauncher IExecutionBuilder<TInput>.SetInput(TInput? input)
        {
            this._genericInput = null;
            this._input = input;

            return this;
        }

        /// <inheritdoc />
        IExecutionLauncher IExecutionBuilder.SetInput(object? input)
        {
            this._genericInput = input;
            this._input = default;

            return this;
        }

        #region Tools

        /// <inheritdoc />
        protected override string? OnFireFailedBuildContext(Exception ex)
        {
            var errorContext = new StringBuilder();

            errorContext.Append("(executionSchemaId : ");
            errorContext.Append(this._executionSchemaId);
            errorContext.Append(")");

            if (NoneType.IsEqualTo<TInput>() == false)
            {
                errorContext.Append("(input : ");

                if (this._input is ISupportDebugDisplayName supportInputDebugDisplayConfig)
                    errorContext.Append(supportInputDebugDisplayConfig.ToDebugDisplayName());
                else
                    errorContext.Append(this._input);

                errorContext.Append(")");
            }

            return errorContext.ToString();
        }

        /// <inheritdoc />
        protected override object? GetInput()
        {
            return this._genericInput ?? this._input;
        }

        /// <inheritdoc />
        protected override IExecutionContext GenerateExecutionContext()
        {
            return new ExecutionContextWithConfiguration<Guid>(Guid.NewGuid(),
                                                               Guid.NewGuid(),
                                                               null,
                                                               this._executionSchemaId);
        }

        /// <inheritdoc />
        protected override Task OnRunAsync<TExpectedOutput>(TExecutor executor, object? input, IExecutionContext executionContextBuilded, bool fire)
        {
            var executionContext = (IExecutionContext<Guid>)executionContextBuilded;
            Task executionTask;

            if (fire)
            {
                if (NoneType.IsEqualTo<TInput>())
                    executionTask = executor.Fire(executionContext);
                else
                    executionTask = executor.Fire((TInput?)input, executionContext);
            }
            else if (NoneType.IsEqualTo<TInput>() && NoneType.IsEqualTo<TExpectedOutput>())
            {
                executionTask = executor.RunAsync(executionContext);
            }
            else if (NoneType.IsEqualTo<TInput>())
            {
                executionTask = executor.RunAsync<TExpectedOutput>(executionContext);
            }
            else
            {
                executionTask = executor.RunAsync<TExpectedOutput, TInput>((TInput?)input, executionContext);
            }

            return executionTask;
        }

        #endregion

        #endregion
    }
}
