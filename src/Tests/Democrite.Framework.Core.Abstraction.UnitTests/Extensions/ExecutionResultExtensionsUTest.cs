// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests.Extensions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Models;
    using Democrite.UnitTests.ToolKit;

    using Microsoft.Extensions.Logging.Abstractions;

    using NFluent;

    using System;

    /// <summary>
    /// Test for <see cref="ExecutionResultExtensions"/>
    /// </summary>
    public sealed class ExecutionResultExtensionsUTest
    {
        [Fact]
        public void ExecutionResult_SafeGetResult_Cancelled()
        {
            var execResult = new ExecutionResult<int>(Guid.NewGuid(),
                                                      false,
                                                      true,
                                                      Guid.NewGuid().ToString(),
                                                      null,
                                                      42);

            Check.ThatCode(() => execResult.SafeGetResult(NullLogger.Instance)).Throws<OperationCanceledException>();
        }

        [Fact]
        public void ExecutionResult_SafeGetResult_Cancelled_ToDebugDisplay()
        {
            var execResult = new ExecutionResult<int>(Guid.NewGuid(),
                                                      false,
                                                      true,
                                                      Guid.NewGuid().ToString(),
                                                      null,
                                                      42);

            var str = execResult.ToDebugDisplay();
            var expect = "[ExecutionId: " + execResult.ExecutionId + "] [Cancelled] [Output:Int32] " + execResult.Output;

            Check.That(str).IsEqualTo(expect);
        }

        [Fact]
        public void ExecutionResult_SafeGetResult_Exception()
        {
            var execResult = new ExecutionResult<int>(Guid.NewGuid(),
                                                     false,
                                                     false,
                                                     Guid.NewGuid().ToString(),
                                                     null,
                                                     Random.Shared.Next(10, 1520));

            var msg = "[ExecutionId: " + execResult.ExecutionId + "] [Failed] [Output:Int32] " + execResult.Output;
            Check.ThatCode(() => execResult.SafeGetResult(NullLogger.Instance)).Throws<DemocriteException>().WithMessage(msg);
        }

        [Fact]
        public void ExecutionResult_SafeGetResult_Exception_ToDebugDisplay()
        {
            var execResult = new ExecutionResult<int>(Guid.NewGuid(),
                                                      false,
                                                      false,
                                                      Guid.NewGuid().ToString(),
                                                      null,
                                                      Random.Shared.Next(10, 1520));

            var str = execResult.ToDebugDisplay();
            var expect = "[ExecutionId: " + execResult.ExecutionId + "] [Failed] [Output:Int32] " + execResult.Output;
            
            Check.That(str).IsEqualTo(expect);
        }

        [Fact]
        public void ExecutionResult_SafeGetResult_Output()
        {
            var execResult = new ExecutionResult<int>(Guid.NewGuid(),
                                                      true,
                                                      false,
                                                      Guid.NewGuid().ToString(),
                                                      Guid.NewGuid().ToString(),
                                                      Random.Shared.Next(10, 1520));

            var memoryLogger = new MemoryTestLogger();

            var output = execResult.SafeGetResult(memoryLogger);

            Check.That(output).IsEqualTo(execResult.Output);
            Check.That(memoryLogger.Logs.Count).IsEqualTo(1);
            Check.That(memoryLogger.Logs.First().message).IsEqualTo("[SafeGetResult:56] - [ExecutionResult:" + execResult.ExecutionId + "] " + execResult.Message);
        }

        [Fact]
        public void ExecutionResult_SafeGetResult_Output_ToDebugDisplay()
        {
            var execResult = new ExecutionResult<int>(Guid.NewGuid(),
                                                      true,
                                                      false,
                                                      Guid.NewGuid().ToString(),
                                                      Guid.NewGuid().ToString(),
                                                      Random.Shared.Next(10, 1520));

            var str = execResult.ToDebugDisplay();
            var expect = "[ExecutionId: " + execResult.ExecutionId + "] [Output:Int32] " + execResult.Output;

            Check.That(str).IsEqualTo(expect);
        }
    }
}
