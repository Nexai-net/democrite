// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Options
{
    using Microsoft.Extensions.Options;

    using System;

    /// <summary>
    /// Static implementation of <see cref="IOptionsMonitor{TOption}"/>
    /// </summary>
    internal sealed class StaticMonitorOption<TOption> : IOptionsMonitor<TOption>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticMonitorOption{TOption}"/> class.
        /// </summary>
        public StaticMonitorOption(TOption option)
        {
            this.CurrentValue = option;
        }

        #endregion

        #region Properties

        public TOption CurrentValue { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TOption Get(string? name)
        {
            return this.CurrentValue;
        }

        /// <inheritdoc />
        public IDisposable? OnChange(Action<TOption, string?> listener)
        {
            return null;
        }

        #endregion
    }
}
