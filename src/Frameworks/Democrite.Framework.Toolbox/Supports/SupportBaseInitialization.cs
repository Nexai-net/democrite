// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Supports
{
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System.Threading.Tasks;

    /// <summary>
    /// Base Implementation of <see cref="ISupportInitialization"/>
    /// </summary>
    /// <seealso cref="ISupportInitialization" />
    public abstract class SupportBaseInitialization : ISupportInitialization
    {
        #region Fields

        private long _initializating;
        private long _initializated;

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return Interlocked.Read(ref this._initializating) > 0; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return Interlocked.Read(ref this._initializated) > 0; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask InitializationAsync<TState>(TState? initializationState = default, CancellationToken token = default)
        {
            if (this.IsInitialized || Interlocked.Increment(ref this._initializating) > 1)
                return;

            try
            {
                await OnInitializationAsync(initializationState, token);
                Interlocked.Increment(ref this._initializated);
            }
            finally
            {
                Interlocked.Exchange(ref this._initializating, 0);
            }
        }

        /// <inheritdoc cref="ISupportInitialization.InitializationAsync{TState}(TState?, CancellationToken)" />
        protected abstract Task OnInitializationAsync<TState>(TState? initializationState, CancellationToken token);

        #endregion
    }
}
