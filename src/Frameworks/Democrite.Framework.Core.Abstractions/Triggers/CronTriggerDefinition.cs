// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Trigger definition used to setup a timer - cron
    /// </summary>
    /// <seealso cref="TriggerBaseDefinition" />
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class CronTriggerDefinition : TriggerDefinition
    {
        #region Fields

        private static readonly Regex s_allExceptSpacesReg = new Regex("[^\\s]+");

        private const string VALUE_GRP = "Values";

        private static readonly Regex s_values;
        private static readonly Regex s_months;
        private static readonly Regex s_daysOfWeek;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="CronTriggerDefinition"/> class.
        /// </summary>
        static CronTriggerDefinition()
        {
            const string PLACEHOLDER = "__VALUES__";

            var classicENCulture = new CultureInfo("en-US");

            var cronPartTemplate = "^(?<value>[*]{1}|__VALUES__[-]{1}__VALUES__|(__VALUES__[,]{0,1})*(?<option>/[0-9]*)?)";

            var months = Enumerable.Range(1, 12)
                                   .Select(indx => classicENCulture.DateTimeFormat.GetAbbreviatedMonthName(indx))
                                   .Distinct()
                                   .ToArray();

            var dayOfWeek = Enumerable.Range(0, 7)
                                      .Select(indx => classicENCulture.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)indx))
                                      .Distinct()
                                      .ToArray();

            s_values = new Regex(cronPartTemplate.Replace(PLACEHOLDER, "(?<" + VALUE_GRP + ">[0-9]{1,2})"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_months = new Regex(cronPartTemplate.Replace(PLACEHOLDER, "[" + string.Join("|", months) + "]"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_daysOfWeek = new Regex(cronPartTemplate.Replace(PLACEHOLDER, "[" + string.Join("|", dayOfWeek) + "]"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CronTriggerDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public CronTriggerDefinition(Guid uid,
                                     Uri refId,
                                     string displayName,
                                     IEnumerable<TriggerTargetDefinition> targets,
                                     bool enabled,
                                     string cronExpression,
                                     bool useSecond,
                                     DefinitionMetaData? metaData,
                                     DataSourceDefinition? triggerGlobalOutputDefinition = null)
            : base(uid,
                   refId,
                   displayName,
                   TriggerTypeEnum.Cron,
                   targets,
                   enabled,
                   metaData,
                   triggerGlobalOutputDefinition)
        {
            this.CronExpression = cronExpression;
            this.UseSecond = useSecond;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public string CronExpression { get; }

        /// <inheritdoc />
        [Id(1)]
        [DataMember]
        public bool UseSecond { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return "[CronExpression: " + this.CronExpression + "] [UseSecond: " + this.UseSecond + "]";
        }

        /// <inheritdoc />
        protected override bool OnValidate(ILogger logger, bool matchWarningAsError = false)
        {
            var valid = true;

            if (string.IsNullOrEmpty(this.CronExpression))
            {
                logger.OptiLog(LogLevel.Error, "Cron Expression must not be empty");
                valid = false;
            }

            try
            {
                int nbRefPart = 0;
                ValidateCronExpression(ref nbRefPart, this.CronExpression);

                if (nbRefPart == 6 && this.UseSecond == false)
                {
                    valid = false;
                    logger.OptiLog(LogLevel.Error, "Cron format use second but option is not set");
                }
            }
            catch (Exception ex)
            {
                valid = false;
                logger.OptiLog(LogLevel.Error, "Cron validation {exception}", ex);
            }

            return valid;
        }

        /// <summary>
        /// Validates the cron expression.
        /// </summary>
        /// <exception cref="InvalidDataException">Cron expression must have 5/6 parts separate with space : (second) minute hour day month dayOfWeak</exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ValidateCronExpression(ref int nbRefPart, string cronExpression)
        {
            ArgumentNullException.ThrowIfNull(cronExpression);

            var segmentsReg = s_allExceptSpacesReg.Matches(cronExpression);

            var segments = segmentsReg.Where(s => s.Success && !string.IsNullOrEmpty(s.Value))
                                      .Select(s => s.Value)
                                      .ToArray();

            nbRefPart = segments.Length;

            if (segments.Length != 5 && segments.Length != 6)
                throw new InvalidDataException("Cron expression must have 5/6 parts separate with space : (second) minute hour day month dayOfWeak");

            int startIndx = 0;

            if (segments.Length == 6)
            {
                ValidateCronExpressionPart("second", segments[0], 0, 60);
                startIndx++;
            }

            ValidateCronExpressionPart("minutes", segments[startIndx + 0], 0, 60);
            ValidateCronExpressionPart("hours", segments[startIndx + 1], 0, 60);
            ValidateCronExpressionPart("days", segments[startIndx + 2], 0, 24);

            ValidateCronExpressionPart("months", segments[startIndx + 3], 0, 12, s_months);
            ValidateCronExpressionPart("dayofweak", segments[startIndx + 4], 0, 6, s_daysOfWeek);

        }

        /// <summary>
        /// Validates the cron expression part.
        /// </summary>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        private static void ValidateCronExpressionPart(string label,
                                                       string expression,
                                                       int min,
                                                       int excludedMax,
                                                       params Regex[] alternateRegex)
        {
            var match = s_values.Match(expression);

            bool useAlternateRegex = false;

            if (match.Success == false && alternateRegex.Any())
            {
                match = alternateRegex.Select(m => m.Match(expression))
                                      .FirstOrDefault(m => m.Success);

                useAlternateRegex = true;
            }

            if ((match?.Success ?? false) == false)
                throw new InvalidDataException(string.Format("[CronFormat] {0} - doesn't follow the pattern '*', 'value', list split by ',' or range define by '-'.", label));

            if (!useAlternateRegex)
            {
                var allValues = match.Groups[VALUE_GRP];

                foreach (Capture value in allValues.Captures)
                {
                    var captureValue = int.Parse(value.Value);

                    if (captureValue < min || captureValue >= excludedMax)
                        throw new ArgumentOutOfRangeException(label, string.Format("[CronFormat] {0} - Value {1} must be in the range of [{2} - {3}[", label, captureValue, min, excludedMax));
                }
            }
        }

        #endregion
    }
}
