// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Loggers
{
    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    ///  Logger that keep in memory the logs
    /// </summary>
    public interface IInMemoryLogger : ILogger, IObservable<string>
    {
        #region Methods

        /// <summary>
        /// Gets a copy of logs at requested time.
        /// </summary>
        IReadOnlyCollection<string> GetLogsCopy();

        #endregion
    }
}
