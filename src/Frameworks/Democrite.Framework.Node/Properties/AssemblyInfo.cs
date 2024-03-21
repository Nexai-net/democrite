// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

using Democrite.Framework.Cluster.Abstractions.Attributes;
using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("Default")]

//Cluster
[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoDefaultMembershipsNodeConfigurator>()]
[assembly: AutoConfigurator<IClusterEndpointAutoConfigurator, AutoDefaultNodeEndpointAutoConfigurator>()]

// Memory
[assembly: AutoConfigurator<INodeDemocriteMemoryAutoConfigurator, AutoDefaultDemocriteMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeDemocriteAdminMemoryAutoConfigurator, AutoDefaultDemocriteAdminMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeReminderStateMemoryAutoConfigurator, AutoDefaultReminderStateMemoryAutoConfigurator>()]
[assembly: AutoConfigurator<INodeCustomGrainMemoryAutoConfigurator, AutoDefaultCustomGrainMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeDefaultMemoryAutoConfigurator, AutoDefaultMemoryConfigurator>()]

// Repository
[assembly: AutoConfigurator<INodeDefaultRepositoryMemoryAutoConfigurator, AutoDefaultRepositoryMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeCustomRepositoryMemoryAutoConfigurator, AutoDefaultCustomRepositoryMemoryConfigurator>()]