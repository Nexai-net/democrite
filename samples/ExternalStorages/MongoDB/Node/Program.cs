// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Create a mongoDB using the folling command "docker run -d -p 27017:27017 mongo:latest"
// You can watch and edit data in you data base using mongo tools like studio 3T https://studio3t.com/

using Common;

using Democrite.Framework;
using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions.Enums;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Node.Grains;

var node = DemocriteNode.Create((host, cfg) => cfg.AddJsonFile("appsettings.json"),
                                b =>
                                {
                                    b.WizardConfig()

                                     // Setup mongo db as cluster meeting point
                                     .UseMongoCluster()

                                     // Expose the current node to client communication
                                     .ExposeNodeToClient()

                                     // Setup cron handlers
                                     .UseCronTriggers()

                                     // Define the execution environement all the debug tool 
                                     .AddDebugTools(LogLevel.Warning)

                                     // Define mongo db as definition source
                                     .AddMongoDefinitionProvider(o => o.ConnectionString("127.0.0.1:27017"))

                                     .ConfigureLogging(c => c.AddConsole())

                                     // Set manually the VGrain definition
                                     .SetupNodeVGrains(s => s.Add<ICounterVGrain, CounterVGrain>())

                                     // Setup mongo db as default storage
                                     .SetupNodeMemories(m =>
                                     {
                                         m.UseMongoStorage(StorageTypeEnum.All);
                                     });

                                    /*
                                     * Orlean dashboard allow to see the cluster running
                                     * through a web site => http://localhost:8080
                                     */
                                    b.ManualyAdvancedConfig()
                                     .UseDashboard();
                                });

await using(node)
{
    await node.StartUntilEndAsync();
}
