// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.ThreadExecutors;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Models;
    using Democrite.UnitTests.ToolKit;
    using Democrite.UnitTests.ToolKit.VGrains.Transformers;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NFluent;

    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Test <see cref="SequenceExecutorCallThreadStageProvider"/> responsable to call vgrain method during flow resolution
    /// </summary>
    public sealed class SequenceExecutorCallThreadStageProviderUnitTests
    {
        /// <summary>
        /// Test <see cref="SequenceExecutorCallThreadStageProvider"/> constructor and dispose action
        /// </summary>
        [Fact]
        public void SequenceExecutorCallThreadStageProvider_Ctor_Dispose()
        {
            using (var provider = new SequenceExecutorCallThreadStageProvider())
            {

            }
        }

        /// <summary>
        /// Test <see cref="SequenceExecutorCallThreadStageProvider.CanHandler"/>
        /// </summary>
        [Fact]
        public void SequenceExecutorCallThreadStageProvider_CanHandler()
        {
            using (var provider = new SequenceExecutorCallThreadStageProvider())
            {
                Check.ThatCode(() => provider.CanHandler(null)).DoesNotThrow().And.WhichResult().IsFalse();

                var mockBaseSequenceStage = new Mock<ISequenceStageDefinition>();
                Check.ThatCode(() => provider.CanHandler(mockBaseSequenceStage.Object)).DoesNotThrow().And.WhichResult().IsFalse();

                var def = typeof(SequenceExecutorCallThreadStageProviderUnitTests).GetMethod(nameof(SequenceExecutorCallThreadStageProvider_CanHandler))!.GetAbstractMethod();

                // True
                Check.ThatCode(() => provider.CanHandler(new SequenceStageCallDefinition(null,
                                                                                         (ConcreteType)typeof(string).GetAbstractType(),
                                                                                         def,
                                                                                         null,
                                                                                         null)))
                     .DoesNotThrow()
                     .And
                     .WhichResult()
                     .IsTrue();
            }
        }

        /// <summary>
        /// Test <see cref="SequenceExecutorCallThreadStageProvider.ExecAsync"/> with valid value
        /// </summary>
        [Fact]
        public async Task SequenceExecutorCallThreadStageProvider_ExecAsync()
        {
            using (var provider = new SequenceExecutorCallThreadStageProvider())
            {
                var def = typeof(ITestExtractEmailTransformer).GetMethod(nameof(ITestExtractEmailTransformer.ExtractEmailsAsync))!.GetAbstractMethod();

                var callDefinition = new SequenceStageCallDefinition(typeof(string).GetAbstractType(),
                                                                     (ConcreteType)typeof(ITestExtractEmailTransformer).GetAbstractType(),
                                                                     def,
                                                                     typeof(string[]).GetAbstractType());

                // Get Handler
                var handler = provider.Provide(callDefinition);

                var handlerFromNull = provider.Provide(null);

                /* No need to create a new Handler. 'Call' algorithme is state less */
                Check.That(handler).IsNotNull().And.IsSameReferenceAs(provider);

                Check.That(handlerFromNull).IsNotNull().And.IsSameReferenceAs(provider);

                var execContext = new Democrite.Framework.Core.Models.ExecutionContext(Guid.NewGuid(), Guid.NewGuid(), null);
                var logger = new MemoryTestLogger();
                var input = Guid.NewGuid().ToString();
                var expectedResults = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

                Expression<Func<IExecutionContext, bool>> execContextValidator = (IExecutionContext c) => execContext.FlowUID == c.FlowUID &&
                                                                                                          execContext.CurrentExecutionId == c.CurrentExecutionId &&
                                                                                                          execContext.ParentExecutionId == c.ParentExecutionId;

                var mockVGrain = new Mock<ITestExtractEmailTransformer>(MockBehavior.Strict);
                mockVGrain.Setup(m => m.ExtractEmailsAsync(input, It.Is(execContextValidator)))
                         .Returns(Task.FromResult(expectedResults));

                var mockDiagnositicLogger = new Mock<IDiagnosticLogger>(MockBehavior.Strict);

                var mockVGrainProvider = new Mock<IVGrainProvider>(MockBehavior.Strict);
                mockVGrainProvider.Setup(m => m.GetVGrainAsync(callDefinition.VGrainType.ToType(), input, It.Is(execContextValidator), It.IsAny<ILogger>()))
                                 .Returns(ValueTask.FromResult<IVGrain>(mockVGrain.Object));

                // ACT
                var result = await handler.ExecAsync(callDefinition,
                                                     input,
                                                     execContext,
                                                     logger,
                                                     mockDiagnositicLogger.Object,
                                                     mockVGrainProvider.Object,
                                                     () => throw new InvalidOperationException("No need to get secure token"));

                // Check

                Check.That(result.result).IsNotNull();
                Check.That(result.expectedResultType).IsNotNull().And.IsEqualTo(typeof(string[])).And.IsEqualTo(callDefinition.Output);

                await result.result;

                mockVGrainProvider.Verify(m => m.GetVGrainAsync(callDefinition.VGrainType.ToType(), input, It.Is(execContextValidator), It.IsAny<ILogger>()), Times.Once);
                mockVGrain.Verify(m => m.ExtractEmailsAsync(input, It.Is(execContextValidator)), Times.Once);

                mockVGrainProvider.VerifyNoOtherCalls();
                mockVGrain.VerifyNoOtherCalls();

                /* No Manuel diagnostic call allowed */
                mockDiagnositicLogger.VerifyNoOtherCalls();

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
