// See https://aka.ms/new-console-template for more information

using Democrite.Framework.Builders;
using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions;
using Democrite.Framework.Core.Abstractions.Enums;
using Democrite.Framework.Node.Blackboard.Abstractions.Models;
using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
using Democrite.Framework.Node.Blackboard.Abstractions.Models.Requests;
using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;
using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
using Democrite.Framework.Node.Blackboard.Builders;
using Democrite.Framework.Node.Blackboard.Builders.Templates;
using Democrite.Framework.Node.Configurations;

using Democrite.Sample.Blackboard.Memory.Controllers;
using Democrite.Sample.Blackboard.Memory.IVGrains;
using Democrite.Sample.Blackboard.Memory.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Democrite sample",
        Version = "v1"
    });
}).AddEndpointsApiExplorer();

var sumSequenceId = new Guid("BEFB98EC-655A-457A-B6A8-8507902768A3");
var blackboardId = new Guid("047C449C-8E64-475A-AEAD-732CC21E01D1");

var blackboardTemplate = BlackboardTemplate.Build("MathBlackboard", fixUid: blackboardId)
                                           .SetupControllers(c =>
                                           {
                                               c.Storage.UseDefault()
                                                .Event.UseController<IAutoComputeEventController, AutoComputeBlackboardOptions>(new AutoComputeBlackboardOptions(sumSequenceId));
                                           })
                                           .LogicalTypeConfiguration("Val[a-zA-Z]+", cfg: r =>
                                           {
                                               r.Storage("NumberValues")
                                                .MaxRecord(5,
                                                           preferenceResolution: BlackboardProcessingResolutionLimitTypeEnum.KeepNewest,
                                                           removeResolution: BlackboardProcessingResolutionRemoveTypeEnum.Decommission)
                                                .Order(42)
                                                .AllowType<int>(c => c.Range(0, 50))
                                                .AllowType<double>(c => c.Range(0, 50.42));
                                           })
                                           .LogicalTypeConfiguration("Result", r =>
                                           {
                                               r.OnlyOne(replacePreviousOne: true)
                                                .AllowType<int>(c => c.Min(0));
                                           })
                                           .Build();

var sumSequence = Sequence.Build("sumSequence", fixUid: sumSequenceId)
                          .RequiredInput<string>()

                          // Prepare fetch query to pull data from the correct blackboard
                          .Use<IBlackboardStoragePullRequestBuilderVGrain>().ConfigureFromInput((input) => new DataRecordPullRequestOption()
                          {
                              LogicalTypePattern = "Values",
                              RecordStatusFilter = RecordStatusEnum.Ready,
                              BoardName = input,
                              BoardTemplateName = "MathBlackboard"
                          })
                          .Call((g, i, ctx) => g.GetPullTargetFromInputAsync(i, ctx))
                          .Return

                          // Push in the execution context, carry through the sequence execution, the blackboard used to extract information
                          .PushToContext(req => new DataRecordRequestDataContext()
                          {
                              BoardId = req.BoardUid,
                              LogicalTypePattern = req.LogicalRecordTypePattern
                          })

                          // Pull information from the blackboard using the request generate above
                          .Use<IBlackboardStorageVGrain>().Call((g, req, ctx) => g.PullDataAsync<int>(req, ctx)).Return

                          // Compute data
                          .Use<ISumValuesVGrain>().Call((g, i, ctx) => g.Sum(i, ctx)).Return

                          // Prepare PUSH request to send the result data to a blackboard (by default search the execution context to identify the blackboard used)
                          .Use<IBlackboardStoragePushRequestBuilderVGrain>().Configure(new DataRecordPushRequestOption()
                          {
                              LogicalType = "Result",
                              PushActionType = DataRecordPushRequestTypeEnum.Push
                          })
                          .Call((g, i, ctx) => g.GetPushTargetFromInputAsync(i, ctx))
                          .Return

                          // PUSH in the blackboard the informations
                          .Use<IBlackboardStorageVGrain>().Call((g, i, ctx) => g.PushDataAsync(i, ctx)).Return

                          .Build();

builder.Host.UseDemocriteNode(b =>
{
    b.WizardConfig()
     .NoCluster()
     .ConfigureLogging(c => c.AddConsole())

     // Setup blackboard usage
     .UseBlackboards(b =>
     {
         // A blackboard is a grain that manager shared data and execute controller orders
         // The blackboard state is store in default blackboard storage (BlackboardConstants.BlackboardStorageConfigurationKey) or default one
         b.UseInMemoryStorageForBoardState()
          .UseInMemoryStorageForRecords()
          .UseInMemoryStorageForRegistryState();
     })

     .SetupNodeMemories(m =>
     {
         m.UseInMemory(StorageTypeEnum.All);
     })

    .AddInMemoryDefinitionProvider(m =>
    {
        m.SetupBlackboardTemplates(blackboardTemplate)
         .SetupSequences(sumSequence);
    });
});

var app = builder.Build();

app.UseSwagger();

app.MapGet("/", context =>
{
    context.Response.Redirect("swagger");
    return Task.CompletedTask;
});

app.MapPost("/Api/PushValue/{context}/int", async ([FromServices] IDemocriteExecutionHandler handler, [FromRoute] string context, [FromBody] int value, CancellationToken token) =>
{
    return await handler.VGrain<IStoreVGrain>()
                        .SetConfiguration(context)
                        .SetInput(value)
                        .Call((g, val, ctx) => g.PushNewValueAsync(val, ctx))
                        .RunAsync(token);
});

app.MapPost("/Api/PushValue/{context}/double", async ([FromServices] IDemocriteExecutionHandler handler, [FromRoute] string context, [FromBody] double value, CancellationToken token) =>
{
    return await handler.VGrain<IStoreVGrain>()
                        .SetConfiguration(context)
                        .SetInput(value)
                        .Call((g, val, ctx) => g.PushNewValueAsync(val, ctx))
                        .RunAsync(token);
});

app.MapPost("/Api/PrepareSlot/{context}/{logicalType}", async ([FromServices] IDemocriteExecutionHandler handler, [FromRoute] string context, [FromRoute] string logicalType, CancellationToken token) =>
{
    return await handler.VGrain<IStoreVGrain>()
                        .SetConfiguration(context)
                        .SetInput(logicalType)
                        .Call((g, val, ctx) => g.PrepareSlotAsync(val, ctx))
                        .RunAsync(token);
});

app.MapGet("/Api/GetValues/{context}/MetaData/", async ([FromServices] IDemocriteExecutionHandler handler, [FromRoute] string context, CancellationToken token) =>
{
    return await handler.VGrain<IStoreVGrain>()
                        .SetConfiguration(context)
                        .Call((g, ctx) => g.GetAllMetaDataAsync(ctx))
                        .RunAsync(token);
});

app.MapGet("/Api/GetValues/{context}", async ([FromServices] IDemocriteExecutionHandler handler, [FromRoute] string context, CancellationToken token) =>
{
    return await handler.VGrain<IStoreVGrain>()
                        .SetConfiguration(context)
                        .Call((g, ctx) => g.GetAllValuesAsync(ctx))
                        .RunAsync(token);
});

app.MapPost("/Api/Sum/{context}", async ([FromServices] IDemocriteExecutionHandler handler, [FromRoute] string context, CancellationToken token) =>
{
    return await handler.Sequence<string>(sumSequenceId)
                        .SetInput(context)
                        .RunAsync<bool>(token);
});

app.UseSwaggerUI();

await app.RunAsync();
