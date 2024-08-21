// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Models;

    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    /// <summary>
    /// Define all execution context information 
    /// </summary>
    public interface IExecutionContext
    {
        #region Properties

        /// <summary>
        /// Gets a flow unique id diffuse through all the ewecution stream and graph
        /// </summary>
        Guid FlowUID { get; }

        /// <summary>
        /// Gets the parent execution identifier.
        /// </summary>
        Guid? ParentExecutionId { get; }

        /// <summary>
        /// Gets the current execution identifier.
        /// </summary>
        Guid CurrentExecutionId { get; }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        CancellationToken CancellationToken { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Force operation to cancel
        /// </summary>
        ValueTask Cancel();

        /// <summary>
        /// Gets the logger associate to current sequence execution and vgrain
        /// </summary>
        ILogger GetLogger<T>(ILoggerFactory loggerProvider) where T : IVGrain;

        /// <summary>
        /// Gets the logger associate to current sequence execution and vgrain
        /// </summary>
        ILogger GetLogger(ILoggerFactory loggerProvider, Type type);

        /// <summary>
        /// Gets linked context
        /// </summary>
        IExecutionContext NextContext();

        /// <summary>
        /// Duplicates current <see cref="IExecutionContext"/> and attach the context info <paramref name="contextInfo"/>
        /// </summary>
        IExecutionContext<TContextInfo> DuplicateWithConfiguration<TContextInfo>(TContextInfo contextInfo);

        /// <summary>
        /// Duplicates current <see cref="IExecutionContext"/> and attach the context info <paramref name="contextInfo"/>
        /// </summary>
        IExecutionContext DuplicateWithConfiguration(object? contextInfo, Type contextType);

        /// <summary>
        /// Gets all data context.
        /// </summary>
        /// <remarks>
        ///     Use during serialization
        /// </remarks>
        IReadOnlyCollection<IContextDataContainer> GetAllDataContext();

        /// <summary>
        /// Try to the pusll data stored in the context carried throught flow execution
        /// </summary>
        TContextData? TryGetContextData<TContextData>(IDemocriteSerializer serializer) where TContextData : struct;

        /// <summary>
        /// Try to the pusll data stored in the context carried throught flow execution
        /// </summary>
        object? TryGetContextData(ConcretType type, IDemocriteSerializer serializer);

        /// <summary>
        /// Try to the push data stored in the context carried throught flow execution
        /// </summary>
        /// <remarks>
        ///     A data is identify by its type, except when <paramref name="override"/> is true the data will not be replaced
        /// </remarks>
        bool TryPushContextData<TContextData>(TContextData contextData,
                                              bool @override,
                                              IDemocriteSerializer serializer) where TContextData : struct;

        /// <summary>
        /// Try to the push data stored in the context carried throught flow execution
        /// </summary>
        /// <remarks>
        ///     A data is identify by its type, except when <paramref name="override"/> is true the data will not be replaced
        /// </remarks>
        bool TryPushContextData(IContextDataContainer contextData, bool @override);

        /// <summary>
        /// Clear all context data
        /// </summary>
        void ClearContextData();

        /// <summary>
        /// Clear all context data of type <paramref name="dataType"/>
        /// </summary>
        void ClearContextData(Type dataType);

        /// <summary>
        /// Clear all context data of type <paramref name="dataType"/>
        /// </summary>
        void ClearContextData<TContextData>() where TContextData : struct;

        /// <summary>
        /// Duplicates this instance.
        /// </summary>
        IExecutionContext Duplicate();

        #endregion
    }

    /// <summary>
    /// Define all the execution context method managed only by the democrit engine
    /// </summary>
    internal interface IExecutionContextInternal
    {
        #region Properties

        /// <summary>
        /// Gets the grain cancellation token.
        /// </summary>
        GrainCancellationToken? GrainCancellationToken { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies the context important information.
        /// </summary>
        void CopyContextImportantInformation(IExecutionContext source);

        ///// <summary>
        ///// Set all data context.
        ///// </summary>
        ///// <remarks>
        /////     Use during serialization
        ///// </remarks>
        //void InjectAllDataContext(IReadOnlyCollection<IContextDataContainer> contextDataContainers);

        /// <summary>
        /// <see cref="GrainCancellationToken"> Cancel system of orlean need to be initialized from from specific
        /// internal service.
        /// </summary>
        void InitGrainCancelToken(GrainReference grainReference);

        /// <summary>
        /// Link vgrain invoked to global cancel system
        /// </summary>
        /// <remarks>
        ///
        ///  By default orlean manage automatically the cancellation token track using
        ///  GrainCancellationToken in method parameters
        ///  
        ///  In the goal to minimize the impact and knowlegde of orlean in the user code
        ///  We decide to used the IExecutionContext to carry the CancellationToken
        ///  
        ///  but to use orlean cancel hierarchy track we have to manually reproduce the
        ///  behavior contain in GrainReferenceRuntime.SetGrainCancellationTokensTarget(GrainReference target, IInvokable request)
        ///  that will connect the target grain <see cref="GrainCancellationToken"/> to current one.
        ///  
        ///  this way when the current grain is cancelled the chain (children only) will be informed and cancelled also
        ///  
        ///  grainToken.AddGrainReference(cancellationTokenRuntime, target);
        /// 
        /// </remarks>
        void AddCancelGrainReference(GrainReference grainReference);

        /// <summary>
        /// Forces the grain cancellation token.
        /// </summary>
        /// <remarks>
        ///     Could different after registrationt to Cancellation system
        /// </remarks>
        void ForceGrainCancellationToken(GrainCancellationToken registredCancellationToken);

        #endregion
    }

    /// <summary>
    /// Define all execution context information with custom TConfiguration
    /// </summary>
    public interface IExecutionContext<TConfiguration> : IExecutionContext
    {
        #region Properties

        /// <summary>
        /// Gets the context information configured at the sequence setups.
        /// </summary>
        /// </value>
        TConfiguration? Configuration { get; }

        #endregion
    }
}
