// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Loggers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using System.Collections.Concurrent;
    using System.Linq;

    public static class DefinitionExtensions
    {
        #region Fields

        private static readonly IOptionsMonitor<LoggerFilterOptions> s_filterOption;
        private static readonly ConcurrentQueue<InMemoryLogger> s_loggerPool;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DefinitionExtensions"/> class.
        /// </summary>
        static DefinitionExtensions()
        {
            s_filterOption = (new LoggerFilterOptions() { MinLevel = LogLevel.Error }).ToMonitorOption();
            s_loggerPool = new ConcurrentQueue<InMemoryLogger>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates the with exception if error or critical
        /// </summary>
        public static void ValidateWithException(this IDefinition definition, ILogger? extraLogger = null)
        {
            InMemoryLogger? logger = null;
            try
            {
                if (!s_loggerPool.TryDequeue(out logger))
                {
                    logger = new InMemoryLogger(s_filterOption);
                }

                if (!definition.Validate(logger))
                {
                    var logs = logger.GetLogsCopy();

                    List<Exception>? errors = null;

                    foreach (var log in logs)
                    {
                        extraLogger?.OptiLog(log.LogLevel, log.Message);

                        if (log.LogLevel >= LogLevel.Error)
                        {
                            var ex = new DefinitionException(definition.GetType(), definition.Uid.ToString(), log.Message);
                            if (errors is null)
                                errors = new List<Exception>();
                            errors.Add(ex);
                        }
                    }

                    if (errors is not null && errors.Any())
                        throw new AggregateException(errors);
                }
            }
            finally
            {
                if (logger is not null && s_loggerPool.Count < short.MaxValue)
                    s_loggerPool.Enqueue(logger);
            }
        }

        #endregion
    }
}
