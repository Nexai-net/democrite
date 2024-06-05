// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Extensions;
    using Democrite.Framework.Core.Models;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Node.ThreadExecutors;
    using Democrite.Framework.Node.UnitTests.Extensions;
    using Democrite.Test.Interfaces;
    using Democrite.Test.VGrains;
    using Democrite.UnitTests.ToolKit;
    using Democrite.UnitTests.ToolKit.Remoting;
    using Democrite.UnitTests.ToolKit.Sequences;
    using Democrite.UnitTests.ToolKit.Services;
    using Democrite.UnitTests.ToolKit.VGrains.Transformers;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NFluent;

    using NSubstitute;
    using NSubstitute.Core;

    using Orleans.Runtime;
    using Orleans.TestingHost;

    using Xunit.Abstractions;

    /// <summary>
    /// Test <see cref="SequenceExecutorVGrain"/>
    /// </summary>
    public sealed class SequenceExecutorUnitTest
    {
        #region Fields

        private const string EMAIL_A = "mickael@nexai.net";
        private const string EMAIL_B = "jerome@nexai.net";
        private const string EMAIL_SAMPLE_TEST = "An email have been send to " + EMAIL_A + " - " + EMAIL_B + " ---";

        private readonly ITestOutputHelper _testOutputHelper;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorUnitTest"/> class.
        /// </summary>
        public SequenceExecutorUnitTest(ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Simple test run one vgrain, manual call input, manual instantiate
        /// </summary>
        [Fact]
        public async Task Manual_Full_Basic_Run()
        {
            var def = Sequence.Build("Baisc")
                              .RequiredInput<string>()
                              .Use<ITestExtractEmailTransformer>().Call((a, input, ctx) => a.ExtractEmailsAsync(input, ctx)).Return
                              .Build();

            var rootLogger = new MemoryTestLogger<SequenceExecutorVGrain>();

            var sequenceProvider = new Mock<ISequenceDefinitionProvider>(MockBehavior.Strict);
            var vgrainProvider = Substitute.For<IVGrainProvider>();
            var logFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var objectConvertMock = new Mock<IObjectConverter>(MockBehavior.Strict);
            var democriteSerializerMock = Substitute.For<IDemocriteSerializer>();
            var sequenceVGrainProviderFactoryMock = Substitute.For<ISequenceVGrainProviderFactory>();
            var sequenceVGrainProviderMock = Substitute.For<ISequenceVGrainProvider>();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IDemocriteSerializer>(democriteSerializerMock);

            var threadStageExecutorProviderMock = (ISequenceExecutorThreadStageProvider)SequenceExecutorThreadStageProviderMock.Create(serviceCollection);
            var timeManager = new TimeManager();

            var executionLog = new TestDiagnosticLogConsumer();
            var diagnosticLogger = new Core.Diagnostics.DiagnosticLogger(new[] { executionLog }, null);

            var execCtx = new ExecutionContextWithConfiguration<Guid>(Guid.NewGuid(), Guid.NewGuid(), null, def.Uid);
            var ctxState = new SequenceExecutorStateSurrogate()
            {
                FlowUid = execCtx.FlowUID,
                SequenceDefinitionId = execCtx.Configuration,
                InstanceId = execCtx.CurrentExecutionId
            };

            var stateStack = new Stack<SequenceExecutorStateSurrogate>();

            var ctx = new Mock<IGrainContext>(MockBehavior.Strict);

            var persistentState = new Mock<IPersistentState<SequenceExecutorStateSurrogate>>(MockBehavior.Strict);
            persistentState.Setup(s => s.ReadStateAsync()).Returns(Task.CompletedTask);

            persistentState.SetupProperty(s => s.State, ctxState);

            var etag = Guid.NewGuid().ToString().Replace("-", "");
            persistentState.SetupGet(s => s.Etag).Returns(etag);

            persistentState.Setup(s => s.WriteStateAsync())
                           .Returns(() =>
                           {
                               stateStack.Push(persistentState.Object.State);
                               return Task.CompletedTask;
                           });

            logFactory.Setup(l => l.CreateLogger(It.IsAny<string>()))
                       .Returns((string name) => rootLogger.CreateChild(name));

            sequenceProvider.Setup(w => w.GetFirstValueByIdAsync(def.Uid, It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult<SequenceDefinition?>(def));

            sequenceVGrainProviderFactoryMock.GetProvider(Arg.Any<ExecutionCustomizationDescriptions?>())
                                             .Returns(new DisposableStructContainer<ISequenceVGrainProvider>(sequenceVGrainProviderMock));

            sequenceVGrainProviderMock.GetGrainProvider(ref Arg.Any<ISequenceStageDefinition>())
                                      .Returns(vgrainProvider);

            vgrainProvider.GetVGrainAsync(Arg.Any<Type>(), Arg.Any<object?>(), Arg.Any<IExecutionContext>(), Arg.Any<ILogger>())
                          //.Returns((Type type, object? input, IExecutionContext ctxVGrain, ILogger logger) =>
                          .Returns((CallInfo info) => 
                          {
                              var type = info.Arg<Type>();
                              var ctxVGrain = info.Arg<IExecutionContext>();

                              Check.That(type).IsEqualTo(typeof(ITestExtractEmailTransformer));
                              Check.That(ctxVGrain).IsNotNull();
                          
                              var transformer = new TestExtractEmailTransformer(rootLogger.CreateChild<TestExtractEmailTransformer>());
                              return ValueTask.FromResult<IVGrain>(transformer);
                          });

            var executor = new SequenceExecutorVGrain(rootLogger,
                                                      logFactory.Object,
                                                      diagnosticLogger,
                                                      sequenceProvider.Object,
                                                      persistentState.Object,
                                                      timeManager,
                                                      objectConvertMock.Object,
                                                      democriteSerializerMock,
                                                      threadStageExecutorProviderMock,
                                                      sequenceVGrainProviderFactoryMock,
                                                      Substitute.For<ISignalService>(),
                                                      Substitute.For<IVGrainDemocriteSystemProvider>());

            var result = await executor.RunAsync<string[], string>(EMAIL_SAMPLE_TEST, execCtx);

            Check.That(result).IsNotNull().And.ContainsExactly(EMAIL_A, EMAIL_B).And.CountIs(2);

            sequenceProvider.Verify(w => w.GetFirstValueByIdAsync(def.Uid, It.IsAny<CancellationToken>()), Times.Exactly(2));
            await vgrainProvider.Received(1).GetVGrainAsync(Arg.Any<Type>(), Arg.Any<object?>(), Arg.Any<IExecutionContext>(), Arg.Any<ILogger>());

            Check.That(stateStack).IsNotNull().And.CountIs(2);

            var allLogs = rootLogger.Flattern().ToArray();

            Check.That(allLogs).IsNotNull().And.Not.HasElementThatMatches(t => t.logLevel == LogLevel.Critical || t.logLevel == LogLevel.Error);
            Check.That(allLogs.Length).IsStrictlyGreaterThan(0);
        }

        /// <summary>
        /// Simple test run one vgrain, manual call input, using orleans <see cref="TestClusterBuilder"/>
        /// </summary>
        [Fact(Timeout = 200_000)]
        public async Task TestCluster_Cluster_Basic_Run()
        {
            var def = Sequence.Build("Baisc")
                              .RequiredInput<string>()
                              .Use<ITestExtractEmailTransformer>().Call((a, input, ctx) => a.ExtractEmailsAsync(input, ctx)).Return
                              .Build();

            var timeout = CancellationHelper.Timeout(TimeSpan.FromSeconds(200_000));
            var flows = await TestClusterProcess(def,
                                                 client => client.Sequence<string>(def.Uid).SetInput(EMAIL_SAMPLE_TEST).RunAsync<string[]>(),
                                                 result => Check.That(result?.Output).IsNotNull().And.ContainsExactly(EMAIL_A, EMAIL_B).And.CountIs(2),
                                                 timeout);

            // Check Flow
            Check.That(flows).IsNotNull().And.CountIs(1);

            var flow = flows.First();

            Check.That(flow).IsNotNull();
            Check.That(flow.Roots).IsNotNull().And.CountIs(1);
            Check.That(flow.Stages).IsNotNull().And.CountIs(2);

            // Check root
            var rootExec = flow.Roots.First();

            Check.That(rootExec).IsNotNull();

            Check.That(rootExec.Call).IsNotNull();
            Check.That(rootExec!.Call!.TargetVGrainType).IsNotNull().And.IsEqualTo(typeof(IGenericContextedExecutor<Guid>).AssemblyQualifiedName);

            Check.That(rootExec.Parameters).IsNotNull();
            Check.That(rootExec.Parameters!.Error).IsNullOrEmpty();

            var parameterFlattern = rootExec.Parameters.InOut?.Flattern()?.ToArray() ?? EnumerableHelper<object>.ReadOnlyArray;

            Check.That(parameterFlattern).IsNotNull().And.CountIs(2).And.ContainsExactly(new string?[] { EMAIL_SAMPLE_TEST, null });

            Check.That(rootExec.Children).IsNotNull().And.CountIs(1);

            // Check first call
            var child = rootExec.Children.First();
            Check.That(child).IsNotNull();
            Check.That(child.Parent).IsNotNull().And.IsSameReferenceAs(rootExec);

            Check.That(child.Parameters).IsNotNull();
            Check.That(child.Parameters!.Error).IsNullOrEmpty();

            var childParameterFlattern = child.Parameters.InOut?.GetTreeValues(v => v?.Next)?.ToArray() ?? EnumerableHelper<TypedArgument>.ReadOnlyArray;

            Check.That(childParameterFlattern).IsNotNull()
                                              .And
                                              .CountIs(2)
                                              .And
                                              .ContainsOnlyElementsThatMatch(t => (t is TypedArgument<string> tStr && tStr.Value.Equals(EMAIL_SAMPLE_TEST)) ||
                                                                                  t is TypedArgument<IExecutionContext> ||
                                                                                  t is TypedArgument<ExecutionContext>);

            Check.That(child.ReturnArg).IsNotNull();
            Check.That(child.ReturnArg!.Error).IsNull();

            var returnArg = child.ReturnArg.InOut?.Flattern()?.ToArray() ?? EnumerableHelper<object>.ReadOnlyArray;
            Check.That(returnArg).IsNotNull().And.CountIs(1);

            Check.That(returnArg.First() as IReadOnlyCollection<string>).IsNotNull().And.ContainsExactly(EMAIL_A, EMAIL_B);

            Check.That(flows.Single()).FollowDefinition(def);
        }

        /// <summary>
        /// Test foreach run one vgrain, manual call input, using orleans <see cref="TestClusterBuilder"/>
        /// </summary>
        [Fact(Timeout = 200_000, Skip = "Instable on CI/CD execute")]
        public async Task TestCluster_Cluster_Foreach_Run()
        {
            var getHtmlStageUid = Guid.NewGuid();
            var extractTagStageUid = Guid.NewGuid();
            var tagQualiferStageUid = Guid.NewGuid();

            var def = Sequence.Build("Foreach")
                              .NoInput()
                              .Use<IHtmlProviderTestTransformer>(cfg => cfg.Uid(getHtmlStageUid))
                                                                       .Call((a, ctx) => a.GetHtmlAsync(ctx))
                                                                       .Return

                              .Use<ITagExtractorTestTransformer>(cfg => cfg.Uid(extractTagStageUid))
                                                                        .Call((a, html, ctx) => a.ExtractTagFromHtmlAsync(html, ctx))
                                                                        .Return

                              .Foreach(IType.From<Tag>(), each =>
                              {
                                  return each.Use<ITagQualifierTestTransformer>(cfg => cfg.Uid(tagQualiferStageUid))
                                             .Call((a, tag, ctx) => a.QualifyTagAsync(tag!, ctx))
                                             .Return;
                              })
                              .Build();

            TagQualify[]? flowResult = null;

            var timeout = CancellationHelper.Timeout(TimeSpan.FromSeconds(200_000));
            var flows = await TestClusterProcess(def,
                                                 //executor => executor.RunAsync<TagQualify[]>(null), // Create test Execution context,
                                                 client => client.Sequence(def.Uid).RunAsync<TagQualify[]>(),
                                                 result =>
                                                 {
                                                     Check.That(result?.Output).IsNotNull().And.CountIs(4);
                                                     flowResult = result!.Output!;
                                                 },
                                                 timeout);

            // Still failed to have the issues on link issue
            Check.That(flows).IsNotNull().And.CountIs(1);

            var tagQualifiersFromLogs = new List<TagQualify>();

            Check.That(flows.Single())
                 .FollowDefinition(def, new Dictionary<Guid, StageValidator>()
                 {
                     [getHtmlStageUid] = (indx, stage, def) =>
                     {
                         var input = (string)stage.ReturnArg?.InOut?.Flattern().First()!;
                         Check.That(input).IsNotNull().And.IsNotEmpty().And.IsEqualTo(HtmlProviderTestTransformer.SampleHtml);
                     },

                     [extractTagStageUid] = (indx, stage, def) =>
                     {
                         var inputs = stage.Parameters?.InOut?.Flattern();
                         Check.That(inputs).IsNotNull().And.Contains(HtmlProviderTestTransformer.SampleHtml);

                         var tags = (Tag[])stage.ReturnArg?.InOut?.Flattern().First()!;
                         Check.That(tags).IsNotNull().And.CountIs(4);
                     },

                     [tagQualiferStageUid] = (indx, stage, def) =>
                     {
                         var tagReturned = (stage.ReturnArg?.InOut?.Flattern() ?? EnumerableHelper<object>.ReadOnly).SingleOrDefault();

                         Check.That(tagReturned).IsNotNull().And.IsInstanceOf<TagQualify>();

                         tagQualifiersFromLogs.Add((TagQualify)tagReturned!);
                     }
                 });

            Check.That(flowResult).IsNotNull();
            Check.That(tagQualifiersFromLogs).CountIs(flowResult!.Length)
                                             .And
                                             .ContainsNoDuplicateItem()
                                             .And
                                             .ContainsOnlyElementsThatMatch(fl => flowResult.Contains(fl, TagQualifyComparer.Instance));

        }

        /// <summary>
        /// Run <paramref name="def"/> through <see cref="TestCluster" />
        /// </summary>
        private async Task<IReadOnlyCollection<FlowValidator>> TestClusterProcess<TResult>(SequenceDefinition def,
                                                                                           Func<IDemocriteExecutionHandler, Task<TResult>> exec,
                                                                                           Action<TResult> resultValidator,
                                                                                           CancellationToken token)
            where TResult : IExecutionResult
        {
            var defManager = new Mock<ISequenceDefinitionProvider>();
            defManager.Setup(s => s.GetFirstValueByIdAsync(def.Uid, It.IsAny<CancellationToken>())).ReturnsAsync(def);

            var executionLog = new TestDiagnosticLogConsumer();

            ITestRemotingService? controller = null;

            var builder = new TestClusterBuilder();

            builder.AddRemoteMockService(remote =>
            {
                remote.AddSingleton(defManager.Object);
                remote.AddSingleton(Substitute.For<ISignalDefinitionProvider>());
                remote.AddSingleton<IDiagnosticLogConsumer>(executionLog);

                controller = remote.Build();
            });

            var localServiceBuilder = new ServiceCollection();
            DemocriteCoreServicesExtensions.SetupCoreServices(localServiceBuilder);

            var localServices = localServiceBuilder.BuildServiceProvider();

            builder.AddSiloBuilderConfigurator<DemocriteTestClusterServicesConfiguration>();

            var cluster = builder.Build();
            ArgumentNullException.ThrowIfNull(controller);
            using (controller)
            {
                await controller.StartRemotingListnerAsync();

                await cluster.DeployAsync();

                var client = new DemocriteExecutionHandler(new VGrainProvider(cluster.GrainFactory, localServices.GetRequiredService<IVGrainIdFactory>()),
                                                           Substitute.For<IDemocriteSerializer>(),
                                                           Substitute.For<IDeferredAwaiterHandler>());

                var result = await exec(client);

                Check.That(result).IsNotEqualTo(default);
                Check.That(result.Cancelled).IsFalse();
                Check.WithCustomMessage(string.Format("[ErrorCode:{0}] Message : {1}", result.ErrorCode, result.Message)).That(result.Succeeded).IsTrue();

                if (!string.IsNullOrEmpty(result.Message))
                    this._testOutputHelper.WriteLine("Extract Info : " + result.Message);

                resultValidator(result);

                await executionLog.FlushAsync(token);
            }

            return FlowValidator.From(executionLog.Logs);
        }

        #endregion
    }
}
