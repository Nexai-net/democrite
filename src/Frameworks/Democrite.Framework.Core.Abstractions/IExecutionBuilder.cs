// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    /// <summary>
    /// Builder used to setup execution
    /// </summary>
    public interface IExecutionBuilder<TInput>
    {
        /// <summary>
        /// Sets the execution input.
        /// </summary>
        /// <remarks>
        ///     Last set remain
        /// </remarks>
        IExecutionLauncher SetInput(TInput? input);
    }

    /// <summary>
    /// Builder used to setup execution
    /// </summary>
    public interface IExecutionBuilder
    {
        /// <summary>
        /// Sets the execution input.
        /// </summary>
        /// <remarks>
        ///     Last set remain
        /// </remarks>
        IExecutionLauncher SetInput(object? input);
    }
}
