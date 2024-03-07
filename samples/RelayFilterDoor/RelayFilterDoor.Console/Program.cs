// See https://aka.ms/new-console-template for more information

using Democrite.Framework;
using Democrite.Framework.Builders;
using Democrite.Framework.Builders.Signals;
using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions.Enums;
using Democrite.Framework.Core.Abstractions.Signals;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var signalInit = Signal.Create("loopInit");
var signalEveryMin = Signal.Create("everyMinSignal");
var signalAndTriggerPopulate = Signal.Create("andTriggerPopulate");

var loopDoor = Door.Create("loopEvery10sec")
                   .Listen(signalInit)
                   .UseLogicalAggregator(b =>
                   {
                       return b.ActiveWindowInterval(TimeSpan.FromSeconds(10))
                               .AssignVariableName("Init", signalInit)
                               .UseVariableThis()
                               .Formula("!Init & !this");
                   })
                   .Build();

var numberDoorSource = Trigger.Door(loopDoor, "NumberSource")
                              .AddTargetSignal(signalAndTriggerPopulate.SignalId)
                              .SetOutput(s => s.StaticCollection(new[] { 0, 1, 2, 3, 4, 5 })
                                                    .PullMode(PullModeEnum.Circling))
                              .Build();

var oodDoor = Door.Create("oodDoorFilter")
                  .Listen(signalAndTriggerPopulate)
                  .UseRelayFilter<int>((value, signal) => value % 2 == 1)
                  .Build();

var evenDoor = Door.Create("evenDoorFilter")
                   .Listen(signalAndTriggerPopulate)
                   .UseRelayFilter<int>((value, signal) => value % 2 == 0)
                   .Build();

var allSignals = new[]
{
    signalAndTriggerPopulate,
    signalEveryMin,
    signalInit
};

var allDoors = new[]
{
    oodDoor,
    evenDoor,
};

/*
 *    | [loopDoor] /10sec | -
 *                          - |                    | ----- [If % 2 = 1] --> Sequence Ood
 *                            | [numberDoorSource] | ----- [If % 2 = 0] --> Sequence Event
 *                                            > [1, 2, 3, 4, 5, 6]
 */
var node = DemocriteNode.Create(b =>
{
    b.WizardConfig()
     .NoCluster()
     .ConfigureLogging(logging => logging.AddConsole())
     .UseSignals()

     .AddDebugTools(LogLevel.Warning)

     .ShowDoors(allDoors)

     .AddInMemoryDefinitionProvider(b =>
     {
         b.SetupSignals(signalInit,
                        signalEveryMin,
                        signalAndTriggerPopulate)

          .SetupDoors(loopDoor,
                      oodDoor,
                      evenDoor)

          .SetupTriggers(numberDoorSource);
     });
});

await using (node)
{
    await node.StartUntilEndAsync((service, execHandler, token) =>
    {
        var signalService = service.GetRequiredService<ISignalService>();
        signalService.Fire(signalInit.SignalId, token);

        return Task.CompletedTask;
    });
}