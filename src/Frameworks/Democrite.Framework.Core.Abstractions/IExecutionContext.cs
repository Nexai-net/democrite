// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Microsoft.Extensions.Logging;

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
        void Cancel();

        /// <summary>
        /// Gets the logger associate to current sequence execution and vgrain
        /// </summary>
        ILogger GetLogger<T>(ILoggerProvider loggerProvider) where T : IVGrain;

        /// <summary>
        /// Gets linked context
        /// </summary>
        IExecutionContext NextContext();

        /// <summary>
        /// Duplicates current <see cref="IExecutionContext"/> and attach the context info <paramref name="contextInfo"/>
        /// </summary>
        IExecutionContext<TContextInfo> DuplicateWithContext<TContextInfo>(TContextInfo contextInfo);

        /// <summary>
        /// Duplicates current <see cref="IExecutionContext"/> and attach the context info <paramref name="contextInfo"/>
        /// </summary>
        IExecutionContext DuplicateWithContext(object? contextInfo, Type contextType);

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
