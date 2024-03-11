// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.ThreadExecutors;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;
    using Democrite.UnitTests.ToolKit;
    using Democrite.UnitTests.ToolKit.VGrains.Transformers;

    using Microsoft.Extensions.Logging;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Test <see cref="SequenceExecutorThreadStageCall"/> responsable to call vgrain method during flow resolution
    /// </summary>
    public sealed class SequenceExecutorCallThreadStageProviderUnitTests
    {
        /// <summary>
        /// Test <see cref="SequenceExecutorThreadStageCall.ExecAsync"/> with valid value
        /// </summary>
        [Fact]
        public async Task SequenceExecutorCallThreadStageProvider_ExecAsync()
        {
            using (var handler = new SequenceExecutorThreadStageCall())
            {
                var def = typeof(ITestExtractEmailTransformer).GetMethod(nameof(ITestExtractEmailTransformer.ExtractEmailsAsync))!.GetAbstractMethod();

                var callDefinition = new SequenceStageCallDefinition(typeof(string).GetAbstractType(),
                                                                     (ConcretType)typeof(ITestExtractEmailTransformer).GetAbstractType(),
                                                                     def,
                                                                     typeof(string[]).GetAbstractType(),
                                                                     null);

                // Get Handler

                var execContext = new Democrite.Framework.Core.Models.ExecutionContext(Guid.NewGuid(), Guid.NewGuid(), null);
                var logger = new MemoryTestLogger();
                var input = Guid.NewGuid().ToString();
                var expectedResults = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

                Expression<Predicate<IExecutionContext>> execContextValidator = (IExecutionContext c) => c != null &&
                                                                                                         execContext.FlowUID == c.FlowUID &&
                                                                                                         execContext.CurrentExecutionId == c.CurrentExecutionId &&
                                                                                                         execContext.ParentExecutionId == c.ParentExecutionId;

                var mockVGrain = Substitute.For<ITestExtractEmailTransformer>(); //new Mock<ITestExtractEmailTransformer>(MockBehavior.Strict);
                mockVGrain.ExtractEmailsAsync(input, Arg.Is(execContextValidator)).Returns(Task.FromResult(expectedResults));

                var mockDiagnositicLogger = Substitute.For<IDiagnosticLogger>(); //new Mock<IDiagnosticLogger>(MockBehavior.Strict);

                var mockVGrainProvider = Substitute.For<IVGrainProvider>(); //new Mock<IVGrainProvider>(MockBehavior.Strict);
                mockVGrainProvider.GetVGrainAsync(callDefinition.VGrainType.ToType(), input, Arg.Is(execContextValidator), Arg.Any<ILogger>()).Returns(ValueTask.FromResult<IVGrain>(mockVGrain));

                // ACT
                var result = await handler.ExecAsync(callDefinition,
                                                     input,
                                                     execContext,
                                                     logger,
                                                     mockDiagnositicLogger,
                                                     mockVGrainProvider,
                                                     () => throw new InvalidOperationException("No need to get secure token"));

                // Check

                Check.That(result.result).IsNotNull();
                Check.That(result.expectedResultType).IsNotNull().And.IsEqualTo(typeof(string[])).And.IsEqualTo(callDefinition.Output);

                await result.result;

                //mockVGrainProvider.Verify(m => m.GetVGrainAsync(callDefinition.VGrainType.ToType(), input, It.Is(execContextValidator), It.IsAny<ILogger>()), Times.Once);
                await mockVGrainProvider.Received(1).GetVGrainAsync(callDefinition.VGrainType.ToType(), input, Arg.Is(execContextValidator), Arg.Any<ILogger>());
                await mockVGrain.Received(1).ExtractEmailsAsync(input, Arg.Any<IExecutionContext>());

                mockVGrainProvider.DidNotReceive();
                mockVGrain.DidNotReceive();

                /* No Manuel diagnostic call allowed */
                mockDiagnositicLogger.DidNotReceive();

                Check.That(result.result).IsInstanceOf<Task<string[]>>()
                                         .Which.IsNotNull();

                var strResults = result.result.GetResult<string[]>();

                Check.That(strResults).IsNotNull()
                                      .And
                                      .CountIs(2)
                                      .And
                                      .ContainsOnlyElementsThatMatch(s => expectedResults.Contains(s));
            }
        }
    }
}
