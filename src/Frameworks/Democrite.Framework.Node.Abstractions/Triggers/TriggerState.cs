// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Triggers
{
    using System;

    [Serializable]
    [GenerateSerializer]
    public class TriggerState
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerState"/> class.
        /// </summary>
        public TriggerState()
        {
            this.DataSourceProviderStates = new Dictionary<Guid, object?>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of the output provider.
        /// </summary>
        [Id(1)]
        public Dictionary<Guid, object?> DataSourceProviderStates { get; private set; }

        #endregion
    }
}
