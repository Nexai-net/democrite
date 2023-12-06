// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;

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
        #region Methods

        /// <inheritdoc />
        public Task Invoke(IIncomingGrainCallContext context)
        {
            var request = context.Request;
            int argumentCount = request.GetArgumentCount();
            for (int i = 0; i < argumentCount; i++)
            {
                if (request.GetArgument(i) is Democrite.Framework.Core.Models.ExecutionContext execContext)
                {
                    execContext.AddCancelGrainReference(context.TargetContext);
                    break;
                }
            }

            return context.Invoke();
        }

        #endregion
    }
}
