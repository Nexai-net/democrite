// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    /// <summary>
    /// Define a trigegr based on cron expression
    /// </summary>
    /// <seealso cref="ITriggerDefinition" />
    public interface ICronTriggerDefinition : ITriggerDefinition
    {
        /// <summary>
        /// Gets the cron expression.
        /// </summary>
        string CronExpression { get; }
    }
}
