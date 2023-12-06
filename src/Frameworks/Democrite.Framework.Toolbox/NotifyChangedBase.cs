// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox
{
    using Democrite.Framework.Toolbox.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Abstractions.Proxies;
    using Democrite.Framework.Toolbox.Proxies;

    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Implement pattern <see cref="INotifyPropertyChanged"/>
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class NotifyChangedBase : INotifyPropertyChanged
    {
        #region Fields

        private readonly static Dictionary<string, PropertyChangedEventArgs> s_eventArgsIndexed;
        private readonly static ReaderWriterLockSlim s_cacheLocker;

        private readonly Func<string?, PropertyChangedEventArgs> _propertyChangedBuilder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NotifyChangedBase"/> class.
        /// </summary>
        static NotifyChangedBase()
        {
            s_eventArgsIndexed = new Dictionary<string, PropertyChangedEventArgs>();
            s_cacheLocker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyChangedBase"/> class.
        /// </summary>
        /// 
        /// <param name="dispatcherProxyStore">Store that MUST contains a <see cref="IDispatcherProxy"/> associate to key <paramref name="executionType"/></param>
        /// <param name="executionType">Type of the <see cref="IDispatcherProxy"/> needed</param>
        /// <param name="useEventArgCache"><c>True</c>the notification will use a static cache to reduce memory grows, GC ... but slower</param>
        /// 
        /// <exception cref="InvalidOperationException">Raised if the <paramref name="dispatcherProxyStore"/> doesn't have <see cref="IDispatcherProxy"/> associate to <paramref name="executionType"/> </exception>
        protected NotifyChangedBase(IDispatcherProxyStore dispatcherProxyStore,
                                    DispatcherTypeEnum executionType,
                                    bool useEventArgCache = true)
            : this(useEventArgCache)
        {
            this.Dispatcher = dispatcherProxyStore.GetRequiredDispatcherProxy(executionType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyChangedBase"/> class.
        /// </summary>
        /// <param name="useEventArgCache"><c>True</c>the notification will use a static cache to reduce memory grows, GC ... but slower</param>
        protected NotifyChangedBase(bool useEventArgCache = true)
        {
            this.Dispatcher = NullDispatcherProxy.Instance;

            this._propertyChangedBuilder = DefaultPropertyChangedBuilder;

            if (useEventArgCache)
                this._propertyChangedBuilder = CachedPropertyChangedBuilder;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        public IDispatcherProxy Dispatcher { get; }

        #endregion

        #region Events

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Update <paramref name="field"/> value by <paramref name="newValue"/> if different and raised the event <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool SetProperty<TValue>(ref TValue field,
                                           in TValue newValue,
                                           [CallerMemberName] string? callerMemberName = null)
        {
            return SetProperty(ref field,
                               newValue,
                               (Action<TValue, TValue>?)null,
                               false,
                               callerMemberName);
        }

        /// <summary>
        /// Update <paramref name="field"/> value by <paramref name="newValue"/> if different and raised the event <see cref="INotifyPropertyChanged.PropertyChanged"/> in <see cref="IDispatcherProxy"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool SetProperty<TValue>(ref TValue field,
                                           in TValue newValue,
                                           bool raiseInDispatcher,
                                           [CallerMemberName] string? callerMemberName = null)
        {
            return SetProperty(ref field,
                               newValue,
                               (Action<TValue, TValue>?)null,
                               raiseInDispatcher,
                               callerMemberName);
        }

        /// <summary>
        /// Update <paramref name="field"/> value by <paramref name="newValue"/> if different and raised the event <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        protected bool SetProperty<TValue>(ref TValue field,
                                           in TValue newValue,
                                           Action<TValue, TValue>? otherDispatchedAction,
                                           [CallerMemberName] string? callerMemberName = null)
        {
            return SetProperty(ref field,
                               newValue,
                               otherDispatchedAction,
                               false,
                               callerMemberName);
        }

        /// <summary>
        /// Update <paramref name="field"/> value by <paramref name="newValue"/> if different and raised the event <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        protected bool SetProperty<TValue>(ref TValue field,
                                           in TValue newValue,
                                           Action? otherDispatchedAction,
                                           bool raiseInDispatcher = false,
                                           [CallerMemberName] string? callerMemberName = null)
        {
            return SetProperty(ref field,
                               newValue,
                               (o, n) => otherDispatchedAction?.Invoke(),
                               raiseInDispatcher,
                               callerMemberName);
        }

        /// <summary>
        /// Update <paramref name="field"/> value by <paramref name="newValue"/> if different and raised the event <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        protected bool SetProperty<TValue>(ref TValue field,
                                           in TValue newValue,
                                           Action<TValue, TValue>? otherDispatchedAction,
                                           bool raiseInDispatcher,
                                           [CallerMemberName] string? callerMemberName = null)
        {
            if (EqualityComparer<TValue>.Default.Equals(field, newValue))
                return false;

            var old = field;
            var cpy = newValue;
            field = newValue;

            if (raiseInDispatcher)
            {
                this.Dispatcher.Send(() =>
                {
                    RaisePropertyChanged(callerMemberName);
                    otherDispatchedAction?.Invoke(old, cpy);
                });
            }
            else
            {
                RaisePropertyChanged(callerMemberName);
                otherDispatchedAction?.Invoke(old, newValue);
            }

            return true;
        }

        /// <summary>
        /// Raises the event <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        protected void RaisePropertyChanged(string? callerMemberName)
        {
            var args = this._propertyChangedBuilder(callerMemberName);
            PropertyChanged?.Invoke(this, args);
        }

        #region Tools

        /// <summary>
        /// <see cref="PropertyChangedEventArgs"/> builder using cached instances
        /// </summary>
        private static PropertyChangedEventArgs CachedPropertyChangedBuilder(string? arg)
        {
            arg ??= string.Empty;

            s_cacheLocker.EnterReadLock();
            try
            {
                if (s_eventArgsIndexed.TryGetValue(arg, out var propertyChangedArg))
                    return propertyChangedArg;
            }
            finally
            {
                s_cacheLocker.ExitReadLock();
            }

            s_cacheLocker.EnterWriteLock();
            try
            {
                // Retry in case other thread have create the value between test above and locker grant access
                if (s_eventArgsIndexed.TryGetValue(arg, out var propertyChangedArg))
                    return propertyChangedArg;

                var newPropertyChangedArg = new PropertyChangedEventArgs(arg);
                s_eventArgsIndexed.Add(arg, newPropertyChangedArg);

                return newPropertyChangedArg;
            }
            finally
            {
                s_cacheLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Defaults the <see cref="PropertyChangedEventArgs"/> builder that instanciate on each request
        /// </summary>
        private static PropertyChangedEventArgs DefaultPropertyChangedBuilder(string? arg)
        {
            return new PropertyChangedEventArgs(arg ?? string.Empty);
        }

        #endregion

        #endregion
    }
}
