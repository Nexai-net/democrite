﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;

    /// <inheritdoc cref="ISignalBuilder" />
    internal sealed class SignalBuilder : SignalNetworkBasePartBuilder<ISignalBuilder>, ISignalBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalBuilder"/> class.
        /// </summary>
        public SignalBuilder(string name, Guid? uid = null)
            : base(name, uid)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SignalDefinition Build()
        {
            return new SignalDefinition(this.Uid,
                                        this.Name,
                                        this.GroupName);
        }

        #endregion
    }
}
