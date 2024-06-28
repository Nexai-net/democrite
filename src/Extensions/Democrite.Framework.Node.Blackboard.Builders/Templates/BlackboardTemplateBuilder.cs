// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using System.Linq;

    /// <summary>
    /// <see cref="BlackboardTemplateDefinition"/> build first step
    /// To force the controller to be setup
    /// </summary>
    internal sealed class BlackboardTemplateBuilder : IBlackboardTemplateBuilder,
                                                      IBlackboardTemplateFinalizerBuilder,
                                                      IBlackboardTemplateControllerBuilder,
                                                      IBlackboardTemplateOptionsBuilder
    {
        #region Fields

        private static readonly BlackboardStorageDefinition s_defaultSystemStorage;

        private readonly Dictionary<BlackboardControllerTypeEnum, IDefinitionBaseBuilder<BlackboardTemplateControllerDefinition>> _storageControllers;
        private readonly List<BlackboardLogicalTypeBaseRule> _logicalTypeDefinitions;

        private BlackboardStorageDefinition? _defaultBlackboardStorageDefinitions;
        private readonly string _nameIdentifier;
        private readonly Guid _uid;

        private bool _anyLogicalType;
        private bool _initializationRequired;

        private bool _allLogicalTypeUnique;
        private bool _allLogicalTypeUniqueAllowReplace;
        private DefinitionMetaData? _definitionMetaData;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BlackboardTemplateBuilder"/> class.
        /// </summary>
        static BlackboardTemplateBuilder()
        {
            s_defaultSystemStorage = new BlackboardStorageDefinition(BlackboardConstants.BlackboardStorageRecordsKey, BlackboardConstants.BlackboardStorageRecordsConfigurationKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateBuilder"/> class.
        /// </summary>
        public BlackboardTemplateBuilder(string nameIdentifier, Guid? fixUid)
        {
            ArgumentNullException.ThrowIfNull(nameIdentifier);

            this._nameIdentifier = nameIdentifier;
            this._uid = fixUid ?? Guid.NewGuid();

            this._logicalTypeDefinitions = new List<BlackboardLogicalTypeBaseRule>();

            this._defaultBlackboardStorageDefinitions = s_defaultSystemStorage;
            this._storageControllers = new Dictionary<BlackboardControllerTypeEnum, IDefinitionBaseBuilder<BlackboardTemplateControllerDefinition>>();

            // Only BlackboardControllerTypeEnum.Storage have a controller setups by default
            this.Storage.UseDefault();

            //var genericControllerBuilder = new BlackboardTemplateGenericControllerBuilder(this, BlackboardControllerTypeEnum.Storage);
            //this._storageControllers[BlackboardControllerTypeEnum.Storage] = genericControllerBuilder;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IBlackboardTemplateStorageControllerBuilder Storage
        {
            get
            {
                var storageControllerBuilder = new BlackboardTemplateStorageControllerBuilder(this);
                this._storageControllers[BlackboardControllerTypeEnum.Storage] = storageControllerBuilder;
                return storageControllerBuilder;
            }
        }

        /// <inheritdoc />
        public IBlackboardTemplateEventControllerBuilder Event
        {
            get
            {
                var eventControllerBuilder = new BlackboardTemplateEventControllerBuilder(this);
                this._storageControllers[BlackboardControllerTypeEnum.Event] = eventControllerBuilder;
                return eventControllerBuilder;
            }
        }

        /// <inheritdoc />
        public IBlackboardTemplateStateControllerBuilder State
        {
            get
            {
                var stateControllerBuilder = new BlackboardTemplateStateControllerBuilder(this);
                this._storageControllers[BlackboardControllerTypeEnum.State] = stateControllerBuilder;
                return stateControllerBuilder;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IBlackboardTemplateFinalizerBuilder LogicalTypeConfiguration(string logicRecordTypePattern, Action<ILogicalTypeConfiguration> cfg)
        {
            ArgumentNullException.ThrowIfNull(logicRecordTypePattern);

            if (!logicRecordTypePattern.StartsWith("^"))
                logicRecordTypePattern = "^" + logicRecordTypePattern;

            if (!logicRecordTypePattern.EndsWith("$"))
                logicRecordTypePattern += "$";

            var config = new LogicalTypeConfiguration(logicRecordTypePattern);

            cfg(config);

            var rules = config.GenerateRules();

            if (!rules.OfType<BlackboardOrderLogicalTypeRule>().Any())
                rules = rules.Append(new BlackboardOrderLogicalTypeRule(logicRecordTypePattern, (short)this._logicalTypeDefinitions.Count)).ToArray();

            this._logicalTypeDefinitions.AddRange(rules);

            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateFinalizerBuilder SetupControllers(Action<IBlackboardTemplateControllerBuilder> builders)
        {
            builders?.Invoke(this);
            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateControllerBuilder Multi(BlackboardControllerTypeEnum types, Action<IBlackboardTemplateGenericControllerBuilder> builder)
        {
            if (types == BlackboardControllerTypeEnum.None)
                throw new NotSupportedException("Don't suppoert None controller type");

            var genericControllerBuilder = new BlackboardTemplateGenericControllerBuilder(this, types);

            builder?.Invoke(genericControllerBuilder);

            var explodeTypes = Enum.GetValues<BlackboardControllerTypeEnum>()
                                   .Where(t => t != BlackboardControllerTypeEnum.None && (t & types) == t)
                                   .ToArray();

            foreach (var explodeType in explodeTypes)
                this._storageControllers[explodeType] = genericControllerBuilder;

            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateFinalizerBuilder SetupDefaultStorage(string storageKey, string? storageConfiguration = null)
        {
            this._defaultBlackboardStorageDefinitions = new BlackboardStorageDefinition(storageKey,
                                                                                        string.IsNullOrEmpty(storageConfiguration)
                                                                                                    ? BlackboardConstants.BlackboardStorageRecordsConfigurationKey
                                                                                                    : storageConfiguration);
            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateFinalizerBuilder AnyLogicalTypeConfiguration(Action<ILogicalTypeConfiguration>? cfg = null)
        {
            this._anyLogicalType = true;
            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateBuilder ConfigureOptions(Action<IBlackboardTemplateOptionsBuilder> optionsBuilder)
        {
            optionsBuilder?.Invoke(this);
            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateOptionsBuilder InitializationRequired()
        {
            this._initializationRequired = true;
            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateOptionsBuilder AllLogicalTypeUnique(bool replace = false)
        {
            this._allLogicalTypeUnique = true;
            this._allLogicalTypeUniqueAllowReplace = replace;
            return this;
        }

        /// <inheritdoc />
        public IBlackboardTemplateBuilder MetaData(Action<IDefinitionMetaDataBuilder> action)
        {
            this._definitionMetaData = DefinitionMetaDataBuilder.Execute(action);
            return this;
        }

        /// <inheritdoc />
        public BlackboardTemplateDefinition Build()
        {
            var controllers = this._storageControllers.GroupBy(k => k.Value)
                                                      .Select(kv =>
                                                      {
                                                          var def = kv.Key.Build();

                                                          if (kv.Count() == 1)
                                                              return def;

                                                          var targetControllerType = kv.Aggregate(BlackboardControllerTypeEnum.None, (acc, v) => acc | v.Key);

                                                          if (def.ControllerType == targetControllerType)
                                                              return def;

                                                          if (def.GetType() == typeof(BlackboardTemplateControllerDefinition))
                                                              return new BlackboardTemplateControllerDefinition(def.Uid, targetControllerType, def.AgentInterfaceType, def.Options, null);

                                                          throw new InvalidDataException($"Multiple controller option for the same type that use more complex definition type than BlackboardTemplateControllerDefinition. Duplicate Keys {def.ControllerType ^ targetControllerType:G}");
                                                      })
                                                      .ToArray();
             
            var rules = this._logicalTypeDefinitions.Distinct().ToArray();

            var indexedRules = rules.GroupBy(r => r.LogicalTypePattern)
                                    .ToDictionary(k => k.Key);

            if (this._anyLogicalType && !indexedRules.ContainsKey(".*"))
            {
                rules = rules.Append(new BlackboardOrderLogicalTypeRule(".*", short.MaxValue))
                             .Append(new BlackboardStorageLogicalTypeRule(".*", s_defaultSystemStorage))
                             .ToArray();
            }
            else if (indexedRules.TryGetValue(".*", out var defaultRules))
            {
                if (!defaultRules.Select(r => r).OfType<BlackboardOrderLogicalTypeRule>().Any())
                    rules = rules.Append(new BlackboardOrderLogicalTypeRule(".*", short.MaxValue)).ToArray();

                if (!defaultRules.Select(r => r).OfType<BlackboardStorageLogicalTypeRule>().Any())
                    rules = rules.Append(new BlackboardStorageLogicalTypeRule(".*", s_defaultSystemStorage)).ToArray();
            }

            if (this._allLogicalTypeUnique)
            {
                var uniqueRules = new List<BlackboardLogicalTypeUniqueRule>();
                foreach (var logicalType in rules.GroupBy(r => r.LogicalTypePattern)
                                                 .Where(rls => rls.OfType<BlackboardLogicalTypeUniqueRule>().Any() == false))
                {
                    uniqueRules.Add(new BlackboardLogicalTypeUniqueRule(logicalType.Key, this._allLogicalTypeUniqueAllowReplace));
                }

                rules = rules.Concat(uniqueRules).ToArray();
            }

            return new BlackboardTemplateDefinition(this._uid,
                                                    this._nameIdentifier,
                                                    controllers,
                                                    rules,

                                                    this._definitionMetaData,

                                                    new BlackboardTemplateConfigurationDefinition(this._initializationRequired),
                                                    this._defaultBlackboardStorageDefinitions);
        }

        #endregion
    }
}
