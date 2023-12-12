// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defintion of a signal.
    /// </summary>
    /// <remarks>
    ///     A signal is a simple signal send by vgrain, door, ... an could trigger chain reaction
    /// </remarks>
    /// <seealso cref="SignalNetworkBasePartDefinition" />
    [Immutable]
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
                                string name,
                                string? group = null)
            : base(uid, name, group)
        {
            this.SignalId = new SignalId(uid, name);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal identifier.
        /// </summary>
        [Id(0)]
        [IgnoreDataMember]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public SignalId SignalId { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override bool OnValidate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
