// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.Framework.Core.Helpers;
    using Democrite.Framework.Node.Abstractions.Models;
    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// Base diagnostic tracer
    /// </summary>
    /// <seealso cref="SafeDisposable" />
    internal abstract class GrainCallDiagnosticTracerService : DiagnosticBaseService<IDiagnosticLog>
    {
        #region Fields

        private static readonly Type s_exectionContextTrait = typeof(IExecutionContext);
        private readonly IOptions<ClusterNodeDiagnosticOptions> _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly NodeInfo _siloInfo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainCallDiagnosticTracerService"/> class.
        /// </summary>
        public GrainCallDiagnosticTracerService(ILocalSiloDetails localSiloDetails,
                                                IOptions<ClusterNodeDiagnosticOptions> options,
                                                IDiagnosticLogger diagnosticLogger,
                                                ILoggerFactory loggerFactory)
            : base(diagnosticLogger)
        {
            // Store option to support hot option reloaded
            this._options = options;
            this._loggerFactory = loggerFactory;

            this._siloInfo = new NodeInfo(localSiloDetails.ClusterId,
                                          localSiloDetails.DnsHostName,
                                          localSiloDetails.SiloAddress.ToParsableString());
        }

        #endregion

        /// <summary>
        ///     Log an invoke information
        /// </summary>
        protected async Task Invoke(IGrainCallContext context, OrientationEnum orientation)
        {
            IDiagnosticInOutLog? inLog = null;

            bool traceCall = false;
            bool traceInOut = false;

            Guid? flowUid = null;
            Guid? instanceExecId = null;
            Guid? parentExecId = null;
            Guid? callerId = null;

            IReadOnlyCollection<object?>? args = null;

            if (this.HasConsumer)
            {
                var vgrainType = context.InterfaceMethod.DeclaringType;

#if DEBUG
                ArgumentNullException.ThrowIfNull(vgrainType);
#endif
                var opt = this._options.Value;

                bool isOrleanSystem = VGrainMetaDataHelper.IsOrleanSystemVGrain(vgrainType);
                bool isDemocriteSystem = VGrainMetaDataHelper.IsDemocriteSystemVGrain(vgrainType);

                var passSystemFilter = (!isOrleanSystem || opt.TraceOrleanSystemVGrain) && (!isDemocriteSystem || opt.TraceDemocriteSystemVGrain);

                traceCall = !opt.DisableVGrainExecutionTracing && passSystemFilter;
                traceInOut = !opt.DisableVGrainInOutTracing && passSystemFilter;

                var argCount = context.Request.GetArgumentCount();
                args = Enumerable.Range(0, argCount)
                                 .Select(i => context.Request.GetArgument(i))
                                 .ToArray();

                if ((traceCall || traceInOut) && !isOrleanSystem)
                    TryExtractIds(context, out flowUid, out instanceExecId, out callerId, args);

                if (traceCall)
                {
                    var log = CreateVGrainCallLog(context,
                                                 orientation,
                                                 vgrainType,
                                                 isOrleanSystem,
                                                 isDemocriteSystem,
                                                 flowUid,
                                                 instanceExecId,
                                                 parentExecId,
                                                 callerId);
                    base.Send(log);
                }

                if (traceInOut)
                {
                    inLog = CreateVGrainInOutLog(OrientationEnum.In, context, flowUid, instanceExecId, callerId, args);
                    base.Send(inLog);
                }
            }

            try
            {

                await context.Invoke();
            }
            catch (Exception ex)
            {
                var argCount = context.Request.GetArgumentCount();
                var execCtx = Enumerable.Range(0, argCount)
                                        .Select(i => context.Request.GetArgument(i))
                                        .OfType<IExecutionContext>()
                                        .FirstOrDefault();

                execCtx?.GetLogger(this._loggerFactory, context.Request.GetInterfaceType())
                       ?.OptiLog(LogLevel.Error, "[Execution Call {grain}.{method}] Failed {exception} ", context.Grain, context.Request.GetMethod(), ex);

                throw;
            }

            if (this.HasConsumer && traceInOut)
            {
                var outLog = CreateVGrainInOutLog(OrientationEnum.Out, context, flowUid, instanceExecId, callerId, args);
                base.Send(outLog);
            }
        }

        /// <summary>
        /// Creates the <see cref="DiagnosticCallLog"/> from specific <see cref="IGrainCallContext"/>
        /// </summary>
        protected virtual IDiagnosticCallLog CreateVGrainCallLog(IGrainCallContext context,
                                                                 OrientationEnum orientation,
                                                                 Type vgrainType,
                                                                 bool isOrleanSystem,
                                                                 bool isDemocriteSystem,
                                                                 Guid? flowUid,
                                                                 Guid? instanceExecId,
                                                                 Guid? parentExecId,
                                                                 Guid? callerId)
        {
            return new DiagnosticCallLog(flowUid,
                                         instanceExecId,
                                         callerId,
                                         SerializeGrainId(context.SourceId),
                                         SerializeGrainId(context.TargetId),
                                         vgrainType.AssemblyQualifiedName,
                                         context.InterfaceMethod.GetAbstractMethod().DisplayName,
                                         //string.Format("{0} {1}({2})",
                                         //              context.InterfaceMethod.ReturnParameter,
                                         //              context.MethodName,
                                         //              string.Join(", ", context.InterfaceMethod.GetParameters().Select(p => p.ParameterType.Name))),
                                         orientation,
                                         DateTime.UtcNow,
                                         isOrleanSystem || context.TargetId.IsSystemTarget(),
                                         isDemocriteSystem,
                                         this._siloInfo);
        }

        /// <summary>
        /// Creates the <see cref="DiagnosticInOutLog"/> from specific <see cref="IGrainCallContext"/>
        /// </summary>
        protected virtual IDiagnosticInOutLog CreateVGrainInOutLog(OrientationEnum inOrOut,
                                                                  IGrainCallContext context,
                                                                  Guid? flowUid,
                                                                  Guid? instanceExecId,
                                                                  Guid? callerId,
                                                                  IReadOnlyCollection<object?>? args)
        {
            string? error = null;
            TypedArgument? typedArgument = null;

            if (inOrOut == OrientationEnum.In && args != null)
            {
                typedArgument = TypedArgument.From(args,
                                                   context.InterfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                                                   SkipExecutionContext);
            }
            else if (inOrOut == OrientationEnum.Out)
            {
                if (context.Response.Exception != null)
                {
                    // TODO : Flattern exception messages
                    error = context.Response.Exception.Message;
                }
                else
                {
                    typedArgument = TypedArgument.From(context.Response.Result,
                                                       context.Response?.GetSimpleResultType() ?? NoneType.Trait,
                                                       SkipExecutionContext);
                }
            }

            return new DiagnosticInOutLog(flowUid,
                                          instanceExecId,
                                          callerId,
                                          inOrOut,
                                          DateTime.UtcNow,
                                          typedArgument,
                                          error);
        }

        /// <summary>
        /// Skips the execution context argument.
        /// </summary>
        private bool SkipExecutionContext(object? current, Type type, int position)
        {
            return s_exectionContextTrait.IsAssignableFrom(type) == false;
        }

        /// <summary>
        /// Tries the extract flow id and current instance exec id; from <see cref="IExecutionContext"/> or VGrain Id
        /// </summary>
        private bool TryExtractIds(IGrainCallContext context,
                                   out Guid? flowUid,
                                   out Guid? currentExecId,
                                   out Guid? callerId,
                                   IReadOnlyCollection<object?> args)
        {
            flowUid = null;
            currentExecId = null;
            callerId = null;

            var execContext = args.OfType<IExecutionContext>().FirstOrDefault();

            if (execContext == null)
            {

            }

            if (execContext != null)
            {
                flowUid = execContext.FlowUID;
                currentExecId = execContext.CurrentExecutionId;
                callerId = execContext.ParentExecutionId;
            }

            //if ((flowUid == Guid.Empty || flowUid == null) && context.TargetId.TryGetGuidKey(out var instanceId, out var possibleContextFlowId))
            //{
            //    var sequenceId = SequenceIdContext.From(context.TargetId);

            //    if (sequenceId.InstanceUniqueId != Guid.Empty)
            //    {
            //        flowUid = sequenceId.FlowUid;
            //        currentExecId = sequenceId.InstanceUniqueId;
            //    }
            //    else
            //    {
            //        currentExecId = instanceId;

            //        if (!string.IsNullOrEmpty(possibleContextFlowId) && Guid.TryParse(possibleContextFlowId, out var flID))
            //            flowUid = flID;
            //    }
            //}

            return flowUid == Guid.Empty || flowUid == null;
        }

        /// <summary>
        /// Serializes the grain identifier.
        /// </summary>
        protected string? SerializeGrainId(GrainId? id)
        {
            return id?.ToString() ?? string.Empty;
        }
    }
}
