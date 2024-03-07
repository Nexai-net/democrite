// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Patterns.Workers
{
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Pipeline that process task in parallel witha limit of quantity
    /// </summary>
    /// <seealso cref=" SafeAsyncDisposable" />
    public sealed class TicketProcessPipeline : SafeAsyncDisposable
    {
        #region Fields

        private readonly CancellationTokenSource _lifecycleCancellationTokenSource;

        private long _usedTicket;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketProcessPipeline"/> class.
        /// </summary>
        public TicketProcessPipeline()
        {
            this._lifecycleCancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes <paramref name="func"/> with a limit of <paramref name="maxticket"/> concurrent execution
        /// </summary>
        public async Task ProcessAsync<TInfo>(Func<Task> func, Func<TInfo?> errorInfoIdentity, uint maxticket, ILogger logger)
        {
            while (this._lifecycleCancellationTokenSource.IsCancellationRequested == false)
            {
                if (Interlocked.Read(ref this._usedTicket) < maxticket)
                {
                    Interlocked.Increment(ref this._usedTicket);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            await func();
                        }
                        catch (Exception ex)
                        {
                            logger.OptiLog(LogLevel.Error, "Faile execute task from {errorInfoIdentity} -> {exception}", errorInfoIdentity(), ex);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref this._usedTicket);
                        }
                    }).Unwrap().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    break;
                }
                this._lifecycleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                await Task.Delay(100, this._lifecycleCancellationTokenSource.Token);
            }

            this._lifecycleCancellationTokenSource.Token.ThrowIfCancellationRequested();
        }

        /// <inheritdoc />
        protected override ValueTask DisposeBeginAsync()
        {
            this._lifecycleCancellationTokenSource.Cancel();
            return base.DisposeBeginAsync();
        }

        #endregion
    }
}
