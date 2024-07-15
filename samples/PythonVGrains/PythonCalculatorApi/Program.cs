namespace Python
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Builders.Artifacts;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OpenApi.Models;

    public static class Program
    {
        [GenerateSerializer]
        public record ComplexResult(string formula, double result);

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Democrite sample",
                    Version = "v1"
                });
            }).AddEndpointsApiExplorer();

            // Slim python config
            var oneShotDef = await Artifact.VGrain("OneShotCalc", uid: new Guid("1B7331F0-3121-43B8-87F7-9B22226D24F8"))
                                           .Python("./Scripts/OneShot", "Calculator.py")
                                           .CompileAsync();

            // Python config with helper
            var deamonDef = await Artifact.VGrain("demonCalc", uid: new Guid("EA23C491-D708-4BF4-88E3-3D95118C10D5"))
                                          .Python()
                                          .Directory("./Scripts")
                                          .ExecuteFile("Deamon/Calculator.py")
                                          .Persistent()
                                          .CompileAsync();

            // Python config with helper and without Directory helper
            var complexDef = await Artifact.VGrain("demonCalc", uid: new Guid("6E24D646-2A9F-4467-942D-64DD4CE53770"))
                                           .Python()
                                           .From("./Scripts", ArtifactPackageTypeEnum.Directory, (string?)null)
                                           .ExecuteFile("Complex/ComplexCompute.py")
                                           .Persistent()
                                           .CompileAsync();

            // Python config withouts helper
            var noResultDef = await Artifact.VGrain("demonCalc", uid: new Guid("3D7B54EE-8A77-42B5-AE84-C7CD091A0AF2"))
                                            .ExecuteBy("Python", new Version("3.12.1"))
                                            .From("./Scripts", ArtifactPackageTypeEnum.Directory, (string?)null)
                                            .ExecuteFile("NoResult/NoResultCompute.py")
                                            .CompileAsync();

            builder.Host.UseDemocriteNode(b =>
                                          {
                                              b.WizardConfig()
                                               .NoCluster()
                                               .ConfigureLogging(c => c.AddConsole())
                                               .AddInMemoryDefinitionProvider(p =>
                                               {
                                                   p.SetupArtifacts(oneShotDef, deamonDef, complexDef, noResultDef);
                                               });
                                          });

            var app = builder.Build();

            app.UseSwagger();

            app.MapGet("/", request =>
            {
                request.Response.Redirect("swagger");
                return Task.CompletedTask;
            });

            app.MapGet("/calc", ([FromServices] IDemocriteExecutionHandler handler, [FromQuery] string operation) =>
            {
                return handler.VGrain<IGenericArtifactExecutableVGrain>()
                              .SetInput(operation)
                              .SetConfiguration(oneShotDef.Uid)  // Call one shot script
                              .Call((g, i, ctx) => g.RunAsync<double, string>(i, ctx))
                              .RunAsync();
            });

            app.MapGet("/calc/dontuseresult", ([FromServices] IDemocriteExecutionHandler handler, [FromQuery] string operation) =>
            {
                return handler.VGrain<IGenericArtifactExecutableVGrain>()
                              .SetInput(operation)
                              .SetConfiguration(oneShotDef.Uid)  // Call one shot script
                              .Call((g, i, ctx) => g.RunWithInputAsync<string>(i, ctx))
                              .RunAsync();
            });

            app.MapGet("/calc/deamon", ([FromServices] IDemocriteExecutionHandler handler, [FromQuery] string operation) =>
            {
                return handler.VGrain<IGenericArtifactExecutableVGrain>()
                              .SetInput(operation)
                              .SetConfiguration(deamonDef.Uid) // Call persistant script
                              .Call((g, i, ctx) => g.RunAsync<double, string>(i, ctx))
                              .RunAsync();
            });

            app.MapGet("/calc/complex", ([FromServices] IDemocriteExecutionHandler handler, [FromQuery] string operation) =>
            {
                return handler.VGrain<IGenericArtifactExecutableVGrain>()
                              .SetInput(operation)
                              .SetConfiguration(complexDef.Uid) // Call persistant complexDef script
                              .Call((g, i, ctx) => g.RunAsync<ComplexResult, string>(i, ctx))
                              .RunAsync();
            });

            app.MapGet("/calc/noresult", ([FromServices] IDemocriteExecutionHandler handler, [FromQuery] string operation) =>
            {
                return handler.VGrain<IGenericArtifactExecutableVGrain>()
                              .SetInput(operation)
                              .SetConfiguration(noResultDef.Uid) // Call persistant noResultDef script
                              .Call((g, i, ctx) => g.RunWithInputAsync(i, ctx))
                              .RunAsync();
            });

            app.UseSwaggerUI();
            app.Run();
        }
    }
}