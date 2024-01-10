// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

using Common;

using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

builder.Configuration.AddJsonFile("appsettings.json");

builder.Host.UseDemocriteClient(b =>
                                {
                                    b.WizardConfig()
                                     .UseMongoCluster()
                                     .ConfigureLogging(c => c.AddConsole());
                                });

var app = builder.Build();

app.UseSwagger();

app.MapGet("/{counter}", ([FromServices] IDemocriteExecutionHandler handler, [FromRoute] string counter, CancellationToken token) =>
{
    return handler.VGrain<ICounterVGrain>()
                  .SetInput(counter)
                  .Call((g, i, ctx) => g.GetValueAsync(i, ctx))
                  .RunAsync(token);
});

app.UseSwaggerUI();

app.Run();
