﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

using Democrite.Framework.Cluster.Abstractions.Attributes;
using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("Default")]

[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoDefaultMembershipsNodeConfigurator>()]
[assembly: AutoConfigurator<IClusterEndpointAutoConfigurator, AutoDefaultNodeEndpointAutoConfigurator>()]
[assembly: AutoConfigurator<INodeDemocriteMemoryAutoConfigurator, AutoDefaultDemocriteMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeReminderStateMemoryAutoConfigurator, AutoDefaultReminderStateMemoryAutoConfigurator>()]
[assembly: AutoConfigurator<INodeCustomMemoryAutoConfigurator, AutoDefaultCustomMemoryConfigurator>()]
