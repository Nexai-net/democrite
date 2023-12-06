// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base Definition of any signal/signal
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract class SignalNetworkBasePartDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNetworkBasePartDefinition"/> class.
        /// </summary>
        protected SignalNetworkBasePartDefinition(Guid uid,
                                                  string name,
                                                  string? group = null)
        {
            this.Uid = uid;
            this.Name = name;
            this.Group = group;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        [Id(0)]
        public Guid Uid { get; }

        /// <summary>
        /// Gets the unique name.
        /// </summary>
        [Id(1)]
        public string Name { get; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        [Id(2)]
        public string? Group { get; }

        #endregion
    }
}
