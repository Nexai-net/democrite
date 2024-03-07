// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.DebugTools.Models
{
    using Democrite.Framework.Node.Abstractions.Models;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Define all the option about display in logger
    /// </summary>
    public sealed class DebugDisplayInfoOptions : INodeOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DebugDisplayInfoOptions"/> class.
        /// </summary>
        static DebugDisplayInfoOptions()
        {
            Default = new DebugDisplayInfoOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDisplayInfoOptions"/> class.
        /// </summary>
        public DebugDisplayInfoOptions()
            : this(LogLevel.Debug)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDisplayInfoOptions"/> class.
        /// </summary>
        public DebugDisplayInfoOptions(LogLevel logLevel = LogLevel.Debug,
                                       bool? jsonFormated = null,
                                       string? headerPrefix = null,
                                       bool useHeaderColor = true,
                                       string? headerBackgroundColor = "\u001b[47m",
                                       string? headerForegroundColor = "\u001b[30m")
        {
            this.LogLevel = logLevel;
            this.JsonFormated = jsonFormated;
            this.HeaderPrefix = headerPrefix;
            this.UseHeaderColor = useHeaderColor;
            this.HeaderBackgroundColor = headerBackgroundColor;
            this.HeaderForegroundColor = headerForegroundColor;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static DebugDisplayInfoOptions Default { get; }

        /// <summary>
        /// Gets the log level used to write the display; Default : <see cref="LogLevel.Debug"/>.
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// Gets a value indicating if the json display must be formated or not; Default null.<br/>
        /// <c>Null</c> let the framework decide, if debugger is attached then format otherwise no.
        /// </summary>
        public bool? JsonFormated { get; }

        /// <summary>
        /// Gets the header prefix to add (usefull to tag the information)
        /// </summary>
        public string? HeaderPrefix { get; }

        /// <summary>
        /// Gets a value indicating whether the header must be color or not (use the microsoft console color code); default <c>True</c>
        /// </summary>
        public bool UseHeaderColor { get; }

        /// <summary>
        /// Gets the color of the header background. Default White
        /// </summary>
        /// <remarks>
        ///     Color Code : https://gist.github.com/dominikwilkowski/60eed2ea722183769d586c76f22098dd
        /// </remarks>
        public string? HeaderBackgroundColor { get; }

        /// <summary>
        /// Gets the color of the header foreground. Default Black
        /// </summary>
        /// <remarks>
        ///     Color Code : https://gist.github.com/dominikwilkowski/60eed2ea722183769d586c76f22098dd
        /// </remarks>
        public string? HeaderForegroundColor { get; }

        #endregion
    }
}
