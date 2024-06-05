// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;

    /// <summary>
    /// Builder used to customize the flow called
    /// </summary>
    public interface IExecutionConfigurationBuilder
    {
        /// <summary>
        /// Apply a grain redirection for specific stage (is no stage is provide the redirection will be available for all)
        /// </summary>
        /// <exception cref="ArgumentException">Raised if you try to add a redirection for the same key pair {stageUid - <typeparamref name="TOldGrain"/>}</exception>
        IExecutionConfigurationBuilder RedirectGrain<TOldGrain, TNewGrain>(params Guid[] stageUids)
            where TNewGrain : TOldGrain
            where TOldGrain : IVGrain;

        /// <summary>
        /// Define a signal to send at the end of the execution
        /// </summary>
        IExecutionConfigurationBuilder ResultSignal(in SignalId signalId, bool includeResult = false);

        /// <summary>
        /// Prevents the sequence executor state storage.
        /// </summary>
        IExecutionConfigurationBuilder PreventSequenceExecutorStateStorage();
    }
}
