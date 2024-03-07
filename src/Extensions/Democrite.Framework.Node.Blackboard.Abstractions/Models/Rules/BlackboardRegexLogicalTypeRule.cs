// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class BlackboardRegexLogicalTypeRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardRegexLogicalTypeRule"/> class.
        /// </summary>
        public BlackboardRegexLogicalTypeRule(string logicalTypePattern,
                                              string matchRegex,
                                              RegexOptions regexOptions) 
            : base(logicalTypePattern)
        {
            ArgumentNullException.ThrowIfNull(matchRegex);
            this.MatchRegex = matchRegex;
            this.RegexOptions = regexOptions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the match regex.
        /// </summary>
        [DataMember]
        public string MatchRegex { get; }

        /// <summary>
        /// Gets the regex options.
        /// </summary>
        [DataMember]
        public RegexOptions RegexOptions { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardRegexLogicalTypeRule reg &&
                   this.MatchRegex == reg.MatchRegex &&
                   this.RegexOptions == reg.RegexOptions;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.MatchRegex, this.RegexOptions);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Regex - {this.MatchRegex}/{this.RegexOptions}";
        }

        #endregion
    }
}
