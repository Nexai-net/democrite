// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.ThreadExecutors;
    using Elvex.Toolbox.Abstractions.Disposables;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Models;
    using Democrite.UnitTests.ToolKit;
    using Democrite.UnitTests.ToolKit.VGrains.Transformers;

    using Moq;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Diagnostics;
    using Democrite.Framework.Core;

    /// <summary>
    /// Test <see cref="SequenceExecutorThreadStageForeach"/> responsable to call vgrain method during flow resolution
    /// </summary>
    public sealed class SequenceExecutorForeachThreadStageProviderUnitTests
    {
        /// <summary>
        /// Test <see cref="SequenceExecutorThreadStageForeach.ExecAsync"/> with valid value
        /// </summary>
        [Fact]
        public async Task SequenceExecutorForeachThreadStageProvider_ExecAsync()
        {
            var rand = new Random();

            // Prepare
            var foreachExecutor = new SequenceExecutorThreadStageForeach();

            var def = typeof(ITestExtractEmailTransformer).GetMethod(nameof(ITestExtractEmailTransformer.ExtractEmailsAsync))!.GetAbstractMethod();

            var callDefinition = new SequenceStageCallDefinition(Guid.NewGuid(),
                                                                 "Root",
                                                                 typeof(string).GetAbstractType(),
                                                                 (ConcretType)typeof(ITestExtractEmailTransformer).GetAbstractType(),
                                                                 def,
                                                                 typeof(string[]).GetAbstractType(),
                                                                 null,
                                                                 null,
                                                                 null,
                                                                 null);

            var innerDef = new SequenceDefinition(Guid.NewGuid(),
                                                  RefIdHelper.Generate(Core.Abstractions.Enums.RefTypeEnum.Sequence, "sequence-test-foreach", "unit.tests"),
                                                  "test",
                                                  SequenceOptionDefinition.Default,
                                                  new[] { callDefinition },
                                                  null);

            var foreachDefinition = new SequenceStageForeachDefinition(Guid.NewGuid(),
                                                                       "Foreach Test",
                                                                       typeof(string[]).GetAbstractType(),
                                                                       innerDef,
                                                                       typeof(string).GetAbstractType(),
                                                                       typeof(string[]).GetAbstractType(),
                                                                       null,
                                                                       null,
                                                                       null);

            var execContext = new Democrite.Framework.Core.Models.ExecutionContext(Guid.NewGuid(), Guid.NewGuid(), null);
            var logger = new MemoryTestLogger();

            var inputs = Enumerable.Range(0, rand.Next(5, 15))
                                   .Select(_ => Guid.NewGuid().ToString())
                                   .ToArray();

            var resultInputs = inputs.ToDictionary(k => k, _ => Guid.NewGuid().ToString());

            IReadOnlyCollection<ISequenceExecutorExecThread>? testInnerThreads = null;
            var testMockInnerThreads = new List<Mock<ISequenceExecutorExecThread>>();

            var diagnosticLogs = new List<IExecutionContextChangeDiagnosticLog>();

            var mockDiagnositicLogger = new Mock<IDiagnosticLogger>(MockBehavior.Strict);
            var mockVGrain = new Mock<ITestExtractEmailTransformer>(MockBehavior.Strict);
            var mockVGrainProvider = new Mock<IVGrainProvider>(MockBehavior.Strict);
            var democriteSerializerMock = Substitute.For<IDemocriteSerializer>();
            var timeManagerMock = Substitute.For<ITimeManager>();

            var mockSecureToken = new Mock<ISecureContextToken<ISequenceExecutorThreadHandler>>(MockBehavior.Strict);
            var mockThreadHandler = new Mock<ISequenceExecutorThreadHandler>(MockBehavior.Strict);

            mockSecureToken.SetupGet(e => e.Token).Returns(mockThreadHandler.Object);
            mockSecureToken.Setup(m => m.Dispose());

            Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>? postCallback = null;

            // Diagnositic Logger

            mockDiagnositicLogger.Setup(m => m.Log(It.IsAny<IExecutionContextChangeDiagnosticLog>()))
                                 .Callback<IExecutionContextChangeDiagnosticLog>((log) => diagnosticLogs.Add(log));

            // Secure mockInnerThread Tken Mock
            mockThreadHandler.Setup(m => m.RegisterPostProcess(It.IsAny<Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>>()))
                             .Callback<Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>>(postAction => postCallback = postAction);

            mockThreadHandler.Setup(m => m.CreateInnerThread(It.IsAny<SequenceExecutorExecThreadState>(), innerDef, It.IsAny<IExecutionContext>()))
                             .Returns<SequenceExecutorExecThreadState, SequenceDefinition, IExecutionContext>((state, def, sourceCtx) =>
                             {
                                 var mockInnerThread = new Mock<ISequenceExecutorExecThread>(MockBehavior.Strict);
                                 var innerCtx = new Democrite.Framework.Core.Models.ExecutionContext(state.FlowUid, state.CurrentStageExecId, state.ParentStageExecId);

                                 mockInnerThread.SetupGet(m => m.ExecutionContext).Returns(innerCtx);

                                 testMockInnerThreads.Add(mockInnerThread);

                                 return mockInnerThread.Object;
                             });

            mockThreadHandler.SetupGet(p => p.HasInnerThreads).Returns(false);

            mockThreadHandler.Setup(m => m.SetInnerThreads(It.IsAny<IReadOnlyCollection<ISequenceExecutorExecThread>>()))
                             .Callback<IReadOnlyCollection<ISequenceExecutorExecThread>>(threads =>
                             {
                                 Check.That(threads).IsNotNull().And.CountIs(inputs.Length);
                                 testInnerThreads = threads;
                             });

            var state = new SequenceExecutorExecThreadState(execContext.FlowUID,
                                                            Guid.NewGuid(),
                                                            execContext.CurrentExecutionId,
                                                            execContext.ParentExecutionId,
                                                            null,
                                                            inputs);

            mockThreadHandler.Setup(p => p.GetCurrentDoneThreadState()).Returns(state);

            // Act
            //var foreachExecutor = foreachExecutor.Provide(foreachDefinition);

            Check.That(foreachExecutor).IsNotNull().And.IsSameReferenceAs(foreachExecutor);

            var result = await foreachExecutor.ExecAsync(foreachDefinition,
                                                         inputs,
                                                         execContext,
                                                         logger,
                                                         mockDiagnositicLogger.Object,
                                                         mockVGrainProvider.Object,
                                                         () => mockSecureToken.Object);

            Check.That(result.expectedResultType).IsNull();
            Check.That(result.result).IsNotNull().And.WhichMember(r => r.IsCompleted).Verifies(c => c.IsTrue());

            await result.result;

            // Check

            mockThreadHandler.Verify(m => m.RegisterPostProcess(It.IsAny<Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>>()), Times.Once);
            mockThreadHandler.Verify(m => m.CreateInnerThread(It.IsAny<SequenceExecutorExecThreadState>(), innerDef, It.IsAny<IExecutionContext>()), Times.Exactly(inputs.Length));
            mockThreadHandler.Verify(m => m.SetInnerThreads(It.IsAny<IReadOnlyCollection<ISequenceExecutorExecThread>>()), Times.Once);

            Check.That(testInnerThreads).IsNotNull().And.CountIs(inputs.Length);

            mockThreadHandler.Invocations.Clear();

            Check.That(postCallback).IsNotNull();

            // Change Test Values 

            mockThreadHandler.SetupGet(p => p.HasInnerThreads).Returns(true);
            mockThreadHandler.SetupGet(p => p.AllInnerThreadsJobDone).Returns(true);

            Debug.Assert(testInnerThreads != null);
            mockThreadHandler.Setup(m => m.PullInnerThreads(true)).Returns(testInnerThreads);

            foreach (var inner in testMockInnerThreads)
            {
                inner.Setup(i => i.GetSecurityThreadHandler())
                     .Returns(() =>
                     {
                         var innerMockSecureToken = new Mock<ISecureContextToken<ISequenceExecutorThreadHandler>>(MockBehavior.Strict);
                         var innerThreadHandler = new Mock<ISequenceExecutorThreadHandler>(MockBehavior.Strict);

                         innerMockSecureToken.SetupGet(i => i.Token).Returns(innerThreadHandler.Object);

                         var innerState = new SequenceExecutorExecThreadState(inner.Object.ExecutionContext.FlowUID,
                                                                              Guid.NewGuid(),
                                                                              inner.Object.ExecutionContext.CurrentExecutionId,
                                                                              inner.Object.ExecutionContext.ParentExecutionId,
                                                                              null,
                                                                              null);

                         innerState.SetJobIsDone(new[] { Guid.NewGuid().ToString() });

                         innerThreadHandler.Setup(m => m.GetCurrentDoneThreadState())
                                           .Returns(innerState);

                         innerMockSecureToken.Setup(m => m.Dispose());

                         return innerMockSecureToken.Object;
                     });
            }

            // Act post process

            var postbackResults = await postCallback!(foreachDefinition, () => mockSecureToken.Object);

            // Check
            Check.That(postbackResults.expectedResultType).IsNotNull().And.IsEqualTo(typeof(string[][]));
            Check.That(postbackResults.result).IsNotNull().And.IsInstanceOfType(typeof(Task<string[][]>));

            var tsk = (Task<string[][]>)postbackResults.result;

            var loopResult = await tsk;

            Check.That(loopResult).IsNotNull()
                                  .And
                                  .CountIs(inputs.Length)
                                  .And
                                  .ContainsOnlyElementsThatMatch(r => r.Length == 1 &&
                                                                      !string.IsNullOrEmpty(r.First()) &&
                                                                      Guid.TryParse(r.First(), out _));
        }
    }
}
