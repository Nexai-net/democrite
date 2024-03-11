// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.DebugTools.Grains
{
    using Democrite.Framework.Bag.DebugTools.Models;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Elvex.Toolbox;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Grain used to display on the logger input and context info
    /// </summary>
    internal abstract class DisplayAllInfoBaseVGrain<TGrain> : VGrainBase<TGrain>
        where TGrain : IVGrain
    {
        #region Fields

        // https://gist.github.com/dominikwilkowski/60eed2ea722183769d586c76f22098dd
        private const string END_COLOR_TAG = "\u001b[0m";

        private readonly IOptionsMonitor<DebugDisplayInfoOptions> _options;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAllInfoBaseVGrain"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public DisplayAllInfoBaseVGrain(ILogger<TGrain> logger,
                                        IOptionsMonitor<DebugDisplayInfoOptions>? options = null)
            : base(logger)
        {
            this._options = options ?? DebugDisplayInfoOptions.Default.ToMonitorOption();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays one info on the logger
        /// </summary>
        protected ValueTask DisplayInfoAsync<TInfo>(TInfo? info, string? infoHeader)
        {
            return DisplayInfoAsync(info, infoHeader, NoneType.Instance, string.Empty);
        }

        /// <summary>
        /// Displays two info on the logger
        /// </summary>
        protected ValueTask DisplayInfoAsync<TInfo, TInfo2>(TInfo? info, string? infoHeader, TInfo2? info2, string? info2Header)
        {
            var option = this._options.CurrentValue;

            var logLevel = option.LogLevel;

            if (logLevel == LogLevel.None)
                logLevel = LogLevel.Debug;

            if (this.Logger.IsEnabled(logLevel) == false)
                return ValueTask.CompletedTask;

            var strBuilder = new StringBuilder(2048);

            var jsonMustBeformatted = option.JsonFormated == null
                                                ? Debugger.IsAttached
                                                : option.JsonFormated.Value;

            var formatting = jsonMustBeformatted ? Formatting.Indented : Formatting.None;

            var displayInfo = info is not NoneType && (info is not null || !string.IsNullOrEmpty(infoHeader));
            var displayInfo2 = info2 is not NoneType && (info2 is not null || !string.IsNullOrEmpty(info2Header));

            if (displayInfo)
            {
                WriteInformation(info, infoHeader, option, strBuilder, formatting);

                if (displayInfo2)
                    strBuilder.AppendLine().AppendLine();
            }

            if (displayInfo2)
                WriteInformation(info2, info2Header, option, strBuilder, formatting);

            this.Logger.Log(logLevel, strBuilder.ToString());

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writes the information in string format based on options
        /// </summary>
        private static void WriteInformation<TInfo>(TInfo? info, string? infoHeader, DebugDisplayInfoOptions option, StringBuilder strBuilder, Formatting formatting)
        {
            if (option.UseHeaderColor)
            {
                if (option.HeaderBackgroundColor != null)
                    strBuilder.Append(option.HeaderBackgroundColor);

                if (option.HeaderForegroundColor != null)
                    strBuilder.Append(option.HeaderForegroundColor);
            }

            strBuilder.Append(" ");

            if (!string.IsNullOrEmpty(option.HeaderPrefix))
                strBuilder.Append(option.HeaderPrefix);

            strBuilder.Append(string.IsNullOrEmpty(infoHeader) ? typeof(TInfo).Name : infoHeader);

            strBuilder.Append(" ");

            if (option.UseHeaderColor && (option.HeaderForegroundColor != null || option.HeaderBackgroundColor != null))
                strBuilder.Append(DisplayAllInfoBaseVGrain<TGrain>.END_COLOR_TAG);

            strBuilder.AppendLine();

            var infoJson = JsonConvert.SerializeObject(info, formatting);
            strBuilder.AppendLine(infoJson);
        }

        #endregion
    }
}
