// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

using Democrite.Framework.Cluster.Abstractions.Attributes;
using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
using Democrite.Framework.Node.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("Default")]

[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoDefaultNodeConfigurator>()]
[assembly: AutoConfigurator<INodeVGrainStateMemoryAutoConfigurator, AutoDefaultVGrainStateMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeReminderStateMemoryAutoConfigurator, AutoDefaultReminderStateMemoryAutoConfigurator>()]

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Democrite.Framework.Node.UnitTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Democrite.UnitTests.ToolKit")]