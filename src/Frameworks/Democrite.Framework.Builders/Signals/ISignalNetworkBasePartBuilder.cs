// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    /// <summary>
    /// Base builder shared by signal and signal
    /// </summary>
    /// <typeparam name="TWizard">The type of the wizard.</typeparam>
    public interface ISignalNetworkBasePartBuilder<TWizard>
        where TWizard : ISignalNetworkBasePartBuilder<TWizard>
    {
    }
}
