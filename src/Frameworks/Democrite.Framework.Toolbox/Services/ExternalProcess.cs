// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Services
{
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Disposables;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Proxy pilot about a external execution process
    /// </summary>
    /// <seealso cref="IExternalProcess" />
    internal sealed class ExternalProcess : SafeDisposable, IExternalProcess
    {
        #region Fields

        private readonly CancellationToken _cancellationToken;
        private readonly ProcessStartInfo _info;

        private readonly Queue<string> _standardOutputLogger;
        private readonly Subject<string> _standardOutput;

        private readonly Queue<string> _errorOutputLogger;
        private readonly Subject<string> _errorOutput;

        private Process? _process;
        private Task? _processTask;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalProcess"/> class.
        /// </summary>
        public ExternalProcess(ProcessStartInfo info,
                               IReadOnlyCollection<string> arguments,
                               CancellationToken cancellationToken)
        {
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;

            this._info = info;
            this._cancellationToken = cancellationToken;

            this.Arguments = arguments;

            // Standard output
            this._standardOutputLogger = new Queue<string>();
            this._standardOutput = new Subject<string>();
            RegisterDisposableDependency(this._standardOutput);

            var defaultRecordToken = this._standardOutput.Subscribe((n) => this._standardOutputLogger.Enqueue(n));
            RegisterDisposableDependency(defaultRecordToken);

            var standardOutputConnectObservable = this._standardOutput.ObserveOn(TaskPoolScheduler.Default)
                                                                      .Publish();

            var standardOutputConnectToken = standardOutputConnectObservable.Connect();
            RegisterDisposableDependency(standardOutputConnectToken);

            this.StandardOutputObservable = standardOutputConnectObservable;

            // Error output
            this._errorOutputLogger = new Queue<string>();

            this._errorOutput = new Subject<string>();
            RegisterDisposableDependency(this._errorOutput);

            var defaultErrorRecordToken = this._errorOutput.Subscribe((n) => this._errorOutputLogger.Enqueue(n));
            RegisterDisposableDependency(defaultErrorRecordToken);

            var errorOutputConnectObservable = this._errorOutput.ObserveOn(TaskPoolScheduler.Default)
                                                                   .Publish();

            var errorOutputConnectToken = errorOutputConnectObservable.Connect();
            RegisterDisposableDependency(errorOutputConnectToken);

            this.ErrorOutputObservable = errorOutputConnectObservable;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string Executable
        {
            get { return this._info.FileName; }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Arguments { get; }


        /// <inheritdoc />
        public IObservable<string> StandardOutputObservable { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<string> StandardOutput
        {
            get { return this._standardOutputLogger; }
        }

        /// <inheritdoc />
        public IObservable<string> ErrorOutputObservable { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<string> ErrorOutput
        {
            get { return this._errorOutputLogger; }
        }

        /// <inheritdoc />
        public int? ExitCode { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Start configured process
        /// </summary>
        public Task RunAsync()
        {
            try
            {
                lock (this._info)
                {
                    if (this._process != null)
                        throw new InvalidOperationException("Could not start a running process");

                    this._process = Process.Start(this._info);

                    if (this._process == null)
                    {
                        throw new InvalidOperationException("Process information are invalid");
                    }

                    //this._process.Exited += Process_Exited;
                    this._process.OutputDataReceived += Process_OutputDataReceived;
                    this._process.ErrorDataReceived += Process_ErrorDataReceived;

                    var processTask = this._process.WaitForExitAsync(this._cancellationToken);

                    this._processTask = processTask.ContinueWith(t =>
                                                   {
                                                       this.ExitCode = this._process.ExitCode;
                                                       this._process = null;
                                                   });

                    this._process.EnableRaisingEvents = true;

                    if (!this._process.Start())
                    {
                        throw new InvalidOperationException("Process couldn't start");
                    }

                    this._process.BeginOutputReadLine();
                    this._process.BeginErrorReadLine();
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(ProcessStartInfo), this._info);
                throw;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task GetAwaiterTask()
        {
            lock (this._info)
            {
                return this._processTask ?? throw new InvalidOperationException("Run process first");
            }
        }

        /// <inheritdoc />
        public Task KillAsync(CancellationToken cancellationToken)
        {
            Process? process;
            lock (this._info)
            {
                process = this._process;
            }

            if (process == null)
                throw new InvalidOperationException("Run process first");

            return Task.Run(() => process.Kill(true), cancellationToken);
        }

        /// <summary>
        /// Handles the OutputDataReceived event of the Process control.
        /// </summary>
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            this._standardOutput.OnNext(e.Data);
        }

        /// <summary>
        /// Handles the ErrorDataReceived event of the Process control.
        /// </summary>
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            this._errorOutput.OnNext(e.Data);
        }

        /// <summary>
        /// Used to kill dependency process
        /// </summary>
        protected override void DisposeBegin()
        {
            lock (this._info)
            {
                this._process?.Kill(true);
            }

            base.DisposeBegin();
        }

        #endregion
    }
}
