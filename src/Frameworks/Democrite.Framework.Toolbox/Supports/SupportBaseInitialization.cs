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
    public abstract class SupportBaseInternalInitialization : SafeDisposable
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
        protected SupportBaseInternalInitialization()
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
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return InitializationImplAsync<NoneType>(NoneType.Instance, token);
        }

        /// <summary>
        /// Internal implementation that support state injection
        /// </summary>
        internal protected async ValueTask InitializationImplAsync<TState>(TState? initializationState, CancellationToken token = default)
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
                    await OnInitializingImplAsync(initializationState, token);
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
        internal protected abstract ValueTask OnInitializingImplAsync<TState>(TState? initializationState, CancellationToken token);

        #endregion
    }

    /// <summary>
    /// Base class to support initialization without state input
    /// </summary>
    /// <seealso cref="SupportBaseInternalInitialization" />
    /// <seealso cref="ISupportInitialization" />
    public abstract class SupportBaseInitialization : SupportBaseInternalInitialization, ISupportInitialization
    {
        /// <inheritdoc />
        protected internal sealed override ValueTask OnInitializingImplAsync<TState>(TState? initializationState, CancellationToken token) 
            where TState : default
        {
            return OnInitializedAsync(token);
        }

        /// <summary>
        /// Initialization without state
        /// </summary>
        protected abstract ValueTask OnInitializedAsync(CancellationToken token);
    }

    /// <summary>
    /// Base Implementation of <see cref="ISupportInitialization"/>
    /// </summary>
    /// <seealso cref="ISupportInitialization" />
    public abstract class SupportBaseInitialization<TState> : SupportBaseInternalInitialization, ISupportInitialization<TState>
    {
        #region Methods

        /// <inheritdoc />
        protected internal sealed override ValueTask OnInitializingImplAsync<TStateAsk>(TStateAsk? initializationState, CancellationToken token)
            where TStateAsk : default
        {
            var isNoneType = NoneType.IsEqualTo<TState>();

            TState? castInitState = default;

            if (initializationState is TState localCastInitState)
                castInitState = localCastInitState;

            if (!isNoneType && initializationState is not null && EqualityComparer<TState>.Default.Equals(castInitState, default))
                throw new InvalidCastException("Expect state to be a " + typeof(TState) + " not a " + typeof(TStateAsk));

            return OnInitializingAsync(isNoneType || initializationState is null ? default : castInitState,
                                      token);
        }

        /// <inheritdoc cref="ISupportInitialization.InitializationAsync{TState}(TState?, CancellationToken)" />
        protected abstract ValueTask OnInitializingAsync(TState? initializationState, CancellationToken token);

        /// <inheritdoc />
        public ValueTask InitializationAsync(TState? initializationState = default, CancellationToken token = default)
        {
            return base.InitializationImplAsync(initializationState, token);
        }

        #endregion
    }
}
