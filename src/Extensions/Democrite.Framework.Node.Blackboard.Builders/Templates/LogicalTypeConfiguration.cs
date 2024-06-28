// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using Elvex.Toolbox.Models;

    /// <summary>
    /// Configuration dedicated to one type on record on the blackboard
    /// </summary>
    internal abstract class LogicalTypeBaseConfiguration : ILogicalTypeBaseConfiguration
    {
        #region Fields

        private readonly Dictionary<string, BlackboardLogicalTypeBaseRule> _rules;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalTypeBaseConfiguration"/> class.
        /// </summary>
        protected LogicalTypeBaseConfiguration(string logicalRecordTypePattern)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(logicalRecordTypePattern);

            this.LogicalRecordTypePattern = logicalRecordTypePattern;
            this._rules = new Dictionary<string, BlackboardLogicalTypeBaseRule>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the rules.
        /// </summary>
        public IReadOnlyCollection<BlackboardLogicalTypeBaseRule> Rules
        {
            get { return this._rules.Values; }
        }

        /// <summary>
        /// Gets the logical record type pattern.
        /// </summary>
        public string LogicalRecordTypePattern { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AddRule(BlackboardLogicalTypeBaseRule rule)
        {
            EnqueueRule(rule);
        }

        /// <summary>
        /// Enqueues the rule.
        /// </summary>
        protected void EnqueueRule(BlackboardLogicalTypeBaseRule blackboardLogicalTypeBaseRule)
        {
            this._rules[blackboardLogicalTypeBaseRule.UniqueIdentity] = blackboardLogicalTypeBaseRule;
        }

        #endregion
    }

    /// <summary>
    /// Configuration dedicated to one type on record on the blackboard
    /// </summary>
    internal sealed class LogicalTypeCheckConfiguration<TType> : LogicalTypeBaseConfiguration, ILogicalTypeCheckConfiguration<TType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalTypeConfiguration{TType}"/> class.
        /// </summary>
        public LogicalTypeCheckConfiguration(string logicalRecordTypePattern)
            : base(logicalRecordTypePattern)
        {
            
        }

        /// <summary>
        /// Converts to rule.
        /// </summary>
        public BlackboardLogicalTypeBaseRule ToRule()
        {
            return new BlackboardTypeCheckLogicalTypeRule(this.LogicalRecordTypePattern,
                                                          (ConcretType)typeof(TType).GetAbstractType(),
                                                          base.Rules);
        }
    }

    /// <summary>
    /// Configuration dedicated to one type on record on the blackboard
    /// </summary>
    internal sealed class LogicalTypeConfiguration : LogicalTypeBaseConfiguration, ILogicalTypeConfiguration
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalTypeConfiguration"/> class.
        /// </summary>
        public LogicalTypeConfiguration(string logicalRecordTypePattern, string? storagestateName = null)
            : base(logicalRecordTypePattern)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ILogicalTypeConfiguration OnlyOne(bool includeDecommissioned = false, bool replacePreviousOne = false)
        {
            MaxRecord(1,
                      includeDecommissioned,
                      preferenceResolution: (replacePreviousOne ? BlackboardProcessingResolutionLimitTypeEnum.ClearAll : BlackboardProcessingResolutionLimitTypeEnum.Reject));
            return this;
        }

        /// <inheritdoc />
        public ILogicalTypeConfiguration MaxRecord(short maxRecords,
                                                   bool includeDecommissioned = false,
                                                   BlackboardProcessingResolutionLimitTypeEnum? preferenceResolution = null,
                                                   BlackboardProcessingResolutionRemoveTypeEnum? removeResolution = null)
        {
            EnqueueRule(new BlackboardMaxRecordLogicalTypeRule(this.LogicalRecordTypePattern,
                                                               includeDecommissioned,
                                                               maxRecords,
                                                               preferenceResolution,
                                                               removeResolution));
            return this;
        }

        /// <inheritdoc />
        public ILogicalTypeConfiguration AllowType<T>(Action<ILogicalTypeCheckConfiguration<T>>? cfg = null)
        {
            var build = new LogicalTypeCheckConfiguration<T>(this.LogicalRecordTypePattern);
            cfg?.Invoke(build);
            EnqueueRule(build.ToRule());
            return this;
        }

        /// <summary>
        /// Generates the rules.
        /// </summary>
        public IReadOnlyCollection<BlackboardLogicalTypeBaseRule> GenerateRules()
        {
            return this.Rules;
        }

        /// <inheritdoc />
        public ILogicalTypeConfiguration Storage(string storageKey, string? storageConfiguration = null)
        {
            EnqueueRule(new BlackboardStorageLogicalTypeRule(this.LogicalRecordTypePattern,
                                                             new BlackboardStorageDefinition(string.IsNullOrEmpty(storageKey) 
                                                                                                        ? BlackboardConstants.BlackboardStorageRecordsKey 
                                                                                                        : storageKey,

                                                                                             string.IsNullOrEmpty(storageConfiguration) 
                                                                                                        ? BlackboardConstants.BlackboardStorageRecordsConfigurationKey
                                                                                                        : storageConfiguration)));
            return this;
        }

        /// <inheritdoc />
        public ILogicalTypeConfiguration UseDefaultStorage(string storageKey)
        {
            EnqueueRule(new BlackboardStorageLogicalTypeRule(this.LogicalRecordTypePattern,
                                                             new BlackboardStorageDefinition(storageKey,
                                                                                             BlackboardConstants.BlackboardStateStorageConfigurationKey)));
            return this;
        }

        /// <inheritdoc />
        public ILogicalTypeConfiguration Order(ushort order)
        {
            EnqueueRule(new BlackboardOrderLogicalTypeRule(this.LogicalRecordTypePattern, (short)order));
            return this;
        }

        /// <inheritdoc />
        public ILogicalTypeConfiguration RemainOnSealed()
        {
            EnqueueRule(new BlackboardRemainOnSealedLogicalTypeRule(this.LogicalRecordTypePattern));
            return this;
        }

        /// <inheritdoc />
        public ILogicalTypeConfiguration Unique(bool replace = false)
        {
            EnqueueRule(new BlackboardLogicalTypeUniqueRule(this.LogicalRecordTypePattern, replace));
            return this;
        }

        #endregion
    }
}
