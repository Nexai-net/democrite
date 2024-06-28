// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using System.Runtime.CompilerServices;

    /// <summary>
    /// Define constants use by Blackboard extensions
    /// </summary>
    public static class BlackboardConstants
    {
        #region Fields

        /// <summary>
        /// The default board template configuration id
        /// </summary><
        /// <remarks>
        ///     Used to inject directly <see cref="IBlackboardRef"/> by ctor
        ///     Not Tested yet
        /// </remarks>
        public const string DefaultBoardTemplateConfigurationKey = "DefaultDemocriteBlackboard";

        /// <summary>
        /// The blackboard registry storage configuration key (registry is the association between blackboad name+template and uid)
        /// </summary>
        public const string BlackboardRegistryStorageKey = "BlackboardRegistries";

        /// <summary>
        /// The blackboard registry storage configuration key (registry is the association between blackboad name+template and uid)
        /// </summary>
        public const string BlackboardRegistryStorageConfigurationKey = "BlackboardRegistriesStorage";

        /// <summary>
        /// The blackboard state storage configuration key (Blackboard Grain, controllers, ... states)
        /// </summary>
        public const string BlackboardStateStorageConfigurationKey = "BlackboardStateStorage";

        /// <summary>
        /// The blackboard state storage key (Blackboard Grain, controllers, ... states)
        /// </summary>
        public const string BlackboardStateStorageKey = "Blackboards";

        /// <summary>
        /// The blackboard records storage key
        /// </summary>
        public const string BlackboardStorageRecordsKey = "BlackboardRecords";

        /// <summary>
        /// The blackboard  records storage configuration key
        /// </summary>
        public const string BlackboardStorageRecordsConfigurationKey = "BlackboardRecordsStorage";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BlackboardConstants"/> class.
        /// </summary>
        static BlackboardConstants()
        {
            BlackboardAnalyzeFeedbackSignal = new SignalId(new Guid("DC879630-A5AE-45B6-AF8D-5A8A6E1B94C1"), "BlackboardAnalyzeFeedbackSignal");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the blackboard analyze feedback signal.
        /// </summary>
        public static SignalId BlackboardAnalyzeFeedbackSignal { get; }

        #endregion
    }
}
