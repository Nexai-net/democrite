// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

using Democrite.Framework.Cluster.Abstractions.Attributes;
using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Cluster.Mongo.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("mongo")]

[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoMembershipsMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDemocriteMemoryAutoConfigurator, AutoDemocriteMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDefaultMemoryAutoConfigurator, AutoDefaultMemoryMongoConfigurator>()]
[assembly: AutoConfigurator<INodeReminderStateMemoryAutoConfigurator, AutoReminderMongoConfigurator>()]
[assembly: AutoConfigurator<INodeCustomMemoryAutoConfigurator, AutoCustomMemoryMongoConfigurator>()]
