// Keep : Democrite.Framework.Core.Abstractions
namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Text;

    /// <summary>
    /// Entend the execution managment
    /// </summary>
    public static class ExecutionResultExtensions
    {
        /// <summary>
        /// Throws if failed or cancelled; otherwise return the execution result
        /// </summary>
        public static void SafeGetResult(this IExecutionResult result, ILogger? logger = null)
        {
            SafeGetResult<NoneType>(result, logger);
        }

        /// <summary>
        /// Throws if failed or cancelled; otherwise return the execution result
        /// </summary>
        public static TResult? SafeGetResult<TResult>(this IExecutionResult<TResult> result, ILogger? logger = null)
        {
            return SafeGetResult<TResult>((IExecutionResult)result, logger);
        }

        /// <summary>
        /// Throws if failed or cancelled; otherwise return the execution result
        /// </summary>
        public static TResult? SafeGetResult<TResult>(this IExecutionResult result, ILogger? logger = null) 
        {
            ArgumentNullException.ThrowIfNull(result);

            logger ??= NullLogger.Instance;

            if (result.Cancelled)
                throw new OperationCanceledException(result.Message ?? result.ToDebugDisplay());

            if (result.Succeeded == false)
            {
                throw new DemocriteException(result.Message ?? result.ToDebugDisplay(),
                                             DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Execution, genericType: DemocriteErrorCodes.ErrorType.Failed));
            }
            else if (!string.IsNullOrEmpty(result.Message))
            {
                logger.OptiLog(LogLevel.Warning, "[ExecutionResult:{executionId}] {message}", result.ExecutionId, result.Message);
            }

            if (!NoneType.IsEqualTo<TResult>() && result.HasOutput)
            {
                var output = result.GetOutput();
                if (output is TResult resultOutput)
                    return resultOutput;

                if (AnyType.IsEqualTo<TResult>())
                    return (TResult)AnyType.CreateContainer(output, output?.GetType() ?? typeof(object));
            }

            return default;
        }

        /// <summary>
        /// Provide a string information about the <see cref="IExecutionResult"/>
        /// </summary>
        public static string ToDebugDisplay(this IExecutionResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result is ISupportDebugDisplayName support)
                return support.ToDebugDisplayName();

            var str = new StringBuilder();

            str.Append("[");
            str.Append(nameof(result.ExecutionId));
            str.Append(": ");
            str.Append(result.ExecutionId);
            str.Append("]");

            if (result.Cancelled)
                str.Append(" [Cancelled]");
            else if (!result.Succeeded)
                str.Append(" [Failed]");

            if (result.HasOutput)
            {
                str.Append(" [Output:");
                str.Append(result.OutputType);
                str.Append("] ");
                
                var output = result.GetOutput();

                if (output is ISupportDebugDisplayName outputSupport)
                    str.Append(outputSupport.ToDebugDisplayName());
                else
                    str.Append(output);
            }

            return str.ToString();
        }
    }
}
