// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;

    using Orleans.Runtime;

    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Incomming filter used to attach the <see cref="GrainCancellationToken"/> to called vgrain using <see cref="IExecutionContext"/>
    /// </summary>
    /// <seealso cref="IIncomingGrainCallFilter" />
    /// <remarks>
    ///
    ///  Comment copy from ExecutionContext.AddCancelGrainReference method
    ///  
    ///  By default orlean manage automatically the cancellation token track using
    ///  GrainCancellationToken in method parameters
    ///  
    ///  In the goal to minimize the impact and knowlegde of orlean in the user code
    ///  We decide to used the IExecutionContext to carry the CancellationToken
    ///  
    ///  but to use orlean cancel hierarchy track we have to manually reproduce the
    ///  behavior contain in GrainReferenceRuntime.SetGrainCancellationTokensTarget(GrainReference target, IInvokable request)
    ///  
    ///  grainToken.AddGrainReference(cancellationTokenRuntime, target);
    /// 
    /// </remarks>
    internal sealed class GrainPopulateCancellationTokenCallFilter : IIncomingGrainCallFilter
    {
        #region Fields

        private static readonly MethodInfo s_getCancelSourceExtension;
        private static readonly object s_recordCallationTokenLocker;
        private static readonly PropertyInfo s_getTokenId;
        
        private static MethodInfo? s_recordCancellationToken;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="GrainPopulateCancellationTokenCallFilter"/> class.
        /// </summary>
        static GrainPopulateCancellationTokenCallFilter()
        {
            s_recordCallationTokenLocker = new object();

            var orleanRuntimeAssembly = typeof(GrainReference).Assembly;

            var indexedTypes = orleanRuntimeAssembly.GetTypes()
                                                    .Where(t => t.Name is not null && t.Name.Contains("Extension", StringComparison.OrdinalIgnoreCase))
                                                    .GroupBy(t => t.Name)
                                                    .ToDictionary(k => k.Key, t => t.First(), StringComparer.OrdinalIgnoreCase);

            var interfaceCancelSourceExtensionType = indexedTypes["ICancellationSourcesExtension"];
            //var cancelSourceExtensionType = indexedTypes["CancellationSourcesExtension"];

            Expression<Func<IGrainContext, IGrainExtension>> grainExtensionExpr = c => GrainContextComponentExtensions.GetGrainExtension<IGrainExtension>(c);
            s_getCancelSourceExtension = ((MethodCallExpression)grainExtensionExpr.Body).Method.GetGenericMethodDefinition().MakeGenericMethod(interfaceCancelSourceExtensionType!);

            s_getTokenId = typeof(GrainCancellationToken).GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic)!;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task Invoke(IIncomingGrainCallContext context)
        {
            var request = context.Request;
            int argumentCount = request.GetArgumentCount();
            for (int i = 0; i < argumentCount; i++)
            {
                if (request.GetArgument(i) is IExecutionContextInternal execContext)
                {
                    // Ensure the Cancellation token is well created and init
                    execContext.InitGrainCancelToken(context.TargetContext.GrainReference);

                    var token = execContext.GrainCancellationToken;

                    // Register the token to target grain dedicated extension ICancellationSourcesExtension

                    // REF : Orleans.Runtime.InsideRuntimeClient.Invoke -> calling CancellationSourcesExtension.RegisterCancellationTokens(target, invokable);
                    //(CancellationSourcesExtension)target.GetGrainExtension<ICancellationSourcesExtension>();

                    var cancellationExtension = s_getCancelSourceExtension.Invoke(null, new object[] { context.TargetContext }); 

                    if (cancellationExtension is null)
                        break;

                    if (s_recordCancellationToken is null)
                    {
                        lock (s_recordCallationTokenLocker)
                        {
                            if (s_recordCancellationToken is null)
                                s_recordCancellationToken = cancellationExtension.GetType()!.GetMethod("RecordCancellationToken", BindingFlags.Instance | BindingFlags.NonPublic)!;
                        }
                    }

                    var tokenId = (Guid)s_getTokenId.GetValue(token!)!;

                    //(object)cancellationExtension.RecordCancellationToken(grainToken.Id, grainToken.IsCancellationRequested);
                    var registredCancellationToken = (GrainCancellationToken)s_recordCancellationToken.Invoke(cancellationExtension, new object[] { tokenId, false })!; 

                    execContext.ForceGrainCancellationToken(registredCancellationToken);

                    break;
                }
            }

            return context.Invoke();
        }

        #endregion
    }
}
