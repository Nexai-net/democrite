// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Toolbox.Abstractions.Disposables;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing simple vgrain call
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorCallThreadStageProvider : SafeDisposable, ISequenceExecutorThreadStageProvider, ISequenceExecutorThreadStageHandler
    {
        #region Fields

        private readonly Dictionary<string, MethodInfo> _methodCallCache;
        private readonly ReaderWriterLockSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorCallThreadStageProvider"/> class.
        /// </summary>
        public SequenceExecutorCallThreadStageProvider()
        {
            this._methodCallCache = new Dictionary<string, MethodInfo>();
            this._locker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandler(ISequenceStageDefinition? stage)
        {
            return stage is SequenceStageCallDefinition;
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(ISequenceStageDefinition? stage)
        {
            return this;
        }

        /// <inheritdoc />
        public async ValueTask<StageStepResult> ExecAsync(ISequenceStageDefinition baseStep,
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

            if (step.ConfigurationType is not null)
            {
                var validators = mthd.GetCustomAttributes()
                                     .OfType<IExecutionContextConfigurationValidator>()
                                     .ToArray();

                if (validators.Length > 0)
                {
                    foreach (var validator in validators)
                        validator.Validate(step.ConfigurationInfo, mthd);
                }
            }

#pragma warning restore IDE0270 // Use coalesce expression

            var args = mthd.GetParameters()
                           .Select(p =>
                           {
                               var paramType = p.ParameterType;

                               if (paramType.IsAssignableTo(typeof(IExecutionContext)))
                               {
                                   if (step.ConfigurationType is null || paramType == typeof(IExecutionContext))
                                       return sequenceContext;
                           
                                   if (paramType == typeof(IExecutionContext<>).MakeGenericType(step.ConfigurationType.ToType()))
                                   {
                                       sequenceContext = sequenceContext.DuplicateWithContext(step.ConfigurationInfo, step.ConfigurationType.ToType());
                                       return sequenceContext;
                                   }
                               }
                           
                               if (input != null && (paramType == input.GetType() || input.GetType().IsAssignableTo(paramType)))
                                   return input;
                           
                               if (paramType.IsClass)
                                   return null;
                           
                               return Activator.CreateInstance(p.ParameterType);
                           
                           }).ToArray();

            var vgrainProxy = await vgrainProvider.GetVGrainAsync(step.VGrainType.ToType(), input, sequenceContext, logger);

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
            MethodInfo? mthd = null;

            using (this._locker.LockRead())
            {
                if (this._methodCallCache.TryGetValue(step.CallMethodDefinition.MethodUniqueId, out var cacheMthd))
                    mthd = cacheMthd;
            }

            if (mthd == null)
            {
                var type = step.VGrainType.ToType();

                mthd = type.GetAllMethodInfos(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                           .FirstOrDefault(m => step.CallMethodDefinition.IsEqualTo(m, true));

                if (mthd != null)
                {
                    if (mthd.IsGenericMethodDefinition && step.CallMethodDefinition.HasGenericArguments)
                    {
                        mthd = mthd.MakeGenericMethod(step.CallMethodDefinition
                                                          .GenericArguments
                                                          .Select(g => g.ToType())
                                                          .ToArray());
                    }

                    using (this._locker.LockWrite())
                    {
                        if (!this._methodCallCache.ContainsKey(step.CallMethodDefinition.MethodUniqueId))
                            this._methodCallCache.Add(step.CallMethodDefinition.MethodUniqueId, mthd);
                    }
                }
            }

            return mthd;
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            this._locker.Dispose();
            base.DisposeEnd();
        }

        #endregion

        #endregion
    }
}
