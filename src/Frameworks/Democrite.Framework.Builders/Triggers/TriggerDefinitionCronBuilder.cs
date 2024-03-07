// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Implementations.Triggers
{
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Cron trigger builder
    /// </summary>
    internal sealed class TriggerDefinitionCronBuilder : TriggerDefinitionWithInputBaseBuilder, ITriggerDefinitionFinalizeBuilder
    {
        #region Fields

        private static readonly Regex s_allExceptSpacesReg = new Regex("[^\\s]+");
        private const string VALUE_GRP = "Values";

        private static readonly Regex s_values;
        private static readonly Regex s_months;
        private static readonly Regex s_daysOfWeek;

        private readonly string _cronExpression;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TriggerDefinitionCronBuilder"/> class.
        /// </summary>
        static TriggerDefinitionCronBuilder()
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
        /// Initializes a new instance of the <see cref="TriggerDefinitionBaseBuilder"/> class.
        /// </summary>
        public TriggerDefinitionCronBuilder(string cronExpression,
                                            string displayName,
                                            Guid? fixUid = null)
            : base(TriggerTypeEnum.Cron, displayName, fixUid)
        {
            this._cronExpression = cronExpression;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override TriggerDefinition Build()
        {
            int nbRefPart = 0;
            ValidateCronExpression(ref nbRefPart);

            return new CronTriggerDefinition(this.Uid,
                                             this.DisplayName,       
                                             this.Targets,
                                             true,
                                             this._cronExpression,
                                             nbRefPart == 6,
                                             base.TriggerGlobalOutputDefinition);
        }

        /// <summary>
        /// Validates the cron expression.
        /// </summary>
        /// <exception cref="InvalidDataException">Cron expression must have 5/6 parts separate with space : (second) minute hour day month dayOfWeak</exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ValidateCronExpression(ref int nbRefPart)
        {
            ArgumentNullException.ThrowIfNull(this._cronExpression);

            var segmentsReg = s_allExceptSpacesReg.Matches(this._cronExpression);

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
        private void ValidateCronExpressionPart(string label,
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
