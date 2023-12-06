// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using Democrite.Framework.Toolbox.Abstractions.Loggers;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Loggers;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extension to format and optimize log
    /// </summary>
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Optimize the log call to minimze boxing and use templating
        /// </summary>
        public static void OptiLog(this ILogger logger,
                                   LogLevel logLevel,
                                   string message,
                                   NoneType? unusedInferenceBreaker = null,
                                   [CallerMemberName] string? callerMemberName = null,
                                   [CallerLineNumber] int lineNumber = 0)
        {
            if (logger.IsEnabled(logLevel))
                WriteLog(logger, logLevel, message, callerMemberName, lineNumber, EnumerableHelper<object>.ReadOnlyArray);
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)"/>
        public static void OptiLog<T1>(this ILogger logger,
                                       LogLevel logLevel,
                                       string message,
                                       T1? arg,
                                       NoneType? unusedInferenceBreaker = null,
                                       [CallerMemberName] string? callerMemberName = null,
                                       [CallerLineNumber] int lineNumber = 0)
        {
            if (logger.IsEnabled(logLevel))
                WriteLog(logger, logLevel, message, callerMemberName, lineNumber, new object?[] { arg });
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OptiLog(this ILogger logger,
                                   LogLevel logLevel,
                                   string message,
                                   string? arg,
                                   NoneType? unusedInferenceBreaker = null,
                                   [CallerMemberName] string? callerMemberName = null,
                                   [CallerLineNumber] int lineNumber = 0)
        {
            OptiLog<string>(logger,
                            logLevel,
                            message,
                            arg,
                            unusedInferenceBreaker,
                            callerMemberName,
                            lineNumber);
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)"/>
        public static void OptiLog<T1, T2>(this ILogger logger,
                                           LogLevel logLevel,
                                           string message,
                                           T1? arg,
                                           T2? arg2,
                                           NoneType? unusedInferenceBreaker = null,
                                           [CallerMemberName] string? callerMemberName = null,
                                           [CallerLineNumber] int lineNumber = 0)
        {
            if (logger.IsEnabled(logLevel))
                WriteLog(logger, logLevel, message, callerMemberName, lineNumber, new object?[] { arg, arg2 });
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OptiLog(this ILogger logger,
                                   LogLevel logLevel,
                                   string message,
                                   string? arg,
                                   string? arg2,
                                   NoneType? unusedInferenceBreaker = null,
                                   [CallerMemberName] string? callerMemberName = null,
                                   [CallerLineNumber] int lineNumber = 0)
        {
            OptiLog<string, string>(logger,
                                    logLevel,
                                    message,
                                    arg,
                                    arg2,
                                    unusedInferenceBreaker,
                                    callerMemberName,
                                    lineNumber);
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)
        public static void OptiLog<T1, T2, T3>(this ILogger logger,
                                                   LogLevel logLevel,
                                                   string message,
                                                   T1? arg,
                                                   T2? arg2,
                                                   T3? arg3,
                                                   NoneType? unusedInferenceBreaker = null,
                                                   [CallerMemberName] string? callerMemberName = null,
                                                   [CallerLineNumber] int lineNumber = 0)
        {
            if (logger.IsEnabled(logLevel))
                WriteLog(logger, logLevel, message, callerMemberName, lineNumber, new object?[] { arg, arg2, arg3 });
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OptiLog(this ILogger logger,
                                       LogLevel logLevel,
                                       string message,
                                       string? arg,
                                       string? arg2,
                                       string? arg3,
                                       NoneType? unusedInferenceBreaker = null,
                                       [CallerMemberName] string? callerMemberName = null,
                                                   [CallerLineNumber] int lineNumber = 0)
        {
            OptiLog<string, string, string>(logger,
                                            logLevel,
                                            message,
                                            arg,
                                            arg2,
                                            arg3,
                                            unusedInferenceBreaker,
                                            callerMemberName,
                                            lineNumber);
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)
        public static void OptiLog<T1, T2, T3, T4>(this ILogger logger,
                                                   LogLevel logLevel,
                                                   string message,
                                                   T1? arg,
                                                   T2? arg2,
                                                   T3? arg3,
                                                   T4? arg4,
                                                   NoneType? unusedInferenceBreaker = null,
                                                   [CallerMemberName] string? callerMemberName = null,
                                                   [CallerLineNumber] int lineNumber = 0)
        {
            if (logger.IsEnabled(logLevel))
                WriteLog(logger, logLevel, message, callerMemberName, lineNumber, new object?[] { arg, arg2, arg3, arg4 });
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)
        public static void OptiLog<T1, T2, T3, T4, T5>(this ILogger logger,
                                                       LogLevel logLevel,
                                                       string message,
                                                       T1? arg,
                                                       T2? arg2,
                                                       T3? arg3,
                                                       T4? arg4,
                                                       T5? arg5,
                                                       NoneType? unusedInferenceBreaker = null,
                                                       [CallerMemberName] string? callerMemberName = null,
                                                       [CallerLineNumber] int lineNumber = 0)
        {
            if (logger.IsEnabled(logLevel))
                WriteLog(logger, logLevel, message, callerMemberName, lineNumber, new object?[] { arg, arg2, arg3, arg4, arg5 });
        }

        /// <inheritdoc cref="OptiLog(ILogger, LogLevel, string, string, int)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OptiLog(this ILogger logger,
                                   LogLevel logLevel,
                                   string message,
                                   string? arg,
                                   string? arg2,
                                   string? arg3,
                                   string? arg4,
                                   NoneType? unusedInferenceBreaker = null,
                                   [CallerMemberName] string? callerMemberName = null,
                                   [CallerLineNumber] int lineNumber = 0)
        {
            OptiLog<string, string, string, string>(logger,
                                                    logLevel,
                                                    message,
                                                    arg,
                                                    arg2,
                                                    arg3,
                                                    arg4,
                                                    unusedInferenceBreaker,
                                                    callerMemberName,
                                                    lineNumber);
        }

        /// <summary>
        /// Writes the log in <see cref="ILogger"/> following template
        /// </summary>
        private static void WriteLog(ILogger logger,
                                     LogLevel logLevel,
                                     string template,
                                     string? callerMemberName,
                                     int lineNumber,
                                     object?[] readOnlyArray)
        {
            logger.Log(logLevel,
                       "[{sourceFile}:{lineNumber}] - " + template,
                       new object?[] { callerMemberName, lineNumber }.Concat(readOnlyArray ?? EnumerableHelper<object>.ReadOnlyArray).ToArray());
        }

        /// <summary>
        /// Create a proxy to apply a conditional log activation 
        /// </summary>
        public static ILogger AddEnableCondition<TCondHolder>(this ILogger logger,
                                                              TCondHolder condHolder,
                                                              Func<TCondHolder, bool> condition)
        {
            return new ConditionalLogger<TCondHolder>(logger, condHolder, condition);
        }

        /// <summary>
        /// Create a proxy to apply a conditional log activation 
        /// </summary>
        public static ILogger<TLogCategory> AddEnableCondition<TCondHolder, TLogCategory>(this ILogger<TLogCategory> logger,
                                                                                          TCondHolder condHolder,
                                                                                          Func<TCondHolder, bool> condition)
        {
            return new ConditionalLogger<TLogCategory, TCondHolder>(logger, condHolder, condition);
        }

        /// <summary>
        /// Adds the singleton memory logger.
        /// </summary>
        public static ILoggingBuilder AddSingletonMemoryLogger(this ILoggingBuilder loggerBuilder)
        {
            loggerBuilder.Services.AddSingleton<IInMemoryLogger, InMemoryLogger>()
                                  .AddSingleton<ILoggerProvider, SingletonMemoryLoggerProvider>();

            return loggerBuilder;
        }
    }
}
