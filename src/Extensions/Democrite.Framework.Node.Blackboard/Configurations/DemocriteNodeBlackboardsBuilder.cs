// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Validators;
    using Democrite.Framework.Node.Configurations;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using System;

    /// <summary>
    /// Blackboard definition builder
    /// </summary>
    /// <seealso cref="IDemocriteNodeBlackboardsBuilder" />
    /// <seealso cref="IDemocriteNodeBlackboardsLocalDefinitionBuilder" />
    internal sealed class DemocriteNodeBlackboardsBuilder : IDemocriteNodeBlackboardsBuilder,
                                                            IDemocriteNodeBlackboardsLocalDefinitionBuilder
    {
        #region Fields

        private readonly InMemoryBlackboardTemplateDefinitionProviderSource _inMemoryBlackboardTemplateDefinitionProviderSource;
        private readonly IDemocriteExtensionBuilderTool _wizardTools;
        private readonly IServiceCollection _serviceCollection;
        private int _buildOnceCounter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteNodeBlackboardsBuilder"/> class.
        /// </summary>
        public DemocriteNodeBlackboardsBuilder(IDemocriteExtensionBuilderTool wizardTools)
        {
            this._wizardTools = wizardTools;
            this._serviceCollection = wizardTools.GetServiceCollection();

            this._inMemoryBlackboardTemplateDefinitionProviderSource = new InMemoryBlackboardTemplateDefinitionProviderSource();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the blackboard services.
        /// </summary>
        internal void BuildServices()
        {
            if (Interlocked.Increment(ref _buildOnceCounter) > 1)
                return;

            this._serviceCollection.AddSingleton<IBlackboardTemplateDefinitionProviderSource>(this._inMemoryBlackboardTemplateDefinitionProviderSource);
            this._serviceCollection.TryAddSingleton<IBlackboardTemplateDefinitionProvider, BlackboardTemplateDefinitionProvider>();
            this._serviceCollection.TryAddSingleton<IBlackboardProvider, BlackboardProvider>();
            this._serviceCollection.TryAddSingleton<IBlackboardDataLogicalTypeRuleValidatorProvider, BlackboardDataLogicalTypeRuleValidatorProvider>();
        }

        /// <inheritdoc />
        public IDemocriteNodeBlackboardsBuilder AddInMemoryDefinitionProvider(Action<IDemocriteNodeBlackboardsLocalDefinitionBuilder> builder)
        {
            builder?.Invoke(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeBlackboardsLocalDefinitionBuilder SetupTemplates(params BlackboardTemplateDefinition[] templates)
        {
            foreach (var tmpl in templates)
                this._inMemoryBlackboardTemplateDefinitionProviderSource.AddOrUpdate(tmpl);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeBlackboardsBuilder UseInMemoryStorageForBoardState()
        {
            ((ISiloBuilder)this._wizardTools.SourceOrleanBuilder).AddMemoryGrainStorage(BlackboardConstants.BlackboardStateStorageConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeBlackboardsBuilder UseInMemoryStorageForRegistryState()
        {
            ((ISiloBuilder)this._wizardTools.SourceOrleanBuilder).AddMemoryGrainStorage(BlackboardConstants.BlackboardRegistryStorageConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeBlackboardsBuilder UseInMemoryStorageForRecords()
        {
            var memoryBuilder = this._wizardTools.TryGetBuilder<IDemocriteNodeMemoryBuilder>();
            memoryBuilder.UseInMemoryRepository(BlackboardConstants.BlackboardStorageRecordsConfigurationKey);
            return this;
        }

        #endregion
    }
}
