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
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Launcher used to call a vgrain method and provide the result
    /// </summary>
    internal sealed class ExecutionDirectLauncher<TVGrain, TInput, TConfig, TResult> : ExecutionBaseLauncher<TVGrain, TResult, IExecutionDirectLauncher>
        where TVGrain : IVGrain
    {
        #region Fields

        private readonly LambdaExpression _expression;
        private readonly IdSpan? _forcedGrainId;
        private readonly TConfig? _config;
        private readonly TInput? _input;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionDirectLauncher{TVGrain, TInput, TConfig, TResult}"/> struct.
        /// </summary>
        public ExecutionDirectLauncher(ILogger<IDemocriteExecutionHandler> logger,
                                       IVGrainProvider vgrainProvider,
                                       TConfig? config,
                                       TInput? input,
                                       Expression expression,
                                       IdSpan? forcedGrainId)
            : base(logger, vgrainProvider)
        {
            ArgumentNullException.ThrowIfNull(expression);

            if (expression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Only expression type lambda are accepted.");

            this._forcedGrainId = forcedGrainId;
            this._expression = (LambdaExpression)expression;
            this._config = config;
            this._input = input;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override IExecutionContext GenerateExecutionContext(Guid? flowId, Guid? parentExecutionId)
        {
            flowId ??= Guid.NewGuid();

            if (NoneType.IsEqualTo<TConfig>())
                return new ExecutionContext(flowId.Value, Guid.NewGuid(), parentExecutionId);

            return new ExecutionContextWithConfiguration<TConfig>(flowId.Value, Guid.NewGuid(), parentExecutionId, this._config);
        }

        /// <inheritdoc />
        protected override object? GetInput()
        {
            return this._input;
        }

        /// <inheritdoc />
        protected override string? OnFireFailedBuildContext(Exception _)
        {
            var errorContext = new StringBuilder();

            errorContext.Append(this._expression);

            if (NoneType.IsEqualTo<TConfig>() == false)
            {
                errorContext.Append("(cfg : ");

                if (this._config is ISupportDebugDisplayName supportDebugDisplayConfig)
                    errorContext.Append(supportDebugDisplayConfig.ToDebugDisplayName());
                else
                    errorContext.Append(this._config);

                errorContext.Append(")");
            }

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
        protected override Task OnRunAsync<TExpectedOutput>(TVGrain executor, object? input, IExecutionContext executionContext, bool fire)
        {
            // Build call parameters
            var parameters = this._expression.Parameters
                                             .Select(p =>
                                             {
                                                 if (p.Type == typeof(TVGrain))
                                                     return executor;

                                                 if (p.Type == typeof(TInput))
                                                     return input;

                                                 if (p.Type.IsAssignableTo(typeof(IExecutionContext)))
                                                     return executionContext;

                                                 throw new NotSupportedException("Only parameter tolerate are vgrain, input and execution context");
                                             })
                                             .ToArray();

            // Compile expression into executable lambda
            var func = this._expression.Compile();

            var result = func.DynamicInvoke(parameters);

            if (result is null || result.GetType().IsAssignableTo(typeof(Task)) == false)
                throw new InvalidOperationException("The action called MUST return a Task");

            return (Task)result;
        }

        #endregion
    }
}
