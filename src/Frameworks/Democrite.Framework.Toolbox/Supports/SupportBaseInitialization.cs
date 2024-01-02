// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Supports
{
    using Democrite.Framework.Toolbox.Abstractions.Supports;
    using Democrite.Framework.Toolbox.Disposables;

    using System.Threading.Tasks;

    /// <summary>
    /// Base Implementation of <see cref="ISupportInitialization"/>
    /// </summary>
    /// <seealso cref="ISupportInitialization" />
    public abstract class SupportBaseInitialization : SafeDisposable, ISupportInitialization
    {
        #region Fields

        private TaskCompletionSource _initializingTask;

        private long _initializing;
        private long _initialized;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportBaseInitialization"/> class.
        /// </summary>
        protected SupportBaseInitialization()
        {
            this._initializingTask = new TaskCompletionSource();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return Interlocked.Read(ref this._initializing) > 0; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return Interlocked.Read(ref this._initialized) > 0; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask InitializationAsync<TState>(TState? initializationState = default, CancellationToken token = default)
        {
            var initTask = this._initializingTask.Task;
            if (this.IsInitialized)
                return;

            if (Interlocked.Increment(ref this._initializing) > 1)
            {
                await initTask;
                return;
            }

            try
            {
                try
                {
                    await OnInitializedAsync(initializationState, token);
                    Interlocked.Increment(ref this._initialized);

                    var tmpTask = this._initializingTask;

                    this._initializingTask = new TaskCompletionSource();
                    tmpTask.TrySetResult();
                }
                finally
                {
                    Interlocked.Exchange(ref this._initializing, 0);
                }
            }
            catch (Exception ex)
            {
                this._initializingTask.TrySetException(ex);
                throw;
            }
        }

        /// <inheritdoc cref="ISupportInitialization.InitializationAsync{TState}(TState?, CancellationToken)" />
        protected abstract ValueTask OnInitializedAsync<TState>(TState? initializationState, CancellationToken token);

        #endregion
    }
}
