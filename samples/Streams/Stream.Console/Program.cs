// See https://aka.ms/new-console-template for more information

using Democrite.Framework;
using Democrite.Framework.Bag.DebugTools;
using Democrite.Framework.Bag.Toolbox.Abstractions;
using Democrite.Framework.Builders;
using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions.Enums;

using Elvex.Toolbox.Abstractions.Models;

using Microsoft.Extensions.Logging;

var consumeSeq = Sequence.Build("Display input with deplay")
                         .RequiredInput<int>()
                         .Use<IDisplayInfoVGrain>().Call((g, m, ctx) => g.DisplayCallInfoAsync(m, ctx)).ReturnNoData
                         .Use<IDelayVGrain>().Configure(TimeSpanRange.Create(500, 700))
                                             .Call((g, ctx) => g.RandomDelayAsync(ctx)).Return
                         .Build();

var fixStreamDefinitionUid = new Guid("BA649F55-09A5-41C9-8C40-D0FCA3833EAE");

var streamDef = StreamQueue.CreateFromDefaultStream("TestFlux", fixStreamDefinitionUid);

var fromStreamTrigger = Trigger.Stream(streamDef)
                               .AddTargetSequence(consumeSeq.Uid)
                               .MaxConcurrentProcess(2)
                               .Build();

//// All 5 sec push value to stream
var pushToStreamTrigger = Trigger.Cron("*/25 * * * * *", "TGR: Push Every 25 sec")
                                 .AddTargetStream(streamDef)
                                 .SetOutput(s => s.StaticCollection(Enumerable.Range(0, 50))
                                                       .PullMode(PullModeEnum.Broadcast))
                                 .Build();

var node = DemocriteNode.Create(b =>
{
    b.WizardConfig()
     .NoCluster()

     .UseCronTriggers(supportSecondCron: true)
     .UseStreamQueues(b =>
     {
         b.SetupDefaultDemocriteMemoryStream();
     })

     .AddDebugTools(LogLevel.Warning)
     .AddToolBoxTools()

     .ConfigureLogging(c => c.AddConsole())

     .AddInMemoryDefinitionProvider(p =>
     {
         p.SetupSequences(consumeSeq)
          .SetupTriggers(fromStreamTrigger, pushToStreamTrigger)
          .SetupStreamQueues(streamDef);
     });
});

await using (node)
{
    await node.StartUntilEndAsync(async (p, handler, token) =>
    {
        //var streamProvider = p.GetRequiredServiceByName<IStreamProvider>(StreamQueueDefinition.DEFAULT_STREAM_KEY); /*GrainStreamingExtensions.GetStreamProvider(this, targetKV.Key);*/

        //var stream = streamProvider.GetStream<object>(StreamId.Get("TestFlux", fixStreamDefinitionUid));

        //for (int i = 0; i < 200; i++)
        //    await stream.OnNextAsync(i);
    });
}