// See https://aka.ms/new-console-template for more information

using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions;

using DynamicDefinition.Api;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

var builder = WebApplication.CreateBuilder();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Dynamic definition"
    });

    options.AddEnumsWithValuesFixFilters();

}).AddEndpointsApiExplorer();

builder.Services.AddSingleton<DemoDefinitionGenerator>();

builder.Host.UseDemocriteNode(b =>
{
    b.WizardConfig()
     .NoCluster()

     .UseCronTriggers(supportSecondCron: true)
     .UseSignals()

     .AddDebugTools(LogLevel.Error)

     .ConfigureLogging(c => c.AddConsole());
});

var app = builder.Build();

app.UseSwagger();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.MapPut("/create/sequence", async ([FromServices] DemoDefinitionGenerator generator, string template, [FromQuery] GenerationModeEnum mode = GenerationModeEnum.Normal) =>
{
    return await generator.GenerateAndPushNewAsync(template, mode);
});

app.MapPut("/create/trigger/cron", async ([FromServices] DemoDefinitionGenerator generator, string cron, Guid sequence) =>
{
    return await generator.GenerateTriggerAndPushNewAsync(cron, sequence);
});

app.MapGet("/execute/sequence", async ([FromServices] IDemocriteExecutionHandler handler, Guid sequenceId, CancellationToken token) =>
{
    return await handler.Sequence(sequenceId)
                        .RunWithAnyResultAsync(token);
});

app.MapGet("/definition/sequences", async ([FromServices] ISequenceDefinitionProvider sequenceDefinitionProvider, CancellationToken token) =>
{
    var allSequenceDefinitions = await sequenceDefinitionProvider.GetAllValuesAsync(token);
    return allSequenceDefinitions;
});

app.MapGet("/definition/dynamics", async ([FromServices] IDynamicDefinitionHandler handler, CancellationToken token) =>
{
    var allDefinitions = await handler.GetDynamicDefinitionMetaDatasAsync(token: token);
    return allDefinitions;
});

app.MapGet("/definition/dynamics/ChangeStatus", async ([FromServices] IDynamicDefinitionHandler handler, Guid definitionId, bool enabled, CancellationToken token) =>
{
    return await handler.ChangeStatus(definitionId, enabled, null!, token);
});

app.MapGet("/definition/dynamics/Remove", async ([FromServices] IDynamicDefinitionHandler handler, Guid definitionId, CancellationToken token) =>
{
    return await handler.RemoveDefinitionAsync(definitionId, null!, token);
});

app.MapGet("/definition/dynamics/Clear", async ([FromServices] IDynamicDefinitionHandler handler, CancellationToken token) =>
{
    var allDefinitions = await handler.GetDynamicDefinitionMetaDatasAsync(token: token);
    return await handler.RemoveDefinitionAsync(null!, token, allDefinitions.Info.Select(i => i.Uid).ToArray());
});

app.UseSwaggerUI();

app.Run();
