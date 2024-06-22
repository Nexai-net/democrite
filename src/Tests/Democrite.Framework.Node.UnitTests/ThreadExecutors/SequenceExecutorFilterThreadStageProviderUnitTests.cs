// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.ThreadExecutors;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;
    using Democrite.UnitTests.ToolKit;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Democrite.Framework.Node.UnitTests.Tools;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    /// <summary>
    /// Test <see cref="SequenceExecutorFilterThreadStageProvider"/> responsable to call vgrain method during flow resolution
    /// </summary>
    public sealed class SequenceExecutorFilterThreadStageProviderUnitTests
    {
        /// <summary>
        /// Test <see cref="SequenceExecutorFilterThreadStageProvider"/> constructor and dispose action
        /// </summary>
        [Fact]
        public void SequenceExecutorFilterThreadStageProvider_Ctor_Dispose()
        {
            using (var provider = new SequenceExecutorFilterThreadStageProvider(Substitute.For<IServiceProvider>()))
            {

            }
        }

        /// <summary>
        /// Test <see cref="SequenceExecutorFilterThreadStageProvider.CanHandler"/>
        /// </summary>
        [Fact]
        public void SequenceExecutorFilterThreadStageProvider_CanHandler()
        {
            using (var provider = new SequenceExecutorFilterThreadStageProvider(Substitute.For<IServiceProvider>()))
            {
                Check.ThatCode(() => provider.CanHandler(null)).DoesNotThrow().And.WhichResult().IsFalse();

                var mockBaseSequenceStage = TestSequenceStageDefinition.Create();
                Check.ThatCode(() => provider.CanHandler(mockBaseSequenceStage)).DoesNotThrow().And.WhichResult().IsFalse();

                var def = typeof(SequenceExecutorFilterThreadStageProviderUnitTests).GetMethod(nameof(SequenceExecutorFilterThreadStageProvider_CanHandler))!.GetAbstractMethod();

                Check.ThatCode(() => provider.CanHandler(new SequenceStageCallDefinition(Guid.NewGuid(),
                                                                                         null,
                                                                                         null,
                                                                                         (ConcretType)(typeof(string).GetAbstractType()),
                                                                                         def,
                                                                                         null,
                                                                                         null,
                                                                                         null,
                                                                                         null,
                                                                                         null)))
                     .DoesNotThrow()
                     .And
                     .WhichResult()
                     .IsFalse();

                // True
                Check.ThatCode(() => provider.CanHandler(SequenceStageFilterDefinition.From<string[], string>(s => s != null)))
                     .WhichResult()
                     .IsTrue();
            }
        }

        /// <summary>
        /// Test <see cref="SequenceExecutorFilterThreadStageProvider.ExecAsync"/> with valid value
        /// </summary>
        [Fact]
        public async Task SequenceExecutorFilterThreadStageProvider_ExecAsync()
        {
            var democriteSerializerMock = Substitute.For<IDemocriteSerializer>();
            var timeManagerMock = Substitute.For<ITimeManager>();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(democriteSerializerMock)
                             .AddSingleton(timeManagerMock);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (var provider = new SequenceExecutorFilterThreadStageProvider(serviceProvider))
            {
                // Use 'x' min/maj as flag to identify the expected result
                Expression<Func<string, bool>> filterExpression = (string input) => !string.IsNullOrEmpty(input) &&
                                                                                    input.Contains('x', StringComparison.OrdinalIgnoreCase);

                var filterDefinition = SequenceStageFilterDefinition.From<string[], string>(filterExpression);

                // Get Handler
                var handler = provider.Provide(filterDefinition);

                var handlerFromNull = provider.Provide(filterDefinition);

                /* Create specific to input but cache result */

                Check.That(handler).IsNotNull();
                Check.That(handlerFromNull).IsNotNull().And.IsSameReferenceAs(handler);

                var execContext = new Democrite.Framework.Core.Models.ExecutionContext(Guid.NewGuid(), Guid.NewGuid(), null);
                var logger = new MemoryTestLogger();

                var input = new[] { "Failed", "failedX", string.Empty, null, "found", "foundx" };
                var expectedResults = new[] { "failedX", "foundx" };

                Expression<Func<IExecutionContext, bool>> execContextValidator = (IExecutionContext c) => execContext.FlowUID == c.FlowUID &&
                                                                                                          execContext.CurrentExecutionId == c.CurrentExecutionId &&
                                                                                                          execContext.ParentExecutionId == c.ParentExecutionId;

                var diagnosticLogs = new List<IDiagnosticInOutLog>();

                var mockDiagnositicLogger = new Mock<IDiagnosticLogger>(MockBehavior.Strict);
                mockDiagnositicLogger.Setup(m => m.Log(It.IsAny<IDiagnosticInOutLog>()))
                                     .Callback<IDiagnosticInOutLog>((log) => diagnosticLogs.Add(log));

                var mockVGrainProvider = new Mock<IVGrainProvider>(MockBehavior.Strict);

                // ACT
                var result = await handler.ExecAsync(filterDefinition,
                                                     input,
                                                     execContext,
                                                     logger,
                                                     mockDiagnositicLogger.Object,
                                                     mockVGrainProvider.Object,
                                                     () => throw new InvalidOperationException("No need to get secure token"));

                // Check

                Check.That(result.result).IsNotNull();
                Check.That(result.expectedResultType).IsNotNull().And.IsEqualTo(typeof(string[])).And.IsEqualTo(filterDefinition.Output);

                await result.result;

                mockDiagnositicLogger.Verify(m => m.Log(It.IsAny<IDiagnosticInOutLog>()), Times.Exactly(2));
                mockDiagnositicLogger.VerifyNoOtherCalls();

                Check.That(diagnosticLogs).CountIs(2);

                var inLog = diagnosticLogs.Single(d => d.Type == DiagnosticLogTypeEnum.InOutContext && d.Orientation == OrientationEnum.In);
                var outLog = diagnosticLogs.Single(d => d.Type == DiagnosticLogTypeEnum.InOutContext && d.Orientation == OrientationEnum.Out);

                var inputLogFlattern = inLog.InOut?.Flattern();
                Check.That(inputLogFlattern).IsNotNull().And.HasOneElementOnly();

                var inputSingleLog = inputLogFlattern?.Single();
                Check.That(inputSingleLog).IsNotNull()
                                          .And
                                          .IsInstanceOf<string[]>()
                                          .Which
                                          .ContainsNoDuplicateItem()
                                          .And
                                          .ContainsOnlyElementsThatMatch(i => input.Contains(i));

                var outLogFlattern = outLog.InOut?.Flattern();
                Check.That(outLogFlattern).IsNotNull().And.HasOneElementOnly();

                var outputSingleLog = outLogFlattern?.Single();
                Check.That(outputSingleLog).IsNotNull()
                                           .And
                                           .IsInstanceOf<string[]>()
                                           .Which
                                           .ContainsNoDuplicateItem()
                                           .And
                                           .ContainsOnlyElementsThatMatch(o => expectedResults.Contains(o));

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
