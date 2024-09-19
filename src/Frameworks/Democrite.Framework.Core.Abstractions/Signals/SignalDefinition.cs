// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defintion of a signal.
    /// </summary>
    /// <remarks>
    ///     A signal is a simple signal send by vgrain, door, ... an could trigger chain reaction
    /// </remarks>
    /// <seealso cref="SignalNetworkBasePartDefinition" />
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SignalDefinition : SignalNetworkBasePartDefinition
    {
        #region Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalDefinition"/> class.
        /// </summary>
        public SignalDefinition(Guid uid,
                                Uri refId,
                                string displayName,
                                DefinitionMetaData? metaData,
                                SignalId? parentSignalId = null)
            : base(uid, refId, RefIdHelper.GetSimpleNameIdentification(refId)!, displayName, metaData)
        {
            this.SignalId = new SignalId(uid, base.Name);
            this.ParentSignalId = parentSignalId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal identifier.
        /// </summary>
        [Id(0)]
        [IgnoreDataMember]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public SignalId SignalId { get; }

        [Id(1)]
        [DataMember]
        public SignalId? ParentSignalId { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override bool OnValidate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        /// <inheritdoc />
        protected override bool OnSignalEquals([NotNull] SignalNetworkBasePartDefinition other)
        {
            return other is SignalDefinition otherSignal &&
                   this.ParentSignalId == otherSignal.ParentSignalId;
        }

        /// <inheritdoc />
        protected override int OnSignalGetHashCode()
        {
            return this.ParentSignalId?.GetHashCode() ?? 0;
        }

        #endregion
    }
}
