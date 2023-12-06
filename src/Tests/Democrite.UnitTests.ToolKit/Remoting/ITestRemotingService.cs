// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Remoting
{
    /// <summary>
    /// Define remote controller
    /// </summary>
    public interface ITestRemotingService : IDisposable
    {
        /// <summary>
        /// Starts remoting services
        /// </summary>
        ValueTask StartRemotingListnerAsync();
    }
}
