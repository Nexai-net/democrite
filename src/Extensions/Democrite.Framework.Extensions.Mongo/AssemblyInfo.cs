// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

using Democrite.Framework.Cluster.Abstractions.Attributes;
using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("mongo")]

// Cluster
[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoMembershipsMongoConfigurator>()]

// Storage
[assembly: AutoConfigurator<INodeDemocriteMemoryAutoConfigurator, AutoDemocriteMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDemocriteAdminMemoryAutoConfigurator, AutoDemocriteAdminMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDefaultMemoryAutoConfigurator, AutoDefaultMemoryMongoConfigurator>()]
[assembly: AutoConfigurator<INodeReminderStateMemoryAutoConfigurator, AutoReminderMongoConfigurator>()]
[assembly: AutoConfigurator<INodeCustomGrainMemoryAutoConfigurator, AutoCustomGrainMemoryMongoConfigurator>()]
[assembly: AutoConfigurator<INodeCustomDefinitionProviderAutoConfigurator, AutoCustomDefinitionProviderMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDemocriteDynamicDefinitionsMemoryAutoConfigurator, AutoDemocriteDynamicDefinitionsMongoConfigurator>()]

// Repository
[assembly: AutoConfigurator<INodeCustomRepositoryMemoryAutoConfigurator, AutoCustomRepositoryMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDefaultRepositoryMemoryAutoConfigurator, AutoDefaultRepositoryMongoConfigurator>()]
