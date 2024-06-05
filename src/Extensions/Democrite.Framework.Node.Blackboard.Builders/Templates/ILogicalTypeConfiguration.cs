// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using System.Xml;

    /// <summary>
    /// Configuration dedicated to one type on record on the blackboard
    /// </summary>
    public interface ILogicalTypeBaseConfiguration
    {
        /// <summary>
        /// Gets the rules.
        /// </summary>
        IReadOnlyCollection<BlackboardLogicalTypeBaseRule> Rules { get; }

        /// <summary>
        /// Gets the logical record type pattern.
        /// </summary>
        string LogicalRecordTypePattern { get; }

        /// <summary>
        /// Maximums of record allowed.
        /// </summary>
        void AddRule(BlackboardLogicalTypeBaseRule rule);
    }

    /// <summary>
    /// Configuration dedicated to one type on record on the blackboard
    /// </summary>
    public interface ILogicalTypeConfiguration : ILogicalTypeBaseConfiguration
    {
        /// <summary>
        /// Define type allowed
        /// </summary>
        ILogicalTypeConfiguration AllowType<T>(Action<ILogicalTypeCheckConfiguration<T>>? cfg = null);

        /// <summary>
        /// Flag the logical type to remain after sealing
        /// </summary>
        ILogicalTypeConfiguration RemainOnSealed();

        /// <summary>
        /// Allow only one value
        /// </summary>
        ILogicalTypeConfiguration OnlyOne(bool includeDecommissioned = false, bool replacePreviousOne = false);

        /// <summary>
        /// Maximums of record allowed.
        /// </summary>
        /// <param name="includeDecommissioned"><c>true</c> if decommissioned value must be count.</param>
        /// <param name="maxRecords">Define the maximum number to allow simultaniously</param>
        /// <param name="preferenceResolution">Define a prefered solution to solve any issue; Attention the controller have the final choice.</param>
        ILogicalTypeConfiguration MaxRecord(short maxRecords,
                                            bool includeDecommissioned = false,
                                            BlackboardProcessingResolutionLimitTypeEnum? preferenceResolution = null,
                                            BlackboardProcessingResolutionRemoveTypeEnum? removeResolution = null);

        /// <summary>
        /// Define default storage usage
        /// </summary>
        ILogicalTypeConfiguration Storage(string storageKey, string? storageConfiguration = null);

        /// <summary>
        /// Tolerate only one record by logical type that match the configuration
        /// </summary>
        /// <param name="replace">Allow replacement if id differ</param>
        ILogicalTypeConfiguration Unique(bool replace = false);

        /// <summary>
        /// Set an order manually; by default the order will the declaration one
        /// </summary>
        ILogicalTypeConfiguration Order(ushort order);

        /// <summary>
        /// Define default storage usage
        /// </summary>
        ILogicalTypeConfiguration UseDefaultStorage(string storageKey);

        // ILogicalTypeConfiguration Aggreagator<TDataRecordAggrator>(AggregateModeEnum mode) // Mode: { Record, Timed, OnStatusChanged } Flags
        // ILogicalTypeConfiguration Convertor<TDataFrom, TDataTo>(AggregateModeEnum mode) // Mode: { Record, Timed, OnStatusChanged } Flags

        /*
         * Security LogicalTypes (Who , ...) , Data Validation,  
         */
    }

    /// <summary>
    /// Configuration dedicated to one type on record on the blackboard
    /// </summary>
    public interface ILogicalTypeCheckConfiguration<TValue> : ILogicalTypeBaseConfiguration
    {

    }
}
