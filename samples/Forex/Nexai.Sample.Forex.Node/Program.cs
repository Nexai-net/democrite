// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

using Democrite.Framework;
using Democrite.Framework.Builders;
using Democrite.Framework.Builders.Signals;
using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions.Enums;
using Democrite.Framework.Core.Abstractions.Sequence;
using Democrite.Framework.Core.Abstractions.Triggers;
using Democrite.Framework.Node.Abstractions.Configurations;
using Democrite.Framework.Node.Configurations;
using Democrite.VGrains.Web.Abstractions;

using Elvex.Toolbox.Abstractions.Enums;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nexai.Sample.Forex.VGrain;
using Nexai.Sample.Forex.VGrain.Abstractions;

static SequenceDefinition CreateCollectorSequence(string currencyPair,
                                                  IEnumerable<Uri> collectionsources,
                                                  out TriggerDefinition triggerDefinition,
                                                  string cron = "* * * * *")
{
    var collectorSequence = Sequence.Build("Fetch:" + currencyPair)
                                    .RequiredInput<Uri>()
                                    .Use<IHtmlCollectorVGrain>().Call((a, url, ctx) => a.FetchPageAsync(url, ctx)).Return
                                    .Use<IPriceInspectorVGrain>().Configure(currencyPair)
                                                                      .Call((a, page, ctx) => a.SearchValueAsync(page, ctx)).Return

                                    .Use<ICurrencyPairVGrain>().Configure(currencyPair)
                                                                         .Call((a, data, ctx) => a.StoreAsync(data, ctx)).Return
                                    .Build();

    triggerDefinition = Trigger.Cron(cron, "CronTick:" + currencyPair)// "* 9-18 * * mon-fri" // Every minutes between 9h and 18h UTC between monday and friday
                               
                               // Define what will be trigged
                               .AddTargetSequence(collectorSequence)

                               .SetOutput(input => input.StaticCollection(collectionsources)
                                                             .PullMode(PullModeEnum.Circling))
                               .Build();

    return collectorSequence;
}

var usdSources = new[]
{
    new Uri("https://fr.investing.com/currencies/eur-usd"),
    new Uri("https://www.boursorama.com/bourse/devises/taux-de-change-euro-dollar-EUR-USD/"),
    new Uri("https://forex.tradingsat.com/cours-euro-dollar-FX0000EURUSD/")
};

var chfSources = new[]
{
    new Uri("https://fr.investing.com/currencies/eur-chf"),
    new Uri("https://www.boursorama.com/bourse/devises/taux-de-change-euro-francsuisse-EUR-CHF/"),
    new Uri("https://forex.tradingsat.com/cours-euro-franc-suisse-FX0000EURCHF/")
};

var eurUsdCollectorSequence = CreateCollectorSequence("eur-usd", usdSources, out var cron2MinEurUsdDefinition, "* * * * *");
var eurChfCollectorSequence = CreateCollectorSequence("eur-chf", chfSources, out var cron2MinEurChfDefinition, "*/2 * * * *");

var valueEurUsdStoredAboveAverage = Signal.Create("CurrencyPair_eur-usd_Stored_Above_Average");
var valueEurChfStoredAboveAverage = Signal.Create("CurrencyPair_eur-chf_Stored_Above_Average");

var manualForceDoorFireing = Signal.Create("Manual_Force_Door_Fireing");

var door = Door.Create("CheckPairAboveAverage")
               .Listen(valueEurUsdStoredAboveAverage, valueEurChfStoredAboveAverage)
               
               // Basic
               .UseLogicalAggregator(LogicEnum.And, TimeSpan.FromSeconds(10))
               
               // Advanced
               //.UseLogicalAggregator(b =>
               //{
               //    return b.Interval(TimeSpan.FromSeconds(0.5))
               //            .AssignVariableName("A", valueEurUsdStoredAboveAverage)
               //            .AssignVariableName("B", valueEurChfStoredAboveAverage)
               //            .AssignVariableName("C", manualForceDoorFireing)
               
               //            /* Fire (if A and B are signal in an interval of 0.5 second except if i was already fire in less than 0.5 sseconds)
               
               //                    Or
               
               //                    C
               //             */
               //            .Formula("(A & B & !this) | C");
               //})
               .Build();

var node = DemocriteNode.Create((ctx, configBuilder) => configBuilder.AddJsonFile("appsettings.json", false),
                                 cfg =>
                                 {
                                     cfg.WizardConfig()
                                        .NoCluster()

                                        .SetupNodeMemories(m =>
                                        {
                                            m.UseInMemory(StorageTypeEnum.All);
                                        })

                                        .ExposeNodeToClient()

                                        .UseCronTriggers()
                                        .UseSignals()

                                        .ConfigureLogging(logging => logging.AddConsole())

                                        .AddInMemoryDefinitionProvider(m =>
                                        {
                                            m.SetupTriggers(cron2MinEurUsdDefinition, cron2MinEurChfDefinition)
                                             .SetupSignals(valueEurChfStoredAboveAverage, valueEurUsdStoredAboveAverage, manualForceDoorFireing)
                                             .SetupDoors(door)
                                             .SetupSequences(eurUsdCollectorSequence, eurChfCollectorSequence);
                                        })

                                        .SetupNodeVGrains(cfg =>
                                        {
                                            cfg.UseWebVGrains()
                                               .Add<IPriceInspectorVGrain, PriceInspectorVGrain>()
                                               .Add<ICurrencyPairVGrain, CurrencyPairVGrain>();
                                        });
                                 });

await using (node)
{
    await node.StartUntilEndAsync(async (service, handler, token) =>
    {
        var factory = service.GetRequiredService<IGrainFactory>();
        var listenerDemoVGrain = factory.GetGrain<ISignalDemoTargetVGrain>(Guid.NewGuid());

        //await listenerDemoVGrain.SubscribeToAsync(valueEurUsdStoredAboveAverage);
        //await listenerDemoVGrain.SubscribeToAsync(valueEurChfStoredAboveAverage);

        await listenerDemoVGrain.SubscribeToAsync(door);
    });
}