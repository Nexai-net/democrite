// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Disposables;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Orleans.Runtime;

    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing simple vgrain call
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorThreadStageCall : SafeDisposable, ISequenceExecutorThreadStageHandler
    {
        #region Fields

        private readonly IDemocriteSerializer _democriteSerializer;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStageCall"/> class.
        /// </summary>
        public SequenceExecutorThreadStageCall(IDemocriteSerializer democriteSerializer)
        {
            this._democriteSerializer = democriteSerializer;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<StageStepResult> ExecAsync(SequenceStageDefinition baseStep,
                                                          object? input,
                                                          IExecutionContext sequenceContext,
                                                          ILogger logger,
                                                          IDiagnosticLogger diagnosticLogger,
                                                          IVGrainProvider vgrainProvider,
                                                          Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            var step = (SequenceStageCallDefinition)baseStep;
            var mthd = GetVGrainMethodToCall(step);

#pragma warning disable IDE0270 // Use coalesce expression
            if (mthd == null)
            {
                throw VGrainMethodDemocriteException.MethodNotFounded(step.VGrainType,
                                                                      step.Output,
                                                                      step.CallMethodDefinition.DisplayName,
                                                                      step.CallMethodDefinition.Arguments);
            }

            object? configuration = null;

            if (step.Configuration is not null)
            {
                var cfgInput = input;

                if (step.ConfigurationFromContextDataType is not null)
                {
                    cfgInput = sequenceContext.TryGetContextData(step.ConfigurationFromContextDataType, this._democriteSerializer);
                    if (cfgInput is null)
                    {
                        logger.OptiLog(LogLevel.Warning, "ConfigurationFromContextDataType is null or missing : {ConfigurationFromContextDataType}", step.ConfigurationFromContextDataType);
                        cfgInput = input;
                    }
                }

                configuration = step.Configuration.Resolve(cfgInput);

                var validators = mthd.GetCustomAttributes()
                                     .OfType<IExecutionContextConfigurationValidator>()
                                     .ToArray();

                if (validators.Length > 0)
                {
                    foreach (var validator in validators)
                        validator.Validate(configuration, mthd);
                }
            }

            var originInput = input;

#pragma warning restore IDE0270 // Use coalesce expression

            var args = mthd.GetParameters()
                           .Select(p =>
                           {
                               var paramType = p.ParameterType;

                               if (paramType.IsAssignableTo(typeof(IExecutionContext)))
                               {
                                   if (step.Configuration?.TargetType is null || paramType == typeof(IExecutionContext))
                                       return sequenceContext;

                                   if (paramType == typeof(IExecutionContext<>).MakeGenericType(step.Configuration.TargetType.ToType()))
                                   {
                                       sequenceContext = sequenceContext.DuplicateWithConfiguration(configuration, step.Configuration.TargetType.ToType());
                                       return sequenceContext;
                                   }
                               }

                               // TODO : Work because only one input parameter, if multiple it will failed
                               if (step.IndexedParameterDefinitions is not null && step.IndexedParameterDefinitions.TryGetValue(p.Position, out var paramAccessDef))
                               {
                                   input = paramAccessDef?.Access?.Resolve(originInput) ?? originInput;
                               }

                               if (input != null && (paramType == input.GetType() || input.GetType().IsAssignableTo(paramType)))
                                   return input;

                               if (paramType.IsClass)
                                   return null;

                               return Activator.CreateInstance(p.ParameterType);

                           }).ToArray();

            var vgrainProxy = await vgrainProvider.GetVGrainAsync(step.VGrainType.ToType(), input, sequenceContext, logger);

            if (vgrainProxy is not null && sequenceContext is IExecutionContextInternal executionContextInternal)
            {
                var grainRef = vgrainProxy.AsReference<GrainReference>();
                executionContextInternal.AddCancelGrainReference(grainRef);
            }

            // OPTIM : Use generator to create proxy call without doing any reflexion
            var resultTask = (Task?)mthd.Invoke(vgrainProxy, args);

            ArgumentNullException.ThrowIfNull(resultTask);

            return new StageStepResult(step.Output?.ToType(), resultTask);
        }

        #region Tools

        /// <summary>
        /// Gets the vgrain method to call from analyze or cache
        /// </summary>
        private MethodInfo? GetVGrainMethodToCall(SequenceStageCallDefinition step)
        {
            return step.CallMethodDefinition.ToMethod(step.VGrainType.ToType()) as MethodInfo;
        }

        #endregion

        #endregion
    }
}
