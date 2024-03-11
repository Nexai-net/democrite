// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Elvex.Toolbox.Abstractions.Disposables;

    /// <summary>
    /// Internal thread executor
    /// </summary>
    internal interface ISequenceExecutorExecThread
    {
        #region Properties

        /// <summary>
        /// Gets the current execution context.
        /// </summary>
        IExecutionContext ExecutionContext { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the security thread handler.
        /// </summary>
        ISecureContextToken<ISequenceExecutorThreadHandler> GetSecurityThreadHandler();

        #endregion
    }
}
