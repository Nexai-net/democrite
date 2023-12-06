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
        #region Properties

        /// <summary>
        /// Gets or sets the state of the input provider.
        /// </summary>
        [Id(1)]
        public object? InputProviderState { get; set; }

        #endregion
    }
}
