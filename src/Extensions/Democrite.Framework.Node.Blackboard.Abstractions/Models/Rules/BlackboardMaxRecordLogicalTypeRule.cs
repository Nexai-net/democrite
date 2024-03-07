// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class BlackboardMaxRecordLogicalTypeRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardMaxRecordLogicalTypeRule"/> class.
        /// </summary>
        public BlackboardMaxRecordLogicalTypeRule(string logicalTypePattern,
                                                  bool includeDecommissioned,
                                                  short maxRecord,
                                                  BlackboardProcessingResolutionLimitTypeEnum? preferenceResolution,
                                                  BlackboardProcessingResolutionRemoveTypeEnum? removeResolution)
            : base(logicalTypePattern)
        {
            this.MaxRecord = maxRecord;
            this.IncludeDecommissioned = includeDecommissioned;

            this.PreferenceResolution = preferenceResolution;
            this.RemoveResolution = removeResolution;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the maximum record; -1 to remove any limitation
        /// </summary>
        [DataMember]
        public short MaxRecord { get; }

        /// <summary>
        /// Gets a value indicating whether [include Decommissioned].
        /// </summary>
        [DataMember]
        public bool IncludeDecommissioned { get; }

        /// <summary>
        /// Gets the preference resolution requested
        /// </summary>
        [DataMember()]
        public BlackboardProcessingResolutionLimitTypeEnum? PreferenceResolution { get; }

        /// <summary>
        /// Gets the preference remove resolution.
        /// </summary>
        [DataMember()]
        public BlackboardProcessingResolutionRemoveTypeEnum? RemoveResolution { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardMaxRecordLogicalTypeRule max &&
                   this.MaxRecord == max.MaxRecord &&
                   this.IncludeDecommissioned == max.IncludeDecommissioned &&
                   this.PreferenceResolution == max.PreferenceResolution &&
                   this.RemoveResolution == max.RemoveResolution;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.MaxRecord,
                                    this.IncludeDecommissioned,
                                    this.PreferenceResolution,
                                    this.RemoveResolution);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Max Record - {this.MaxRecord} - (IncludeDecommissioned: {this.IncludeDecommissioned}) (PreferenceResolution: {this.PreferenceResolution}) (RemoveResolution: {this.RemoveResolution})";
        }

        #endregion
    }
}
