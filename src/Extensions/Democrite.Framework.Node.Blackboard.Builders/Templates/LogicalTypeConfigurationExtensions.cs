// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using System.Numerics;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Configuration dedicated to one type on record on the blackboard extensions
    /// </summary>
    public static class LogicalTypeConfigurationExtensions
    {
        /// <summary>
        /// Specify the tolerate range [<paramref name="minIncluded"/> -> <paramref name="maxExclude"/>[
        /// </summary>
        public static ILogicalTypeCheckConfiguration<TNumber> Range<TNumber>(this ILogicalTypeCheckConfiguration<TNumber> cfg,
                                                                        TNumber minIncluded,
                                                                        TNumber maxExclude)
            where TNumber : struct, INumber<TNumber>
        {
            if (cfg.Rules.Any(r => r is BlackboardNumberRangeLogicalTypeRule<TNumber>))
                throw new InvalidOperationException("Range could only be setup once");

            cfg.AddRule(new BlackboardNumberRangeLogicalTypeRule<TNumber>(cfg.LogicalRecordTypePattern, minIncluded, maxExclude));

            return cfg;
        }

        /// <summary>
        /// Determines the minimum of the parameters tolerate
        /// </summary>
        public static ILogicalTypeCheckConfiguration<TNumber> Min<TNumber>(this ILogicalTypeCheckConfiguration<TNumber> cfg,
                                                                      TNumber minIncluded)
            where TNumber : struct, INumber<TNumber>
        {
            if (cfg.Rules.Any(r => r is BlackboardNumberRangeLogicalTypeRule<TNumber>))
                throw new InvalidOperationException("Range could only be setup once");

            cfg.AddRule(new BlackboardNumberRangeLogicalTypeRule<TNumber>(cfg.LogicalRecordTypePattern, minIncluded, null));

            return cfg;
        }

        /// <summary>
        /// Determines the maximum of the parameters tolerate
        /// </summary>
        public static ILogicalTypeCheckConfiguration<TNumber> Max<TNumber>(this ILogicalTypeCheckConfiguration<TNumber> cfg,
                                                                      TNumber maxExclude)
            where TNumber : struct, INumber<TNumber>
        {
            if (cfg.Rules.Any(r => r is BlackboardNumberRangeLogicalTypeRule<TNumber>))
                throw new InvalidOperationException("Range could only be setup once");

            cfg.AddRule(new BlackboardNumberRangeLogicalTypeRule<TNumber>(cfg.LogicalRecordTypePattern, null, maxExclude));

            return cfg;
        }

        /// <summary>
        /// Matches the regex.
        /// </summary>
        public static ILogicalTypeCheckConfiguration<string> MatchRegex(this ILogicalTypeCheckConfiguration<string> cfg,
                                                                   string regex,
                                                                   RegexOptions regexOptions = RegexOptions.None)
        {
            if (cfg.Rules.Any(r => r is BlackboardRegexLogicalTypeRule))
                throw new InvalidOperationException("Range could only be setup once");

            cfg.AddRule(new BlackboardRegexLogicalTypeRule(cfg.LogicalRecordTypePattern, regex, regexOptions));

            return cfg;
        }
    }
}
