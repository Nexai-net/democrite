// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Define the mode a component MUST take
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class ComponentPermission
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentPermission"/> class.
        /// </summary>
        public ComponentPermission(bool enable, bool signalFireing, string etag)
        {
            this.Enable = enable;
            this.SignalFireing = signalFireing;
            this.Etag = etag;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="ComponentPermission"/> is enable.
        /// </summary>
        public bool Enable { get; }

        /// <summary>
        /// Gets a value indicating whether the component is allow to fire signals.
        /// </summary>
        public bool SignalFireing { get; }

        /// <summary>
        /// Gets the etag of the current component permissions
        /// </summary>
        public string Etag { get; }

        #endregion
    }
}
