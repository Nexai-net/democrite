// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Helpers;

    using Elvex.Toolbox.Extensions;

    using MessagePack.Formatters;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Stateless Base implementation of <see cref="IVGrain"/>
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="IVGrain" />
    /// <remarks>
    ///     This base class must be used instead of <see cref="Grain"/> to allow vgrain system (orleans) to be replace more easily if needed
    /// </remarks>
    public abstract class VGrainBase<TVGrainInterface> : Grain, IVGrain, ILifecycleParticipant<IGrainLifecycle>, IDisposable, IVGrainInformationProvider
        where TVGrainInterface : IVGrain
    {
        #region Fields

        private readonly CancellationTokenSource _lifecycleCancellationToken;

        private readonly List<IDisposable> _disposables;
        private long _disposableRefCount = 0;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainBase"/> class.
        /// </summary>
        public VGrainBase(ILogger<TVGrainInterface> logger)
            : base()
        {
            this._disposables = new List<IDisposable>();

            this.Logger = logger;

            this.MetaData = BuildVGrainMetaData(GetType());
            this._lifecycleCancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="VGrainBase"/> class.
        /// </summary>
        ~VGrainBase()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; protected set; }

        /// <inheritdoc />
        public VGrainMetaData MetaData { get; }

        /// <summary>
        /// Gets the grain lifecycle token.
        /// </summary>
        public CancellationToken VGrainLifecycleToken
        {
            get { return this._lifecycleCancellationToken.Token; }
        }

        /// <summary>
        /// Gets the identity card.
        /// </summary>
        public IComponentIdentityCard? IdentityCard { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Participate(IGrainLifecycle observer)
        {
            var observerName = GetType().Name;

            lock (this._disposables)
            {
                this._disposables.Add(observer.Subscribe(observerName, GrainLifecycleStage.First, ActivationFirst));
                this._disposables.Add(observer.Subscribe(observerName, GrainLifecycleStage.Last, ActivationLast));
                this._disposables.Add(observer.Subscribe(observerName, GrainLifecycleStage.SetupState, ActivationSetupState));
                this._disposables.Add(observer.Subscribe(observerName, GrainLifecycleStage.Activate, VGrainActivate));
            }
        }

        /// <inheritdoc />
        public GrainId GetGrainId()
        {
            return base.GrainReference.GrainId;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #region Tools

        /// <summary>
        /// Called on first <see cref="VGrainBase"/> setup.
        /// </summary>
        private Task ActivationFirst(CancellationToken ct)
        {
            LifecycleLog(nameof(ActivationFirst));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called on last <see cref="VGrainBase"/> setup.
        /// </summary>
        private Task ActivationLast(CancellationToken ct)
        {
            LifecycleLog(nameof(ActivationLast));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called on <see cref="VGrainBase"/> setup.
        /// </summary>
        private async Task ActivationSetupState(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            try
            {
                CheckGrainIdValidity();
                LifecycleLog(nameof(ActivationSetupState));
                await OnActivationSetupState(ct);
            }
            catch (Exception ex)
            {
                this.Logger.OptiLog(LogLevel.Critical, "{grainName}.{method} - VGrain {grainId} - Exception : {exception}", GetType().Name, nameof(ActivationSetupState), this.GrainReference.GrainId, ex);
                throw;
            }
        }

        /// <summary>
        /// Called on <see cref="VGrainBase"/> setup.
        /// </summary>
        protected virtual Task OnActivationSetupState(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            LifecycleLog(nameof(OnActivateAsync));
            return base.OnActivateAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            LifecycleLog(nameof(OnDeactivateAsync));
            this._lifecycleCancellationToken.Cancel();
            return base.OnDeactivateAsync(reason, cancellationToken);
        }

        /// <summary>
        /// Called on <see cref="VGrainBase"/> activated.
        /// </summary>
        private Task VGrainActivate(CancellationToken ct)
        {
            LifecycleLog(nameof(VGrainActivate));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="fromManual"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool fromManual)
        {
            if (Interlocked.Increment(ref this._disposableRefCount) > 1)
                return;

            DisposeResourcesBegin();

            if (fromManual)
            {
                DisposeManagedResources();

                IEnumerable<IDisposable> disposables;
                lock (this._disposables)
                {
                    disposables = this._disposables.ToArray();
                    this._disposables.Clear();
                }

                foreach (var disposable in disposables)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.OptiLog(LogLevel.Error,
                                            "{type}.Dispose - VGrain Id : '{id}' - Exception {exeception} (From {from}).",
                                            GetType(),
                                            base.GrainReference.GrainId,
                                            ex,
                                            disposable);
                    }
                }
            }

            DisposeUnManagedResources();

            DisposeResourcesEnd();
        }

        /// <summary>
        /// End step Disposes the resources.
        /// </summary>
        protected virtual void DisposeResourcesEnd()
        {
        }

        /// <summary>
        /// Disposes un managed resources.
        /// </summary>
        protected virtual void DisposeUnManagedResources()
        {
        }

        /// <summary>
        /// Disposes managed resources; only call on manual dispose
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
        }

        /// <summary>
        /// Begin step Disposes the resources.
        /// </summary>
        protected virtual void DisposeResourcesBegin()
        {
        }

        /// <summary>
        /// Builds the vgrain meta data.
        /// </summary>
        private static VGrainMetaData BuildVGrainMetaData(Type type)
        {
            return VGrainMetaDataHelper.GetVGrainMetaDataType(type);
        }

        /// <summary>
        /// Lifecycles the log.
        /// </summary>
        private void LifecycleLog(in string state)
        {
            var grainId = GetGrainId();
            this.Logger.OptiLog(LogLevel.Trace, "{state} : {vgrainType}:{vgrainId}", state, grainId.Type, grainId.Key);
        }

        /// <summary>
        /// Registers it self as a component and get in return its dedicated <see cref="IComponentIdentityCard"/>
        /// That managed permission.
        /// </summary>
        internal async ValueTask<TIdentityCard> RegisterAsComponentAsync<TIdentityCard>(Guid componentId)
            where TIdentityCard : IComponentIdentityCard
        {
            var componentIdentityCardProvider = this.ServiceProvider.GetRequiredService<IComponentIdentitCardProviderClient>();

            var card = await componentIdentityCardProvider.GetComponentIdentityCardAsync(GetGrainId(), componentId);
            this.IdentityCard = card;

            return (TIdentityCard)card;
        }

        /// <summary>
        /// Checks the grain identifier validity.
        /// </summary>
        /// <exception cref=""></exception>
        private void CheckGrainIdValidity()
        {
            // OPTI : Cache attribute by type
            var validatorAttributes = GetType().GetCustomAttributes()
                                               .OfType<VGrainIdBaseValidatorAttribute>()
                                               .ToArray();

            if (validatorAttributes.Length == 0)
                return;

            var grainId = GetGrainId();
            var grainIdStr = grainId.ToString();

            foreach (var validator in validatorAttributes)
            {
                if (!validator.Validate(grainId, grainIdStr, this.Logger))
                    throw new VGrainIdValidationFaildException(validator, GetType(), grainId);
            }
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Base implementation of <see cref="IVGrain"/> with a specific <typeparamref name="TVGrainState"/>
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="IVGrain" />
    /// <remarks>
    ///     This base class must be used instead of <see cref="Grain"/> to allow vgrain system (orleans) to be replace more easily if needed
    /// </remarks>
    public abstract class VGrainStatedBase<TVGrainState, TVGrainInterface> : VGrainBase<TVGrainInterface>, ILifecycleParticipant<IGrainLifecycle>
        where TVGrainInterface : IVGrain
        where TVGrainState : class
    {
        #region Fields

        protected static readonly Type s_stateTraits = typeof(TVGrainState);

        private TVGrainState? _state;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainBase"/> class.
        /// </summary>
        protected VGrainStatedBase(ILogger<TVGrainInterface> logger, bool stateStorageEnabled = true)
            : base(logger)
        {
            this.StateStorageEnabled = stateStorageEnabled;

            if (s_stateTraits.GetTypeInfoExtension().IsCSharpScalarType)
                logger.OptiLog(LogLevel.Warning, "Direct scalar type ({grainStateRisky}) as state is not managed by this kind of repository.", s_stateTraits);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets saved state.
        /// </summary>
        protected TVGrainState? State
        {
            get { return this._state ?? default; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether state storage is enabled.
        /// </summary>
        protected bool StateStorageEnabled { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected sealed override async Task OnActivationSetupState(CancellationToken ct)
        {
            await PullStateAsync(ct);

            await (OnActivationSetupState(this.State, ct) ?? Task.CompletedTask);
        }

        /// <inheritdoc />
        public sealed async override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (this._state != null)
                await PushStateAsync(cancellationToken);

            await base.OnDeactivateAsync(reason, cancellationToken);

            await (OnDeactivateAsync(this._state, reason, cancellationToken) ?? Task.CompletedTask);
        }

        /// <summary>
        /// Loads the state from storage.
        /// </summary>
        protected async ValueTask PullStateAsync(CancellationToken ct)
        {
            if (this.State is not null && this.StateStorageEnabled == false)
                return;

            var state = await OnPullStateAsync(ct);
            this._state = state;
        }

        /// <summary>
        /// Push <paramref name="newState"/> to storage
        /// </summary>
        protected ValueTask PushStateAsync(CancellationToken ct)
        {
            return PushStateAsync(this.State!, ct);
        }

        /// <summary>
        /// Push <paramref name="newState"/> to storage
        /// </summary>
        protected async ValueTask PushStateAsync(TVGrainState newState, CancellationToken ct)
        {
            this._state = newState;

            if (this.StateStorageEnabled == false)
                return;

            await OnPushStateAsync(newState, ct);
        }

        /// <summary>
        /// Called on <see cref="VGrainBase" /> setup.
        /// </summary>
        protected virtual Task OnActivationSetupState(TVGrainState? state, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///  Call after push state on desactivation
        /// </summary>
        /// <inheritdoc cref="OnDeactivateAsync(DeactivationReason, CancellationToken)" />
        protected virtual Task OnDeactivateAsync(TVGrainState? state, DeactivationReason reason, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when state refreshed.
        /// </summary>
        protected virtual Task OnStateRefreshedAsync(TVGrainState vgrainState)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Loads the state from storage.
        /// </summary>
        protected abstract ValueTask<TVGrainState> OnPullStateAsync(CancellationToken ct);

        /// <summary>
        /// Push <paramref name="newState"/> to storage
        /// </summary>
        protected abstract ValueTask OnPushStateAsync(TVGrainState newState, CancellationToken ct);

        #endregion
    }

    /// <summary>
    /// Base implementation of <see cref="IVGrain"/> with a specific <typeparamref name="TVGrainState"/>
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="IVGrain" />
    /// <remarks>
    ///     This base class must be used instead of <see cref="Grain"/> to allow vgrain system (orleans) to be replace more easily if needed
    /// </remarks>
    public abstract class VGrainBase<TVGrainState, TVGrainInterface> : VGrainStatedBase<TVGrainState, TVGrainInterface>, ILifecycleParticipant<IGrainLifecycle>
        where TVGrainInterface : IVGrain
        where TVGrainState : class
    {
        #region Fields

        private readonly IPersistentState<TVGrainState> _persistentState;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainBase"/> class.
        /// </summary>
        protected VGrainBase(ILogger<TVGrainInterface> logger,
                             IPersistentState<TVGrainState> persistentState, 
                             bool stateStorageEnabled = true)
            : base(logger, stateStorageEnabled)
        {
            this._persistentState = persistentState;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the state from storage.
        /// </summary>
        protected override async ValueTask<TVGrainState> OnPullStateAsync(CancellationToken ct)
        {
            if (this._persistentState == null)
                return this.State!;

            await this._persistentState.ReadStateAsync();
            ct.ThrowIfCancellationRequested();

            var state = this._persistentState.State;
            await OnStateRefreshedAsync(state);

            return state;
        }

        /// <summary>
        /// Push <paramref name="newState"/> to storage
        /// </summary>
        protected override async ValueTask OnPushStateAsync(TVGrainState newState, CancellationToken ct)
        {
            if (this._persistentState == null || this.StateStorageEnabled == false)
                return;

            this._persistentState.State = newState;
            await this._persistentState.WriteStateAsync();
        }

        #endregion
    }

    /// <summary>
    /// Base implementation of <see cref="IVGrain"/> with a specific <typeparamref name="TVGrainState"/> using <typeparamref name="TStateSurrogate"/> as carry on save and restore
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="IVGrain" />
    /// <remarks>
    ///     This base class must be used instead of <see cref="Grain"/> to allow vgrain system (orleans) to be replace more easily if needed
    /// </remarks>
    public abstract class VGrainBase<TVGrainState, TStateSurrogate, TConverter, TVGrainInterface> : VGrainStatedBase<TVGrainState, TVGrainInterface>, ILifecycleParticipant<IGrainLifecycle>
        where TStateSurrogate : struct
        where TVGrainState : class
        where TConverter : class, IConverter<TVGrainState, TStateSurrogate>
        where TVGrainInterface : IVGrain
    {
        #region Fields

        private static readonly TConverter? s_converter;
        private TConverter? _localStateConverter;

        private readonly IPersistentState<TStateSurrogate> _persistentState;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainBase{TVGrainState, TStateSurrogate, TConverter}"/> class.
        /// </summary>
        static VGrainBase()
        {
            var converterTraits = typeof(TConverter);
            var canConverterDefaultBuild = converterTraits.IsAbstract == false &&
                                           converterTraits.GetConstructors().Length == 1 &&
                                           converterTraits.GetConstructors().Single().GetParameters().Length == 0;

            if (canConverterDefaultBuild)
                s_converter = Activator.CreateInstance<TConverter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainBase"/> class.
        /// </summary>
        protected VGrainBase(ILogger<TVGrainInterface> logger,
                             IPersistentState<TStateSurrogate> persistentState, 
                             bool stateStorageEnabled = true)
            : base(logger, stateStorageEnabled)
        {
            this._persistentState = persistentState;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the state from storage.
        /// </summary>
        protected override async ValueTask<TVGrainState> OnPullStateAsync(CancellationToken ct)
        {
            if (this._persistentState == null)
                return this.State!;

            await this._persistentState.ReadStateAsync();
            ct.ThrowIfCancellationRequested();

            var cnv = GetStateConverter();
            var state = cnv.ConvertFromSurrogate(this._persistentState.State);

            await OnStateRefreshedAsync(state);

            return state;
        }

        /// <summary>
        /// Push <paramref name="newState"/> to storage
        /// </summary>
        protected override async ValueTask OnPushStateAsync(TVGrainState newState, CancellationToken ct)
        {
            if (this._persistentState == null)
                return;

            var converter = GetStateConverter();
            this._persistentState.State = converter.ConvertToSurrogate(newState);

            await this._persistentState.WriteStateAsync();
        }

        /// <summary>
        /// Gets the state converter.
        /// </summary>
        protected virtual TConverter GetStateConverter()
        {
            if (s_converter is not null)
                return s_converter;

            if (this._localStateConverter is null)
                this._localStateConverter = ActivatorUtilities.CreateInstance<TConverter>(base.ServiceProvider);

            return this._localStateConverter;
        }

        #endregion
    }
}
