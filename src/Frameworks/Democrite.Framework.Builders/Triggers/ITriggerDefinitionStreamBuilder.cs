// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    /// <summary>
    /// Common part of trigger definition
    /// </summary>
    public interface ITriggerDefinitionStreamBuilder
    {
        /// <summary>
        /// Set the maximumn of concurrent message processing; Default 1000 - Minimun 1
        /// </summary>
        /// <remarks>
        ///     fixMaxConcurrentProcess
        /// </remarks>
        ITriggerDefinitionBuilder MaxConcurrentProcess(uint maxConcurrent);

        /// <summary>
        /// Set the maximumn of concurrent message processing relative to number of silo.
        /// </summary>
        /// <remarks>
        ///     maxConcurrentProcess = (int)(factor * number_silo)
        ///    
        ///     if (fixMaxConcurrentProcess > 0 && maxConcurrentProcess > fixMaxConcurrentProcess)
        ///         maxConcurrentProcess = fixMaxConcurrentProcess
        /// </remarks>
        ITriggerDefinitionBuilder MaxConcurrentFactorClusterRelativeProcess(uint maxConcurrent);
    }
}
